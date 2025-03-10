using System.Collections.Generic;
using System.IO;

namespace bdmanager {
  public class ProxyConfig {
    public List<string> appNames { get; set; } = new List<string>();
    public string socks5ProxyEndpoint { get; set; } = "127.0.0.1:1080";
    public List<string> supportedProtocols { get; set; } = new List<string> { "TCP", "UDP" };
  }

  public class ProxiFyreConfig {
    public string logLevel { get; set; } = "None";
    public List<ProxyConfig> proxies { get; set; } = new List<ProxyConfig>();

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
  }
}
