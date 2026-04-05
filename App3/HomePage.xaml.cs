using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ProcessBuilder;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Windows.System;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();

            this.Loaded += homepage_loaded;

        }
        private void homepage_loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            LoadInstalledVersions();
        }
        private async void LaunchButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            string name = InputBox.Text;
            LaunchButton.IsEnabled = false;
            try
            {
                var config = ConfigManager.ReadConfig();
                string javaPath = config.JavaPath;
                
                if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
                {
                    MainWindow.Instance.ShowGlobalNotification("错误", "请先去设置页面选择正确的 Java 路径！", InfoBarSeverity.Error);
                    ResetButton();
                    return;
                }
                /*确定目录*/
                string exePath = Environment.ProcessPath!;
                string baseDir = Path.GetDirectoryName(exePath)!;
                string customMinecraftPath = Path.Combine(baseDir, ".minecraft");
                var path = new MinecraftPath(customMinecraftPath);
                var launcher = new MinecraftLauncher(path);
                /*
                 * 启动脚本
                 * 暂未优化
                 */
                var session = MSession.CreateOfflineSession("name");
                string targetVersion = RoleSelector.SelectedItem.ToString()!;
                MainWindow.Instance.ShowGlobalNotification("提示", $"正在启动版本: {targetVersion}，请稍候...", InfoBarSeverity.Informational);
                var launchOption = new MLaunchOption
                {
                    MaximumRamMb = config.MaxRamMb,
                    Session = session,
                    JavaPath = javaPath
                };
                Process process = await launcher.CreateProcessAsync(targetVersion, launchOption);
                process.Start();
                MainWindow.Instance.ShowGlobalNotification("成功", "游戏已成功启动！", InfoBarSeverity.Success);




            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowGlobalNotification("启动失败", ex.Message, InfoBarSeverity.Error);

            }
            finally
            { 
             ResetButton();
            }
        }
        private void ResetButton()
        {
            LaunchButton.Content = "启动游戏";
            LaunchButton.IsEnabled = true;
        }
        private void LoadInstalledVersions()
        {
            string rootPath = Path.GetDirectoryName(Environment.ProcessPath)!;
            string versionsPath = Path.Combine(rootPath, ".minecraft", "versions");
            List<string> installedVersions = new List<string>();

            if (Directory.Exists(versionsPath))
            {
                // 获取 versions 文件夹下的所有子文件夹
                string[] versionFolders = Directory.GetDirectories(versionsPath);

                foreach (string folderPath in versionFolders)
                {
                    // 获取文件夹的名字
                    string versionName = Path.GetFileName(folderPath);

                    // 检查这个文件夹里有没有对应的 .json 图纸文件
                    string jsonFilePath = Path.Combine(folderPath, $"{versionName}.json");

                    if (File.Exists(jsonFilePath))
                    {
                        installedVersions.Add(versionName);
                    }

                }
            }

            RoleSelector.ItemsSource = installedVersions;
            if (installedVersions.Count > 0)
            {
                RoleSelector.SelectedIndex = 0;
            }
            else
            {
                MainWindow.Instance.ShowGlobalNotification("提示", "还没下载任何游戏，请先去下载页面！", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning);
            }
        }

        


    }
}
