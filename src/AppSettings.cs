using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace bdmanager {
  public class AppSettings {
    public bool AutoStart { get; set; } = false;
    public bool AutoConnect { get; set; } = false;
    public bool StartMinimized { get; set; } = false;

    public string ByeDpiArguments { get; set; } = "-Ku -a3 -An -Kt,h -d1 -s3+s -An";

    public string ProxiFyreIp { get; set; } = "127.0.0.1";
    public int ProxiFyrePort { get; set; } = 1080;
    public List<string> ProxifiedApps { get; set; } = new List<string>();

    public string ByeDpiPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", "byedpi", "ciadpi.exe");
    public string ProxiFyrePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", "proxifyre", "proxifyre.exe");

    private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "settings.json");

    public static AppSettings Load() {
      try {
        if (File.Exists(SettingsPath)) {
          string json = File.ReadAllText(SettingsPath);
          return JsonSerializer.Deserialize<AppSettings>(json);
        }
      }
      catch (Exception ex) {
        Console.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
      }

      return new AppSettings();
    }

    public void Save() {
      try {
        string directory = Path.GetDirectoryName(SettingsPath);
        if (!Directory.Exists(directory)) {
          Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(this);
        File.WriteAllText(SettingsPath, json);
      }
      catch (Exception ex) {
        Console.WriteLine($"Ошибка при сохранении настроек: {ex.Message}");
      }
    }

    public void UpdateProxiFyreConfig() {
      try {
        string ConfigPath = Path.Combine(Path.GetDirectoryName(ProxiFyrePath), "app-config.json");

        ProxiFyreConfig config = new ProxiFyreConfig();
        ProxyConfig proxyConfig = new ProxyConfig {
          appNames = ProxifiedApps,
          socks5ProxyEndpoint = $"{ProxiFyreIp}:{ProxiFyrePort}",
          supportedProtocols = new List<string> { "TCP", "UDP" }
        };

        config.proxies.Add(proxyConfig);
        config.Save(ConfigPath);

        if (!File.Exists(ConfigPath)) {
          throw new Exception($"Не удалось создать файл конфигурации: {ConfigPath}");
        }
      }
      catch (Exception ex) {
        throw new Exception($"Ошибка при обновлении конфигурации ProxiFyre: {ex.Message}", ex);
      }
    }

    private static List<string> ShellSplit(string input) {
      var tokens = new List<string>();
      var escaping = false;
      var quoteChar = ' ';
      var quoting = false;
      var lastCloseQuoteIndex = int.MinValue;
      var current = new System.Text.StringBuilder();

      for (int i = 0; i < input.Length; i++) {
        var c = input[i];

        if (escaping) {
          current.Append(c);
          escaping = false;
        }
        else if (c == '\\' && quoting) {
          if (i + 1 < input.Length && input[i + 1] == quoteChar) {
            escaping = true;
          } else {
            current.Append(c);
          }
        }
        else if (quoting && c == quoteChar) {
          quoting = false;
          lastCloseQuoteIndex = i;
        }
        else if (!quoting && (c == '\'' || c == '"')) {
          quoting = true;
          quoteChar = c;
        }
        else if (!quoting && char.IsWhiteSpace(c)) {
          if (current.Length > 0 || lastCloseQuoteIndex == i - 1) {
            tokens.Add(current.ToString());
            current.Clear();
          }
        }
        else {
          current.Append(c);
        }
      }

      if (current.Length > 0 || lastCloseQuoteIndex == input.Length - 1) {
        tokens.Add(current.ToString());
      }

      return tokens;
    }

    public string GetByeDpiArguments() {
      try {
        if (string.IsNullOrWhiteSpace(ByeDpiArguments)) {
          return string.Empty;
        }

        var linuxOnlyArgs = new HashSet<string>
        {
          "-D", "--daemon",
          "-w", "--pidfile",
          "-E", "--transparent",
          "-k", "--ip-opt",
          "-S", "--md5sig",
          "-Y", "--drop-sack",
          "-F", "--tfo"
        };

        var args = ShellSplit(ByeDpiArguments);
        var result = args.Where(arg => !linuxOnlyArgs.Contains(arg));

        return string.Join(" ", result);
      }
      catch (Exception ex) {
        Console.WriteLine($"Ошибка при обработке аргументов ByeDPI: {ex.Message}");
        return ByeDpiArguments;
      }
    }
  }
}
