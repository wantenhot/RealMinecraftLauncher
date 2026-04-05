using System;
using System.IO;
using System.Text.Json;

namespace App3
{
    public class LauncherConfig
    {
        public string JavaPath { get; set; } = "";
        public int MaxRamMb { get; set; } = 4096;      // 默认分配 4096MB 内存
        
    }

    public static class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(
            Path.GetDirectoryName(Environment.ProcessPath)!, "RML", "config.json");
        static ConfigManager()
        {
            string dirPath = Path.GetDirectoryName(ConfigPath)!;
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }
        public static LauncherConfig ReadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                var defaultConfig = new LauncherConfig();
                var defaultJava = JavaDetector.GetInstalledJavas();
                if (defaultJava != null && defaultJava.Count > 0)
                {
                    defaultConfig.JavaPath = defaultJava[0];
                }

                SaveConfig(defaultConfig);

                return defaultConfig;
            }
            

            try
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<LauncherConfig>(json) ?? new LauncherConfig();
            }
            catch
            {
                return new LauncherConfig();
            }
        }
        public static void SaveConfig(LauncherConfig config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);

            File.WriteAllText(ConfigPath, json);
        }
    }
}
