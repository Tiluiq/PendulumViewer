using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pendulum
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        MainWindow parent; 
        public SettingWindow(MainWindow parent)
        {
            InitializeComponent();

            this.parent = parent;

            GLTextBox.Text = parent.settings.gl.ToString("F8");
            ForceRateTextBox.Text = parent.settings.forceRate.ToString("F8");
            RestoringForceTextBox.Text = parent.settings.restoringForce.ToString("F8");
            AngleTextBox.Text = parent.settings.defaultAngle.ToString("F8");
            FileTextBox.Text = parent.settings.fileName;
            TopMostCheckBox.IsChecked = parent.settings.isTopMost;
            DebugCheckBox.IsChecked = parent.settings.isDebug;
        }

        private void FileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "ファイル選択";
            openFileDialog.Filter = "PNGファイル(*.png)|*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                FileTextBox.Text = openFileDialog.FileName;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            double gl;
            if (!double.TryParse(GLTextBox.Text, out gl))
            {
                MessageBox.Show("\"揺れやすさ\"の値が読み取れませんでした");
                return;
            }
            double forceRate;
            if (!double.TryParse(ForceRateTextBox.Text, out forceRate))
            {
                MessageBox.Show("\"移動による力の大きさ\"の値が読み取れませんでした");
                return;
            }
            double restoringForce;
            if (!double.TryParse(RestoringForceTextBox.Text, out restoringForce))
            {
                MessageBox.Show("\"元の運動に戻す力\"の値が読み取れませんでした");
                return;
            }
            if (restoringForce < 0.0)
            {
                MessageBox.Show("\"元の運動に戻す力\"は0以上にしてください");
                return;
            }
            double angle;
            if (!double.TryParse(AngleTextBox.Text, out angle))
            {
                MessageBox.Show("\"初期角度(ラジアン)\"の値が読み取れませんでした");
                return;
            }
            string fileName = FileTextBox.Text;
            if (!File.Exists(fileName))
            {
                MessageBox.Show("ファイル\"" + fileName + "\"が見つかりません");
                return;
            }
            bool isTopMost = (bool)TopMostCheckBox.IsChecked;
            bool isDebug = (bool)DebugCheckBox.IsChecked;

            Settings settings = new Settings(gl, forceRate, restoringForce, angle, fileName, isTopMost, isDebug);
            parent.SetSettings(settings);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
