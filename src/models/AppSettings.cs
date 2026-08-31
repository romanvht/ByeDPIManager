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
    public string ByeDpiIp { get; set; } = "127.0.0.1";
    public int ByeDpiPort { get; set; } = 1080;

    public RoutingMode RoutingMode { get; set; } = RoutingMode.ProxiFyre;
    public bool ProxiFyreLan { get; set; } = false;
    public List<string> ProxifiedApps { get; set; } = new List<string>();

    public int ProxyTestDelay { get; set; } = 0;
    public int ProxyTestRequestsCount { get; set; } = 1;
    public string ProxyTestSni { get; set; } = "google.com";

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

      string defaultFullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, defaultPath));

      if (string.Equals(trimmedPath, defaultFullPath, StringComparison.OrdinalIgnoreCase)) {
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
      return GetByeDpiArguments(ByeDpiArguments);
    }

    public string GetByeDpiArguments(string arguments) {
      try {
        var args = ShellSplit(arguments ?? string.Empty);
        var result = FilterLinuxOnlyArgs(args).ToList();

        if (!HasOption(result, "-i", "--ip")) {
          result.AddRange(new[] { "--ip", ByeDpiIp });
        }
        if (!HasOption(result, "-p", "--port")) {
          result.AddRange(new[] { "--port", ByeDpiPort.ToString() });
        }

        return string.Join(" ", result);
      }
      catch (Exception ex) {
        Program.logger.Log($"ByeDPI: {ex.Message}");
        return arguments;
      }
    }

    public string GetProxyIp(string arguments) {
      try {
        List<string> args = ShellSplit(arguments ?? string.Empty);
        if (TryGetOptionValue(args, "-i", "--ip", out string ip) && !string.IsNullOrWhiteSpace(ip)) {
          return GetConnectableIp(ip);
        }
      }
      catch (Exception ex) {
        Program.logger.Log($"ByeDPI: {ex.Message}");
      }

      return GetConnectableIp(ByeDpiIp);
    }

    public int GetProxyPort(string arguments) {
      try {
        List<string> args = ShellSplit(arguments ?? string.Empty);
        if (TryGetOptionValue(args, "-p", "--port", out string value)
            && int.TryParse(value, out int port)
            && port >= 1 && port <= 65535) {
          return port;
        }
      }
      catch (Exception ex) {
        Program.logger.Log($"ByeDPI: {ex.Message}");
      }

      return Math.Max(1, Math.Min(65535, ByeDpiPort));
    }

    private static bool HasOption(IReadOnlyList<string> args, string shortOption, string longOption) {
      return args.Any(argument => IsOption(argument, shortOption, longOption));
    }

    private static bool TryGetOptionValue(
      IReadOnlyList<string> args,
      string shortOption,
      string longOption,
      out string value
    ) {
      for (int index = 0; index < args.Count; index++) {
        string argument = args[index];
        if (argument == shortOption || argument == longOption) {
          return SetOptionValue(index + 1 < args.Count ? args[index + 1] : null, out value);
        }

        string longPrefix = longOption + "=";
        if (argument.StartsWith(longPrefix, StringComparison.Ordinal)) {
          return SetOptionValue(argument.Substring(longPrefix.Length), out value);
        }

        if (argument.StartsWith(shortOption, StringComparison.Ordinal) && argument.Length > shortOption.Length) {
          return SetOptionValue(argument.Substring(shortOption.Length).TrimStart('='), out value);
        }
      }

      value = null;
      return false;
    }

    private static bool SetOptionValue(string candidate, out string value) {
      value = candidate?.Trim().Trim('"', '\'');
      return !string.IsNullOrWhiteSpace(value);
    }

    private static bool IsOption(string argument, string shortOption, string longOption) {
      return argument == shortOption
        || argument == longOption
        || argument.StartsWith(longOption + "=", StringComparison.Ordinal)
        || (argument.StartsWith(shortOption, StringComparison.Ordinal) && argument.Length > shortOption.Length);
    }

    private static string GetConnectableIp(string ip) {
      string value = string.IsNullOrWhiteSpace(ip)
        ? "127.0.0.1"
        : ip.Trim().Trim('"', '\'').Trim('[', ']');
      if (value == "0.0.0.0") return "127.0.0.1";
      if (value == "::") return "::1";
      return value;
    }
  }
}
