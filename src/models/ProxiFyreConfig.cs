using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace bdmanager {
  public class ProxyConfig {
    public List<string> appNames { get; set; } = new List<string>();
    public string socks5ProxyEndpoint { get; set; } = "127.0.0.1:1080";
    public List<string> supportedProtocols { get; set; } = new List<string> { "TCP", "UDP" };
  }

  public class ProxiFyreConfig {
    public string logLevel { get; set; } = "error";
    public bool bypassLan { get; set; } = true;
    public List<ProxyConfig> proxies { get; set; } = new List<ProxyConfig>();
    public List<string> excludes { get; set; } = new List<string>();

    public static ProxiFyreConfig Load(string filePath) {
      if (File.Exists(filePath)) {
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<ProxiFyreConfig>(json);
      }
      return new ProxiFyreConfig();
    }

    public void Save(string filePath) {
      string json = JsonSerializer.Serialize(this);
      File.WriteAllText(filePath, json);
    }

    public static bool UpdateConfig(AppSettings settings) {
      try {
        string configPath = Path.Combine(Path.GetDirectoryName(settings.ProxiFyrePath), "app-config.json");

        var config = new ProxiFyreConfig();
        var appNames = settings.ProxifiedApps.Select(a => a.Trim().Trim('"').Trim('\'')).ToList();

        config.proxies.Add(new ProxyConfig {
          appNames = appNames.Count > 0 ? appNames : new List<string> { "" },
          socks5ProxyEndpoint = $"{settings.ProxiFyreIp}:{settings.ProxiFyrePort}",
          supportedProtocols = new List<string> { "TCP", "UDP" }
        });

        config.bypassLan = !settings.ProxiFyreLan;
        config.excludes.Add(Path.GetFileNameWithoutExtension(settings.ByeDpiPath));
        config.Save(configPath);

        return true;
      }
      catch (Exception ex) {
        Program.logger.Log($"ProxiFyre: {ex.Message}");
        return false;
      }
    }
  }
}
