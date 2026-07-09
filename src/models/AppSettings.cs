using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace bdmanager {
  public class AppSettings {
    public bool AutoStart { get; set; } = false;
    public bool AutoConnect { get; set; } = false;
    public bool StartMinimized { get; set; } = false;

    public bool MinimizeToTray { get; set; } = true;

    public string Language { get; set; } = "ru";
    public string Hotkey { get; set; } = "Ctrl+Alt+B";

    public string ByeDpiArguments { get; set; } = "-Ku -a1 -An -o1 -At,r,s -d1";
    public List<HistoryItem> ByeDpiHistory { get; set; } = new List<HistoryItem>();

    public bool DisableProxiFyre { get; set; } = false;
    public string ProxiFyreIp { get; set; } = "127.0.0.1";
    public int ProxiFyrePort { get; set; } = 1080;
    public bool ProxiFyreLan { get; set; } = false;
    public List<string> ProxifiedApps { get; set; } = new List<string>();

    public int ProxyTestDelay { get; set; } = 0;
    public int ProxyTestRequestsCount { get; set; } = 1;

    public string ByeDpiPath { get; set; } = Path.Combine("libs", "byedpi", "ciadpi.exe");
    public string ProxiFyrePath { get; set; } = Path.Combine("libs", "proxifyre", "proxifyre.exe");

    private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "settings.json");
    private static readonly string DefaultByeDpiPath = Path.Combine("libs", "byedpi", "ciadpi.exe");
    private static readonly string DefaultProxiFyrePath = Path.Combine("libs", "proxifyre", "proxifyre.exe");

    public static AppSettings Load() {
      try {
        if (File.Exists(SettingsPath)) {
          string json = File.ReadAllText(SettingsPath);
          var settings = JsonSerializer.Deserialize<AppSettings>(json);
          settings.NormalizePaths();
          settings.ByeDpiHistory = HistoryManager.Ensure(settings.ByeDpiHistory);
          return settings;
        }
      }
      catch (Exception ex) {
        Program.logger.Log($"{ex.Message}");
      }

      return new AppSettings();
    }

    public string GetByeDpiExecutablePath() {
      return ResolveAppPath(ByeDpiPath);
    }

    public string GetProxiFyreExecutablePath() {
      return ResolveAppPath(ProxiFyrePath);
    }

    private void NormalizePaths() {
      ByeDpiPath = NormalizeDefaultPath(ByeDpiPath, DefaultByeDpiPath);
      ProxiFyrePath = NormalizeDefaultPath(ProxiFyrePath, DefaultProxiFyrePath);
    }

    private static string ResolveAppPath(string path) {
      if (string.IsNullOrWhiteSpace(path)) {
        return path;
      }

      if (Path.IsPathRooted(path)) {
        return path;
      }

      return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
    }

    private static string NormalizeDefaultPath(string path, string defaultPath) {
      if (string.IsNullOrWhiteSpace(path)) {
        return defaultPath;
      }

      string trimmedPath = path.Trim();
      if (!Path.IsPathRooted(trimmedPath)) {
        return trimmedPath;
      }

      string relativeSuffix = Path.DirectorySeparatorChar + defaultPath;
      string alternateSuffix = Path.AltDirectorySeparatorChar + defaultPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

      if (trimmedPath.EndsWith(relativeSuffix, StringComparison.OrdinalIgnoreCase) ||
          trimmedPath.EndsWith(alternateSuffix, StringComparison.OrdinalIgnoreCase)) {
        return defaultPath;
      }

      return trimmedPath;
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
        var result = FilterLinuxOnlyArgs(args).ToList();

        if (!DisableProxiFyre) {
          bool hasPort = result.Any(arg => arg == "-p" || arg == "--port") || result.Any(arg => arg.StartsWith("-p") && arg.Length > 2 && char.IsDigit(arg[2]));

          if (!hasPort) {
            result.AddRange(new[] { "--port", ProxiFyrePort.ToString() });
          }
        }

        return string.Join(" ", result);
      }
      catch (Exception ex) {
        Program.logger.Log($"ByeDPI: {ex.Message}");
        return ByeDpiArguments;
      }
    }
  }
}
