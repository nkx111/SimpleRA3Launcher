using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
//using static System.Net.WebRequestMethods;

namespace SimpleRA3Launcher
{
    
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string GamePath = null;            // full path
        public string GameSkudefFileName = null;  // full name

        public List<string> ModNames = new List<string>();
        public List<string> ModSkudefFileNames = new List<string>();

        SettingsWindow settingsWindow = null;
        //LauncherOptionsWindow launcherOptionsWindow = null;

        public bool ShowCrackWarning = true;
        public bool ShowUnCrackWarning = false;

        private Process CheaterProcess = null;
        private IntPtr GameProcessHandle = IntPtr.Zero;

        private const string OriginalGame_ModName = "__original";

        public MainWindow()
        {
            InitializeComponent();
            settingsWindow = new SettingsWindow(this);
        }

        private void FindMods()
        {
            // 首先在当前exe的文件夹下查找mod
            string rootPath = Directory.GetCurrentDirectory();
            DirectoryInfo root = new DirectoryInfo(rootPath);
            foreach (var file in root.GetFiles())
            {
                if (file.Extension.ToUpper() == ".SKUDEF")
                {
                    var _name = ValidateSkuDef(file);
                    if (_name != "")
                    {
                        ModNames.Add(_name);
                        ModSkudefFileNames.Add(file.FullName);
                    }

                }
            }

            // 再在我的文档的目录下寻找mod
            var mydocfolderpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var commonModFolderPath = System.IO.Path.Combine(mydocfolderpath, "Red Alert 3", "Mods");
            DirectoryInfo mod_root = new DirectoryInfo(commonModFolderPath);
            foreach (var subdir in mod_root.GetDirectories())
            {
                foreach (var file in subdir.GetFiles())
                {
                    if (file.Extension.ToUpper() == ".SKUDEF")
                    {
                        var _name = ValidateSkuDef(file);
                        if (_name != "")
                        {
                            if (!ModSkudefFileNames.Contains(file.FullName))
                            {
                                if (ModNames.Contains(_name))
                                {
                                    ModNames.Add(_name + "(Mods/" + subdir.Name + ")");
                                }
                                else
                                {
                                    ModNames.Add(_name);
                                }
                                ModSkudefFileNames.Add(file.FullName);
                            }
                        }
                    }
                }
            }

            // 最下面添加原版启动选项
            ModNames.Add("原版");
            ModSkudefFileNames.Add(OriginalGame_ModName);


            foreach (var _mod in ModNames)
            {
                TransparentComboBox.Items.Add(_mod);
            }
        }


        private void SelectMod()
        {
            RegistryKey user = Registry.CurrentUser;
            RegistryKey rk = user.OpenSubKey("SOFTWARE\\RA3ModLauncher");
            if (rk != null)
            {
                var Preferred_Mod_Path = (string)rk.GetValue("preferred_mod_path");
                if (Preferred_Mod_Path != null && ModSkudefFileNames.Contains(Preferred_Mod_Path))
                {
                    TransparentComboBox.SelectedIndex = ModSkudefFileNames.IndexOf(Preferred_Mod_Path);
                    TransparentComboBox_SelectionChanged(null, null);
                    return;
                }
            }
            TransparentComboBox.SelectedIndex = 0;
            TransparentComboBox_SelectionChanged(null, null);
        }

