using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace bdmanager {
  public class AppSettings {
    public bool AutoStart { get; set; } = false;
    public bool AutoConnect { get; set; } = false;
    public bool StartMinimized { get; set; } = false;
    public string Language { get; set; } = "ru";
    public string Hotkey { get; set; } = "Ctrl+Alt+B";

    public string ByeDpiArguments { get; set; } = "-Ku -a1 -An -o1 -At,r,s -d1";

    public bool DisableProxiFyre { get; set; } = false;
    public string ProxiFyreIp { get; set; } = "127.0.0.1";
    public int ProxiFyrePort { get; set; } = 1080;
    public List<string> ProxifiedApps { get; set; } = new List<string>();

    public int ProxyTestDelay { get; set; } = 0;
    public int ProxyTestRequestsCount { get; set; } = 1;

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
        Program.logger.Log($"{ex.Message}");
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
        Program.logger.Log($"{ex.Message}");
      }
    }

    public bool UpdateProxiFyreConfig() {
      try {
        string ConfigPath = Path.Combine(Path.GetDirectoryName(ProxiFyrePath), "app-config.json");

        ProxiFyreConfig config = new ProxiFyreConfig();
        ProxyConfig proxyConfig = new ProxyConfig {
          appNames = ProxifiedApps.Count > 0 ? ProxifiedApps : new List<string> { "" },
          socks5ProxyEndpoint = $"{ProxiFyreIp}:{ProxiFyrePort}",
          supportedProtocols = new List<string> { "TCP", "UDP" }
        };

        string byeDpi = Path.GetFileNameWithoutExtension(ByeDpiPath);

        config.proxies.Add(proxyConfig);
        config.excludes.Add(byeDpi);
        config.Save(ConfigPath);

        return true;
      }
      catch (Exception ex) {
        Program.logger.Log($"ProxiFyre: {ex.Message}");
        return false;
      }
    }

    public static List<string> ShellSplit(string input) {
      var tokens = new List<string>();
      var quoteChar = ' ';
      var quoting = false;
      var lastCloseQuoteIndex = int.MinValue;
      var current = new System.Text.StringBuilder();

      for (int i = 0; i < input.Length; i++) {
        var c = input[i];

        if (quoting && c == quoteChar) {
          current.Append('"');
          quoting = false;
          lastCloseQuoteIndex = i;
        }
        else if (!quoting && (c == '\'' || c == '"')) {
          current.Append('"');
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

    public static IEnumerable<string> FilterLinuxOnlyArgs(IEnumerable<string> args) {
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

      return args.Where(arg => !linuxOnlyArgs.Contains(arg));
    }

    public string GetByeDpiArguments() {
      try {
        if (string.IsNullOrWhiteSpace(ByeDpiArguments)) {
          return string.Empty;
        }

        var args = ShellSplit(ByeDpiArguments);
        var result = FilterLinuxOnlyArgs(args);

        return string.Join(" ", result);
      }
      catch (Exception ex) {
        Program.logger.Log($"ByeDPI: {ex.Message}");
        return ByeDpiArguments;
      }
    }
  }
}
