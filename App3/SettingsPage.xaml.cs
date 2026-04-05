using System;
using System.Collections.Generic;
using System.IO; 
using Microsoft.UI.Xaml.Controls;

namespace App3
{
    public sealed partial class SettingsPage : Page
    {
        private readonly string settingsFilePath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "RML", "MyLauncher_JavaSetting.txt");
        private bool isInitializing = true;
        private bool _isUpdatingRam = false;

        public SettingsPage()
        {
            this.InitializeComponent();
            LoadJavaSettings();
            var config = ConfigManager.ReadConfig();
            long totalMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
            int totalMemoryMb = (int)(totalMemoryBytes / 1024 / 1024);

            RamSlider.Maximum = totalMemoryMb;
            RamInput.Maximum = totalMemoryMb;
            if (config.MaxRamMb > totalMemoryMb)
            {
                config.MaxRamMb = totalMemoryMb;
                ConfigManager.SaveConfig(config);
            }

            _isUpdatingRam = true;
            RamSlider.Value = config.MaxRamMb;
            RamInput.Value = config.MaxRamMb;
            _isUpdatingRam = false;
        }

        private void LoadJavaSettings()
        {
            List<string> javas = JavaDetector.GetInstalledJavas();
            JavaComboBox.ItemsSource = javas;

            string savedJava = "";
            if (File.Exists(settingsFilePath))
            {
                savedJava = File.ReadAllText(settingsFilePath); 
            }

            if (!string.IsNullOrEmpty(savedJava) && javas.Contains(savedJava))
            {
                JavaComboBox.SelectedItem = savedJava;
            }
            else if (javas.Count > 0)
            {
                JavaComboBox.SelectedIndex = 0;
                File.WriteAllText(settingsFilePath, JavaComboBox.SelectedItem.ToString());
            }

            isInitializing = false;
        }

        private void JavaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (JavaComboBox.SelectedItem != null)
            {
                var config = ConfigManager.ReadConfig();
                config.JavaPath = JavaComboBox.SelectedItem.ToString()!;
                ConfigManager.SaveConfig(config);

                if (!isInitializing)
                {
                    MainWindow.Instance.ShowGlobalNotification("设置已保存", "Java 路径已更新", InfoBarSeverity.Success);
                }
            }
        }

        private void BrowseJavaButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            MainWindow.Instance.ShowGlobalNotification("提示", "文件选择功能马上就来！", InfoBarSeverity.Informational);
        }
        private void RamSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (_isUpdatingRam) return;
            int newRam = (int)e.NewValue;

            _isUpdatingRam = true;
            if (RamInput != null) RamInput.Value = newRam;
            SaveRamConfig(newRam);
            _isUpdatingRam = false;
        }

        private void SaveRamConfig(int ramSize)
        {
            var config = ConfigManager.ReadConfig();
            config.MaxRamMb = ramSize;
            ConfigManager.SaveConfig(config);
        }

        private void RamInput_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            
            if (_isUpdatingRam || double.IsNaN(args.NewValue)) return;

            int newRam = (int)args.NewValue;

            _isUpdatingRam = true; 
            if (RamSlider != null) RamSlider.Value = newRam; 
            SaveRamConfig(newRam);
            _isUpdatingRam = false; 
        }
    }
}