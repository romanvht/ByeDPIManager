using System;
using System.Collections.Generic;
using System.IO;

namespace bdmanager
{
    public class AppSettings
    {
        public string ByeDpiArguments { get; set; } = "";
        
        public string ProxiFyreIp { get; set; } = "127.0.0.1";
        public int ProxiFyrePort { get; set; } = 1080;
        public List<string> ProxifiedApps { get; set; } = new List<string>();
        
        public string ByeDpiPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", "byedpi", "ciadpi.exe");
        public string ProxiFyrePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", "proxifyre", "proxifyre.exe");
        public string ProxiFyreConfigPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", "proxifyre", "app-config.json");

        private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Program.AppName, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return SimpleJsonSerializer.Deserialize<AppSettings>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
            }

            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = SimpleJsonSerializer.Serialize(this);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении настроек: {ex.Message}");
            }
        }

        public void UpdateProxiFyreConfig()
        {
            try
            {
                string proxiFyreDir = Path.GetDirectoryName(ProxiFyrePath);
                ProxiFyreConfigPath = Path.Combine(proxiFyreDir, "app-config.json");
                
                string configDir = Path.GetDirectoryName(ProxiFyreConfigPath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                ProxiFyreConfig config = new ProxiFyreConfig();
                
                if (ProxifiedApps == null || ProxifiedApps.Count == 0)
                {
                    ProxifiedApps = new List<string> { };
                }
                
                ProxyConfig proxyConfig = new ProxyConfig
                {
                    appNames = ProxifiedApps,
                    socks5ProxyEndpoint = $"{ProxiFyreIp}:{ProxiFyrePort}",
                    supportedProtocols = new List<string> { "TCP", "UDP" }
                };
                
                config.proxies.Add(proxyConfig);
                
                config.Save(ProxiFyreConfigPath);
                
                if (!File.Exists(ProxiFyreConfigPath))
                {
                    throw new Exception($"Не удалось создать файл конфигурации: {ProxiFyreConfigPath}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обновлении конфигурации ProxiFyre: {ex.Message}", ex);
            }
        }
    }
} 