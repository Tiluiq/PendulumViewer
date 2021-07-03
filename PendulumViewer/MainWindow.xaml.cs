using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Pendulum
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        public Settings settings { get; private set; }

        double angle, angleVelocity, angleAcceleration;
        System.Drawing.Point prevPoint;

        public MainWindow()
        {
            InitializeComponent();

            SetupTimer();

            settings = new Settings(0.002, 0.00002, 0.1, 0.75, "Pendulum.png", true, false);
            angle = settings.defaultAngle;
            angleVelocity = 0.0;
            angleAcceleration = 0.0;

            prevPoint = System.Windows.Forms.Cursor.Position;
        }

        private void SetupTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 17);
            //timer.Tick += new EventHandler(Swing);
            timer.Tick += new EventHandler(GrabMove);
            timer.Start();

            this.Closing += new CancelEventHandler(StopTimer);
        }

        private void StopTimer(object sender, CancelEventArgs e)
        {
            timer.Stop();
        }

        private void Swing(object sender, EventArgs e)
        {
            angleAcceleration = -Math.Sin(angle) * 0.002;
            angleVelocity += angleAcceleration;
            angle += angleVelocity;

            PendulumImageRotate.Angle = angle * 180 / Math.PI;
        }

        double prevTop = double.MinValue;
        double prevLeft = double.MinValue;
        private void GrabMove(object sender, EventArgs e)
        {
            angleAcceleration = -settings.gl * Math.Sin(angle);

            if (double.IsNaN(prevTop) || double.IsNaN(prevLeft))
            {
                prevTop = pendulumWindow.Top;
                prevLeft = pendulumWindow.Left;
            }

            double top = pendulumWindow.Top;
            double left = pendulumWindow.Left;

            double diffTop = top - prevTop;
            double diffLeft = left - prevLeft;
            double diffSize = Math.Sqrt(diffTop * diffTop + diffLeft * diffLeft);
            double diffAngle = Math.Atan2(diffTop, diffLeft);

            if (diffSize > 0.0 && !double.IsInfinity(diffSize))
            {
                double rotateForceAngle = (PendulumImageRotate.Angle / 180 * Math.PI) - (diffAngle - Math.PI * 0.5);
                double rotateForce = diffSize * Math.Sin(rotateForceAngle);

                angleAcceleration += rotateForce * settings.forceRate;
            }

            //元に戻す方向
            double k = 0.5 * angleVelocity * angleVelocity;
            double u = settings.gl * (1 - Math.Cos(angle));
            double defaultEnergy = settings.gl * (1 - Math.Cos(settings.defaultAngle));
            if (Mouse.LeftButton == MouseButtonState.Released)  //掴んでいないときだけ
                angleAcceleration += (defaultEnergy - (k + u)) * settings.restoringForce * (1.0 - Math.Abs(angle / Math.PI)) * Math.Sign(angleVelocity);

            angleVelocity += angleAcceleration;
            angle += angleVelocity;
            PendulumImageRotate.Angle = angle * 180 / Math.PI;

            if (angle > Math.PI)
                angle -= 2 * Math.PI;
            if (angle < -Math.PI)
                angle += 2 * Math.PI;

            //デバッグ時ラベル更新
            if (settings.isDebug)
            {
                prevTopLabel.Content = "prevTop: " + prevTop;
                prevLeftLabel.Content = "prevLeft: " + prevLeft;

                currentTopLabel.Content = "currentTop: " + top;
                currentLeftLabel.Content = "currentLeft: " + left;

                diffTopLabel.Content = "diffTop: " + diffTop;
                diffLeftLabel.Content = "diffLeft: " + diffLeft;
                diffSizeLabel.Content = "diffSize: " + diffSize;
                diffAngleLabel.Content = "diffAngle: " + diffAngle;

                kLabel.Content = "k: " + k.ToString("F8");
                uLabel.Content = "u: " + u.ToString("F8");
                energyLabel.Content = "energy: " + (k + u).ToString("F8");
                defaultEnergyLabel.Content = "defaultEnergy: " + defaultEnergy.ToString("F8");

                angleLabel.Content = "angle: " + angle.ToString("F8");
            }


            prevTop = top;
            prevLeft = left;

        }

        bool isDrag = false;
        private void PendulumImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDrag = true;
            ((UIElement)sender).CaptureMouse();
            prevPoint = System.Windows.Forms.Cursor.Position;
        }

        private void MouseTest(object sender, EventArgs e)
        {

            mousePointLabel.Content = "mousePoint: "  + System.Windows.Forms.Cursor.Position + ", "
                + ((System.Windows.Forms.Control.MouseButtons & System.Windows.Forms.MouseButtons.Left)== System.Windows.Forms.MouseButtons.Left);
            System.Drawing.Point point = System.Windows.Forms.Cursor.Position;

            if ((System.Windows.Forms.Control.MouseButtons & System.Windows.Forms.MouseButtons.Left) != System.Windows.Forms.MouseButtons.Left)
                isDrag = false;

            if (isDrag)
            {
                int diffX = point.X - prevPoint.X;
                int diffY = point.Y - prevPoint.Y;
                pendulumWindow.Top += diffY;
                pendulumWindow.Left += diffX;
            }
            prevPoint = point;

        }

        private void PendulumImage_MouseMove(object sender, MouseEventArgs e)
        {
            System.Drawing.Point point = System.Windows.Forms.Cursor.Position;

            if (settings.isDebug)
                mousePointLabel.Content = "mousePoint: " + System.Windows.Forms.Cursor.Position;

            if ((System.Windows.Forms.Control.MouseButtons & System.Windows.Forms.MouseButtons.Left) != System.Windows.Forms.MouseButtons.Left) //掴みながら右クリック押したときの誤動作低減用
                isDrag = false;

            if (isDrag)
            {
                if (settings.isDebug)
                    mousePointLabel.Content += ", captured";
                int diffX = point.X - prevPoint.X;
                int diffY = point.Y - prevPoint.Y;
                pendulumWindow.Top += diffY;
                pendulumWindow.Left += diffX;
            }
            prevPoint = point;
        }

        private void PendulumImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrag = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }

        private void SettingMenu_Click(object sender, EventArgs e)
        {
            SettingWindow sw = new SettingWindow(this);
            sw.ShowDialog();
            sw.Close();
        }

        public void SetSettings(Settings settings)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(settings.fileName, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                PendulumImage.Source = bitmap;

                PendulumImage.Height = bitmap.PixelHeight;
                PendulumImage.Width = bitmap.PixelWidth;
                PendulumImageRotate.CenterY = bitmap.PixelHeight / 2;
                PendulumImageRotate.CenterX = bitmap.PixelWidth / 2;

                //ウィンドウサイズ
                this.Height = this.Width = Math.Sqrt(PendulumImage.Height * PendulumImage.Height + PendulumImage.Width * PendulumImage.Width);
            }
            catch (Exception e)
            {
                MessageBox.Show("ファイル\"" + settings.fileName + "\"が開けませんでした\n" + e);
                return;
            }

            //最前面表示設定
            this.Topmost = settings.isTopMost;

            //開発時のラベル表示
            Label[] labels = { prevTopLabel, prevLeftLabel, currentTopLabel, currentLeftLabel, diffTopLabel, diffLeftLabel, diffSizeLabel, diffAngleLabel,
                kLabel, uLabel, energyLabel, defaultEnergyLabel, angleLabel, mousePointLabel };
            if (this.settings.isDebug != settings.isDebug)
            {
                Visibility v = settings.isDebug ? Visibility.Visible : Visibility.Hidden;
                foreach (Label label in labels)
                    label.Visibility = v;
            }

            this.settings = settings;
            angle = this.settings.defaultAngle; //リセット
            angleVelocity = 0.0;    //リセット
            angleAcceleration = 0.0;    //リセット
        }

        private void FinishMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }

    public struct Settings
    {
        public double gl;
        public double forceRate;
        public double restoringForce;
        public double defaultAngle;
        public string fileName;
        public bool isTopMost;
        public bool isDebug;

        public Settings(double gl, double forceRate, double restoringForce, double defaultAngle, string fileName, bool isTopMost, bool isDebug)
        {
            this.gl = gl;
            this.forceRate = forceRate;
            this.restoringForce = restoringForce;
            this.defaultAngle = defaultAngle;
            this.fileName = fileName;
            this.isTopMost = isTopMost;
            this.isDebug = isDebug;
        }

        public Settings(Settings settings) : this(settings.gl, settings.forceRate, settings.restoringForce, settings.defaultAngle, settings.fileName, settings.isTopMost, settings.isDebug) { }
    }
}