        // Returns the mod name if the skudef file is valid.
        // Returns "" otherwise
        private string ValidateSkuDef(FileInfo _modSkuDefFile)
        {
            var reader = new StreamReader(_modSkuDefFile.FullName);
            bool mod = false;
            List<string> bigFiles = new List<string>();

            string line = reader.ReadLine();
            while (line != null)
            {
                if (line.StartsWith("mod-game"))
                {
                    // this is not a valid mod skudef
                    mod = true;
                }
                if (line.StartsWith("add-big"))
                {
                    bigFiles.Add(line.Substring(8));
                }

                line = reader.ReadLine();
            }

            reader.Close();

            if (!mod)
            {
                return "";
            }

            FileInfo[] files = new DirectoryInfo(_modSkuDefFile.DirectoryName).GetFiles();

            foreach (var bigFile in bigFiles)
            {
                bool bigFile_Found = false;
                foreach (var file in files)
                {
                    if (file.Name == bigFile)
                    {
                        bigFile_Found = true;
                    }
                }
                if (!bigFile_Found)
                {
                    //MessageBox.Show("problemtic mod file! skudef and big file unmatched!");
                    //label2.Content = "请检查skudef文件";
                    return "";
                }
            }

            var _ModName = _modSkuDefFile.Name.Substring(0, _modSkuDefFile.Name.IndexOf(_modSkuDefFile.Extension));
            return _ModName;
        }

        public bool VerifyGameInPath(string path, bool message = false)
        {
            if (path == null || !Directory.Exists(path))
            {
                if (message)
                {
                    MessageBox.Show("RA3 game not found! Invalid path!");
                }
                return false;
            }
            // make sure it is a valid ra3 path
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles();
            string _GameSkudef = null;
            foreach (var file in files)
            {
                if (file.Name.Contains("1.12.SkuDef"))
                {
                    _GameSkudef = file.FullName;
                }
            }
            if (_GameSkudef == null)
            {
                if (message)
                {
                    MessageBox.Show("RA3 game not found! Lacks Skudef");
                }
                return false;
            }


            bool has112game = false;
            root = new DirectoryInfo(path + "\\Data");
            if (root.Exists)
            {
                files = root.GetFiles();
                foreach (var file in files)
                {
                    if (file.Name.Contains("ra3_1.12.game"))
                    {
                        has112game = true;
                    }
                }
            }
            if (!has112game)
            {
                if (message)
                {
                    MessageBox.Show("RA3 game not found! Lacks ra3_1.12.game");
                }
                return false;
            }

            GameSkudefFileName = _GameSkudef;
            GamePath = path;
            label4.Content = GamePath;
            settingsWindow.textBox2.Text = GamePath;
            button1.IsEnabled = true;
            return true;
        }

        private bool FindGame()
        {
            // find first in program registry
            RegistryKey user = Registry.CurrentUser;
            RegistryKey rk = user.OpenSubKey("SOFTWARE\\RA3ModLauncher");
            if (rk != null)
            {
                var _GamePath = (string)rk.GetValue("preferred_game_path");
                if (VerifyGameInPath(_GamePath))
                {
                    return true;
                }
            }



            // then find in system registry
            RegistryKey local = Registry.LocalMachine;
            rk = local.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\RA3.exe");
            if (rk != null)
            {
                var _GamePath = (string)rk.GetValue("path");
                if (VerifyGameInPath(_GamePath))
                {
                    return true;
                }
            }


            rk = local.OpenSubKey("Software\\Electronic Arts\\Electronic Arts\\Red Alert 3");
            if (rk != null)
            {
                var _GamePath = (string)rk.GetValue("Install Dir");
                if (VerifyGameInPath(_GamePath))
                {
                    return true;
                }
            }


            MessageBox.Show("Game not found! Please manually set the path in the Launcher.");
            return false;

        }

        public static int ReadIntRegValue(RegistryKey rk, string para)
        {
            int result = 0;
            var _result = rk.GetValue(para);
            if (_result != null)
            {
                result = (int)_result;
            }
            return result;
        }

