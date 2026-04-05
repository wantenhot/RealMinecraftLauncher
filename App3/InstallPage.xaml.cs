using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http; // 用来发网络请求
using System.Text.Json; // 用来解析 JSON 数据
using System.Threading.Tasks; // 用来做异步操作（防止软件卡死）

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InstallPage : Page
    {
        public InstallPage()
        {
            InitializeComponent();
            _ = LoadMinecraftVersion();
        }

        public class MojangManifest
        {
            public List<MojangVersion>? versions { get; set; }
        
        
        }
        public class MojangVersion
        {
            public string? id { get; set; }   
            public string? type { get; set; }
            public string? url { get; set; }
        }
        public class MinecraftVersion
        {
            public string? VersionName { get; set; } 
            public string? VersionType { get; set; } 
            public string? VersionUrl { get; set; }
        }

        public class VersionManifestJson
        {
            public VersionDownloads? downloads { get; set; }
            public List<LibraryEntry>? libraries { get; set; } 
            public AssetIndexEntry? assetIndex { get; set; }  
        }

        public class VersionDownloads
        {
            public DownloadFile? client { get; set; }
        }

        public class DownloadFile
        {
            public string? url { get; set; } 
        }
        public class LibraryEntry
        {
            public LibraryDownloads? downloads { get; set; }
        }
        public class LibraryDownloads
        {
            public LibraryArtifact? artifact { get; set; }
        }
        public class LibraryArtifact
        {
            public string? path { get; set; } 
            public string? url { get; set; }
        }
        public class AssetIndexEntry
        {
            public string? id { get; set; }
            public string? url { get; set; } 
        }
        public class AssetIndexJson
        {
            public Dictionary<string, AssetObject>? objects { get; set; }
        }
        public class AssetObject
        {
            public string? hash { get; set; }
        }

        private async Task LoadMinecraftVersion()
        {
            try
            {
                using HttpClient client = new HttpClient();
                string url = "https://launchermeta.mojang.com/mc/game/version_manifest_v2.json";


                string json_text = await client.GetStringAsync(url);
                var manifest = JsonSerializer.Deserialize<MojangManifest>(json_text);

                List<MinecraftVersion> ui_version = new List<MinecraftVersion>();

                if (manifest != null && manifest.versions != null)
                {
                    foreach (var v in manifest.versions)
                    {
                        if (v == null || v.type == null || v.id == null)
                        {
                            continue;
                        }
                        string display_type = v.type;
                        if (v.type == "release") display_type = "正式版 (Release)";
                        else if (v.type == "snapshot") display_type = "快照 (Snapshot)";
                        else if (v.type == "old_alpha") display_type = "远古 Alpha";
                        else if (v.type == "old_beta") display_type = "远古 Beta";

                        ui_version.Add(new MinecraftVersion
                        {
                            VersionName = v.id,
                            VersionType = display_type,
                            VersionUrl = v.url
                        });

                    }
                    VersionListName.ItemsSource = ui_version;

                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowGlobalNotification("网络错误", "获取版本失败：" + ex.Message, InfoBarSeverity.Error);
            }
        }



        
        private async void VersionListName_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is MinecraftVersion clicked_version)
            {
                string? targetVersion = clicked_version.VersionName;
                string? targetType = clicked_version.VersionType;
                string? targetUrl = clicked_version.VersionUrl;

                ContentDialog dialog = new ContentDialog
                {
                    Title = "确认下载",

                    Content = $"您即将下载: \nMinecraft {targetVersion}\n类型: {targetType}\n\n是否继续?",

                    PrimaryButtonText = "立即下载",

                    CloseButtonText = "取消",

                    XamlRoot = this.XamlRoot,

                    DefaultButton = ContentDialogButton.Primary


                };
                ContentDialogResult result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    MainWindow.Instance.ShowGlobalNotification("准备下载", $"正在准备下载 {targetVersion}...", InfoBarSeverity.Success);
                    _ = StartDownloadVersionAsync(targetVersion!, targetUrl!);
                }
                else
                {
                    MainWindow.Instance.ShowGlobalNotification("已取消", "下载操作已取消。", InfoBarSeverity.Informational);
                }

            }
        }

        private async Task StartDownloadVersionAsync(string versionId, string versionUrl)
        {
            try
            {
                string rootPath = System.AppContext.BaseDirectory;
                string dotMinecraftPath = Path.Combine(rootPath, ".minecraft");
                
                string versionFolder = Path.Combine(dotMinecraftPath, "versions", versionId);
                Directory.CreateDirectory(versionFolder); 

                string jsonFilePath = Path.Combine(versionFolder, $"{versionId}.json");

                using HttpClient client = new HttpClient();

                string jsonContent = await client.GetStringAsync(versionUrl);

               
                
                await File.WriteAllTextAsync(jsonFilePath, jsonContent);

                //下载成功
                MainWindow.Instance.ShowGlobalNotification("下载成功", $"{versionId} 的核心 JSON 文件已成功保存！", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);

                var versionData = System.Text.Json.JsonSerializer.Deserialize<VersionManifestJson>(jsonContent);
                string? clientJarUrl = versionData?.downloads?.client?.url;
                if (string.IsNullOrEmpty(clientJarUrl))
                {
                    MainWindow.Instance.ShowGlobalNotification("解析失败", "找不到 client.jar 的下载链接！", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
                    return; // 终止行动
                }
                MainWindow.Instance.ShowGlobalNotification("开始下载", $"正在下载 {versionId}.jar 本体，请稍候...", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning);
                string jarFilePath = Path.Combine(versionFolder, $"{versionId}.jar");
                byte[] jarBytes = await client.GetByteArrayAsync(clientJarUrl);
                await File.WriteAllBytesAsync(jarFilePath, jarBytes);
                MainWindow.Instance.ShowGlobalNotification("大功告成", $"Minecraft {versionId} 核心文件已全部下载完毕！", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);
                MainWindow.Instance.ShowGlobalNotification("下载资源", "开始下载游戏资源 (Assets)，文件极多请耐心等待...", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning);
                if (versionData?.libraries != null)//下载支持文件
                {
                    foreach (var lib in versionData.libraries)
                    {
                        var artifact = lib.downloads?.artifact;
                        if (artifact != null && !string.IsNullOrEmpty(artifact.url) && !string.IsNullOrEmpty(artifact.path))
                        {
                            string libFilePath = Path.Combine(dotMinecraftPath, "libraries", artifact.path);
                            Directory.CreateDirectory(Path.GetDirectoryName(libFilePath)!); 

                            if (!File.Exists(libFilePath))
                            {
                                byte[] libBytes = await client.GetByteArrayAsync(artifact.url);
                                await File.WriteAllBytesAsync(libFilePath, libBytes);
                            }
                        }
                    }
                }
                if (versionData?.assetIndex != null && !string.IsNullOrEmpty(versionData.assetIndex.url))
                {
                    string assetId = versionData.assetIndex.id ?? "legacy";

                    //json
                    string indexesFolder = Path.Combine(dotMinecraftPath, "assets", "indexes");
                    Directory.CreateDirectory(indexesFolder);
                    string assetIndexFilePath = Path.Combine(indexesFolder, $"{assetId}.json");

                    string assetIndexJsonContent = await client.GetStringAsync(versionData.assetIndex.url);
                    await File.WriteAllTextAsync(assetIndexFilePath, assetIndexJsonContent);

                    var assetData = System.Text.Json.JsonSerializer.Deserialize<AssetIndexJson>(assetIndexJsonContent);
                    if (assetData?.objects != null)
                    {
                        foreach (var asset in assetData.objects.Values)
                        {
                            if (!string.IsNullOrEmpty(asset.hash))
                            {
                                string folderName = asset.hash.Substring(0, 2);
                                string assetUrl = $"https://resources.download.minecraft.net/{folderName}/{asset.hash}";
                                string assetFilePath = Path.Combine(dotMinecraftPath, "assets", "objects", folderName, asset.hash);
                                Directory.CreateDirectory(Path.GetDirectoryName(assetFilePath)!);

                                if (!File.Exists(assetFilePath))
                                {
                                    byte[] assetBytes = await client.GetByteArrayAsync(assetUrl);
                                    await File.WriteAllBytesAsync(assetFilePath, assetBytes);
                                }
                            }
                        }
                    }
                }
                MainWindow.Instance.ShowGlobalNotification("全部完成", $"Minecraft {versionId} 所有核心、库、资源下载完毕！", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success);

            }
            catch (System.Exception ex)
            {
                
                MainWindow.Instance.ShowGlobalNotification("下载失败", $"文件写入错误：{ex.Message}", Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
            }
        }
    }
}
