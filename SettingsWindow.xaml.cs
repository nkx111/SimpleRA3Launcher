using Microsoft.SqlServer.Server;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace SimpleRA3Launcher
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool allowClose = false;
        private MainWindow _owner;
        public SettingsWindow(MainWindow win)
        {
            InitializeComponent();
            this._owner = win;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!allowClose)
            {
                e.Cancel = true; // 阻止窗口关闭
                this.Hide();     // 隐藏窗口
            }
        }

        private void checkBox2_Checked(object sender, RoutedEventArgs e)
        {
            checkBox1.IsChecked = true;
            checkBox1.IsEnabled = false;
        }
        private void checkBox2_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBox1.IsEnabled = true;
        }


        private void checkBox3_Checked(object sender, RoutedEventArgs e)
        {
            checkBox2.IsChecked = true;
            checkBox2.IsEnabled = false;
        }

        private void checkBox3_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBox2.IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            openFileDlg.Description = "请选择文件夹";
            openFileDlg.SelectedPath = this.textBox2.Text;
            openFileDlg.ShowNewFolderButton = true;

            if (openFileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((MainWindow)_owner).VerifyGameInPath(openFileDlg.SelectedPath, true);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {



        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            if (_owner.ShowCrackWarning)
            {
                MessageBox.Show("下次启动游戏时，将把游戏文件替换为学习版！请做好备份。", "提示", MessageBoxButton.OK, MessageBoxImage.Question);

                _owner.ShowCrackWarning = false;
            }
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_owner.ShowUnCrackWarning)
            {
                MessageBox.Show("如果游戏文件已经替换，则需要手动恢复！", "提示", MessageBoxButton.OK, MessageBoxImage.Question);

                _owner.ShowUnCrackWarning = false;
            }
        }
    }
}
