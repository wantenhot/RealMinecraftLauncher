using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace App3
{
    public static class JavaDetector
    {
        // 扫描并返回所有找到的 javaw.exe 路径
        public static List<string> GetInstalledJavas()
        {
            // 使用 HashSet 自动去重
            HashSet<string> javaPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // Oracle Java
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\JavaSoft\Java Runtime Environment");
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\JavaSoft\Java Development Kit");
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\JavaSoft\JDK");
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\JavaSoft\JRE");

            // AdoptOpenJDK
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\AdoptOpenJDK\JDK");
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\AdoptOpenJDK\JRE");

            // Eclipse Foundation (Temurin / Adoptium)
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\Eclipse Foundation\JDK");
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\Eclipse Foundation\JRE");
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\Eclipse Adoptium\JDK");
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\Eclipse Adoptium\JRE");

            // BellSoft (Liberica JDK)
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\BellSoft\Liberica");

            // Microsoft OpenJDK
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\Microsoft\JDK");

            // Amazon Corretto
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\Amazon\Corretto");

            // Azul Systems (Zulu JDK)
            SearchAllRegistryViews(javaPaths, @"SOFTWARE\Azul Systems\Zulu");

            string? javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome))
                CheckAndAdd(javaPaths, Path.Combine(javaHome, "bin", "javaw.exe"));

            string? pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (string p in pathEnv.Split(Path.PathSeparator))
                {
                    CheckAndAdd(javaPaths, Path.Combine(p, "javaw.exe"));
                }
            }
            string[] commonRoots = {
                @"C:\Program Files\Java",
                @"C:\Program Files (x86)\Java",
                @"C:\Program Files\Eclipse Adoptium",
                @"C:\Program Files\Amazon Corretto",
                @"C:\Program Files\BellSoft\LibericaJDK",
                @"C:\Program Files\Zulu",
                // 微软正版启动器的默认 Java 下载位置
                Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Packages\Microsoft.4297127D64EC6_8wekyb3d8bbwe\LocalCache\Local\runtime"),
                Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Minecraft Launcher\runtime")
            };

            foreach (string root in commonRoots)
            {
                if (Directory.Exists(root))
                {
                    try
                    {
                        string[] allExes = Directory.GetFiles(root, "javaw.exe", SearchOption.AllDirectories);
                        foreach (string exe in allExes) CheckAndAdd(javaPaths, exe);
                    }
                    catch { }
                }
            }
            return new List<string>(javaPaths);

        }
        private static void SearchAllRegistryViews(HashSet<string> paths, string subKeyPath)
        {
            // 查 64位 系统下的路径
            SearchSingleRegistry(paths, subKeyPath, RegistryView.Registry64);
            // 查 32位 系统下的路径 
            SearchSingleRegistry(paths, subKeyPath, RegistryView.Registry32);
        }
        private static void SearchSingleRegistry(HashSet<string> paths, string subKeyPath, RegistryView view)
        {
            try
            {
                // 以指定的位数（32或64）打开本地机器注册表
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using RegistryKey? key = baseKey.OpenSubKey(subKeyPath);

                if (key == null) return;

                foreach (string version in key.GetSubKeyNames())
                {
                    using RegistryKey? versionKey = key.OpenSubKey(version);
                    // 找到 JavaHome 路径
                    string? javaHome = versionKey?.GetValue("JavaHome") as string;
                    if (!string.IsNullOrEmpty(javaHome))
                    {
                        CheckAndAdd(paths, Path.Combine(javaHome, "bin", "javaw.exe"));
                    }
                }
            }
            catch { } 
        }
        private static void CheckAndAdd(HashSet<string> paths, string exePath)
        {
            try
            {
                // 如果文件真的存在，才放进名单里
                if (File.Exists(exePath)) paths.Add(exePath);
            }
            catch { }
        }
    }
}