using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; } = null!;
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            InitializeFolders();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(titlebar);
            ContentFrame.Navigate(typeof(HomePage));
            NavView.SelectedItem = HomeMenu;
        }
        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var clicked_item_tag = args.InvokedItemContainer.Tag.ToString();
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
                return;
            }
            switch (clicked_item_tag)
            {
                case "home":
                    ContentFrame.Navigate(typeof(HomePage));
                    break;

                case "install":
                    ContentFrame.Navigate(typeof(InstallPage));
                    break;

                default: 
                    

                    break;
            
            
            }


        }
        private void InitializeFolders()
        {
            string rootPath = Path.GetDirectoryName(Environment.ProcessPath)!;
            string dotMinecraftPath = Path.Combine(rootPath, ".minecraft");
            string rmlPath = Path.Combine(rootPath, "RML");

            // 创建文件夹
            Directory.CreateDirectory(dotMinecraftPath);
            Directory.CreateDirectory(rmlPath);
        }
        public void ShowGlobalNotification(string title, string message, InfoBarSeverity severity)
        {
            GlobalInfoBar.Title = title;
            GlobalInfoBar.Message = message;
            GlobalInfoBar.Severity = severity;
            GlobalInfoBar.IsOpen = true;

            // 贴心设计：3秒后自动把横幅收起来，不用玩家手动点叉！
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, e) =>
            {
                GlobalInfoBar.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
        }

    }
}

        

