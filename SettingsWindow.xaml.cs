using Microsoft.SqlServer.Server;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace SimpleRA3Launcher
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool allowClose = false;
        public bool ShowUn60FPSWarning = true;
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
            if (_owner.ShowCrackWarning)
            {
                MessageBox.Show("启动游戏时，将替换游戏文件为学习版！请做好备份。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                _owner.ShowCrackWarning = false;
            }
        }
        private void checkBox2_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_owner.ShowUnCrackWarning)
            {
                MessageBox.Show("将不再替换游戏文件。但如果文件已经替换，则需要手动恢复！（文件：\"红警3安装目录\\Data\\ra3_1.12.game\"）", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                _owner.ShowUnCrackWarning = false;
            }
        }


        private void checkBox3_Checked(object sender, RoutedEventArgs e)
        {
            checkBox2.IsChecked = true;
            checkBox2.IsEnabled = false;
            //if (_owner.Show60FPSWarning)
            //{
            //    MessageBox.Show("将在启动游戏时生效，也会对其他MOD生效。", "提示", MessageBoxButton.OK, MessageBoxImage.Question);
            //    _owner.Show60FPSWarning = false;
            //}
        }

        private void checkBox3_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBox2.IsEnabled = true;
            if (ShowUn60FPSWarning)
            {
                MessageBox.Show("将解除60FPS游戏模式！需要启动一次游戏才会生效。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                ShowUn60FPSWarning = false;
            }
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

        private void checkBox4_Checked(object sender, RoutedEventArgs e)
        {
            stackPanel1.IsEnabled = true;
            if (textBox3.Text == "")
            {
                textBox3.Text = "1920";
            }
            if (textBox4.Text == "")
            {
                textBox4.Text = "1080";
            }
            if (textBox1.Text != "")
            {
                // 删除旧的手动指定的分辨率参数
                string pattern = @"-win\s*|-xres\s*\d+\s*|-yres\s*\d+\s*";
                string result = Regex.Replace(textBox1.Text, pattern, "").Trim();
                textBox1.Text = result;
            }
        }

        private void checkBox4_Unchecked(object sender, RoutedEventArgs e)
        {
            stackPanel1.IsEnabled = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mr = MessageBox.Show("将删除Skirmish.ini文件，会重置遭遇战的游戏选项。建议仅在遇到地图列表消失问题时进行此操作！（在不同MOD之间切换时，可能会遇到该问题。）", "确认操作", MessageBoxButton.OKCancel);
            if (mr == MessageBoxResult.OK){
                string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string profiledir = Path.Combine(roamingPath, "Red Alert 3", "Profiles");
                try
                {
                    bool deleted = false;
                    foreach (string subFolderPath in Directory.GetDirectories(profiledir))
                    {
                        foreach (string filePath in Directory.GetFiles(subFolderPath))
                        {
                            string fileName = Path.GetFileName(filePath);
                            if (fileName.Equals("Skirmish.ini", StringComparison.OrdinalIgnoreCase))
                            {
                                // 如果文件名匹配，则删除文件
                                File.Delete(filePath);
                                deleted = true;
                            }
                        }
                    }

                    if (deleted)
                    {
                        MessageBox.Show("删除成功！");
                    }
                    else
                    {
                        MessageBox.Show("未找到Skirmish.ini文件！");
                    }
                }
                catch
                {
                    MessageBox.Show("删除失败！");
                }
            }
        }
    }
}