        private bool ExtractResource(string resource, string path, string backupname = null)
        {
            string fullResourceName = "";
            foreach (var resouceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (resouceName.Contains(resource))
                {
                    fullResourceName = resouceName;
                    break;
                }
            }
            if (fullResourceName == "")
            {
                return false;
            }


            Stream stream = GetType().Assembly.GetManifestResourceStream(fullResourceName);
            if (stream == null)
            {
                return false;
            }

            if (File.Exists(path))
            {
                long length = new System.IO.FileInfo(path).Length;
                if (length == stream.Length)
                {
                    return true;
                }
                else
                {
                    if (backupname != null)
                    {
                        if (!File.Exists(backupname))
                        {
                            File.Move(path, backupname);
                        }
                        else
                        {
                            try
                            {
                                File.Delete(path);
                            }
                            catch
                            {
                                MessageBox.Show("替换游戏文件失败！");
                                return false;
                            }
                        }
                    }
                    byte[] bytes = new byte[(int)stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    try
                    {
                        File.WriteAllBytes(path, bytes);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

            }
            else
            {
                byte[] bytes = new byte[(int)stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                try
                {
                    File.WriteAllBytes(path, bytes);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // find game, find mod
            if (!FindGame())
            {
                button1.IsEnabled = false;
            }

            FindMods();
            if (ModNames.Count > 0)
            {
                SelectMod();
            }
            else
            {
                button1.Content = "启动原版";
            }


            settingsWindow.Hide();
            //launcherOptionsWindow.Hide();

            // load sys setup
            RegistryKey user = Registry.CurrentUser;
            RegistryKey rk = user.OpenSubKey("SOFTWARE\\RA3ModLauncher");
            if (rk != null)
            {

                //var use_admin = ReadIntRegValue(rk, "use_admin");
                var replace_file = ReadIntRegValue(rk, "replace_file");
                var use_patch = ReadIntRegValue(rk, "use_patch");
                var use_60fps = ReadIntRegValue(rk, "use_60fps");

                if (replace_file == 1)
                {
                    ShowCrackWarning = false;
                    ShowUnCrackWarning = true;
                }

                settingsWindow.checkBox1.IsChecked = replace_file == 1;
                settingsWindow.checkBox2.IsChecked = use_patch == 1;
                settingsWindow.checkBox3.IsChecked = use_60fps == 1;

                var additional_opt = (string)rk.GetValue("additional_opt");
                settingsWindow.textBox1.Text = additional_opt;


            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            settingsWindow.Show();
            settingsWindow.Activate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RegistryKey user = Registry.CurrentUser;
            RegistryKey rk = user.OpenSubKey("SOFTWARE\\RA3ModLauncher", true);
            try
            {
                if (rk == null)
                {
                    rk = user.OpenSubKey("SOFTWARE", true).CreateSubKey("RA3ModLauncher");
                }

                //rk.SetValue("use_admin", settingsWindow.checkBox1.Checked ? 1 : 0);
                rk.SetValue("replace_file", (bool)settingsWindow.checkBox1.IsChecked ? 1 : 0);
                rk.SetValue("use_patch", (bool)settingsWindow.checkBox2.IsChecked ? 1 : 0);
                rk.SetValue("use_60fps", (bool)settingsWindow.checkBox3.IsChecked ? 1 : 0);
                rk.SetValue("additional_opt", settingsWindow.textBox1.Text);
                
                if (settingsWindow.textBox2.Text != "")
                {
                    rk.SetValue("preferred_game_path", settingsWindow.textBox2.Text);
                }
                if (ModSkudefFileNames.Count > 0 && TransparentComboBox.SelectedIndex >= 0 && TransparentComboBox.SelectedIndex < ModSkudefFileNames.Count)
                {
                    rk.SetValue("preferred_mod_path", ModSkudefFileNames[TransparentComboBox.SelectedIndex]);
                }
            }
            catch
            {
            }

            settingsWindow.allowClose = true;
            settingsWindow.Close();
        }


        public void StartMod(string _ModSkudef, string cwd = null)
        {
            var process = new Process();

            // main game
            string game_112_path = GamePath + "\\Data\\ra3_1.12.game";
            string game_112_bk_path = GamePath + "\\Data\\ra3_1.12.game.ModLauncher_OriginalFile.backup";

            process.StartInfo.FileName = game_112_path;
            if ((bool)settingsWindow.checkBox3.IsChecked)
            {
                // use 60FPS patch (already with 4GB)
                ExtractResource("ra3_1.12_60FPS.game", game_112_path, game_112_bk_path);
            }
            else if ((bool)settingsWindow.checkBox2.IsChecked)
            {
                // use 4GB patch
                ExtractResource("ra3_1.12_4GB.game", game_112_path, game_112_bk_path);
            }
            else if ((bool)settingsWindow.checkBox1.IsChecked)
            {
                // use cracked patch
                ExtractResource("ra3_1.12_Original.game", GamePath + "\\Data\\ra3_1.12.game");
            }


            // mod
            var GameSkudef_escape = GameSkudefFileName.Replace("\\", "\\\\");
            var BaseGameArgument = " -config \"" + GameSkudef_escape + "\" ";

            string ModArgument = "";
            if (_ModSkudef != null && _ModSkudef != OriginalGame_ModName)
            {
                var ModSkudef_escape = _ModSkudef.Replace("\\", "\\\\");
                ModArgument = " -modConfig \"" + ModSkudef_escape + "\" ";
            }

            // additional args
            string AdditionalGameArgument = "";
            if (settingsWindow.textBox1.Text != "")
            {
                AdditionalGameArgument = " " + settingsWindow.textBox1.Text;
            }

            string _cwd = GamePath;
            if (cwd != null)
            {
                _cwd = cwd;
            }

            // start game
            process.StartInfo.Arguments = BaseGameArgument + ModArgument + AdditionalGameArgument;
            process.StartInfo.WorkingDirectory = _cwd;
            process.StartInfo.UseShellExecute = false;
            process.EnableRaisingEvents = true;
            // process.Exited += new EventHandler(Process_Exited);

            if (process.Start())
            {
                // GameProcessHandle = Cheat.OpenProcess(Cheat.PROCESS_ALL_ACCESS, false, process.Id);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            StartMod(ModSkudefFileNames[this.TransparentComboBox.SelectedIndex]);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (CheaterProcess == null)
            {
                var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RedAlert3_Trainer_1.12_FINAL3.exe");
                if (ExtractResource("RedAlert3_Trainer_1.12_FINAL3.exe", path))
                {
                    WindowsIdentity identity = WindowsIdentity.GetCurrent();
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    bool isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

                    var process = new Process();
                    process.StartInfo.FileName = path;
                    if (isElevated)
                    {
                        process.StartInfo.UseShellExecute = false;
                    }
                    else
                    {
                        process.StartInfo.UseShellExecute = true;
                    }

                    try
                    {
                        process.Start();
                        CheaterProcess = process;
                        process.EnableRaisingEvents = true;
                        process.Exited += new EventHandler(CheaterProcess_Exited);
                    }
                    catch { }
                }
            }
        }

        private void CheaterProcess_Exited(object sender, EventArgs e)
        {
            Action action = () =>
            {
                CheaterProcess = null;
            };
            Dispatcher.Invoke(action);
        }

        private void TransparentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ModSkudefFileNames[this.TransparentComboBox.SelectedIndex] == OriginalGame_ModName)
            {
                button1.Content = "启动原版";
            }
            else
            {
                button1.Content = "启动 Mod";
            }
        }

        public void OpenFolderInExplorer(string folderPath)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开文件夹: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string mapdir = System.IO.Path.Combine(roamingPath, "Red Alert 3", "Maps");
            if(Directory.Exists(mapdir))
            {
                OpenFolderInExplorer(mapdir);
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            var mydocfolderpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var commonModFolderPath = System.IO.Path.Combine(mydocfolderpath, "Red Alert 3","Mods");
            if (Directory.Exists(commonModFolderPath))
            {
                OpenFolderInExplorer(commonModFolderPath);
            }
        }
    }
}
