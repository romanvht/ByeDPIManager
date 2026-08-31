using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace bdmanager {
  public class SystemProxyManager {
    private const string InternetSettingsPath = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
    private const string ProxyEnableName = "ProxyEnable";
    private const string ProxyServerName = "ProxyServer";
    private const string ProxyOverrideName = "ProxyOverride";
    private const string AutoConfigUrlName = "AutoConfigURL";
    private const string DefaultProxyOverride = "<local>";

    private const int InternetOptionRefresh = 37;
    private const int InternetOptionSettingsChanged = 39;

    private static readonly string BackupPath = Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      "config",
      "system-proxy-backup.json"
    );

    private bool _isActive;

    public bool IsActive => _isActive;

    [DllImport("wininet.dll", SetLastError = true)]
    private static extern bool InternetSetOption(
      IntPtr internetHandle,
      int option,
      IntPtr buffer,
      int bufferLength
    );

    public bool Enable(string host, int port) {
      if (_isActive) {
        return true;
      }

      try {
        if (File.Exists(BackupPath) && !RestoreIfNeeded()) {
          return false;
        }

        string formattedHost = host != null && host.Contains(":") ? $"[{host}]" : host;
        string proxyServer = $"socks5://{formattedHost}:{port}";
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(InternetSettingsPath)) {
          if (key == null) {
            throw new InvalidOperationException("Unable to open the current user's Internet settings.");
          }

          SystemProxyBackup backup = CreateBackup(key, proxyServer);
          SaveBackup(backup);

          key.SetValue(ProxyServerName, proxyServer, RegistryValueKind.String);
          key.SetValue(ProxyOverrideName, DefaultProxyOverride, RegistryValueKind.String);
          key.SetValue(ProxyEnableName, 1, RegistryValueKind.DWord);
          key.DeleteValue(AutoConfigUrlName, false);

          if (!IsOwnedConfiguration(key, backup)) {
            throw new InvalidOperationException("Windows did not accept the system proxy settings.");
          }
        }

        NotifySettingsChanged();
        _isActive = true;
        Program.logger.Log(string.Format(
          Program.localization.GetString("process_manager.system_proxy.started"),
          host,
          port
        ));
        return true;
      }
      catch (Exception ex) {
        Program.logger.Log(string.Format(
          Program.localization.GetString("process_manager.system_proxy.start_error"),
          ex.Message
        ));
        RestoreAfterFailedEnable();
        return false;
      }
    }

    public bool Disable() {
      return RestoreIfNeeded();
    }

    public bool RestoreIfNeeded() {
      if (!File.Exists(BackupPath)) {
        _isActive = false;
        return true;
      }

      try {
        SystemProxyBackup backup = JsonSerializer.Deserialize<SystemProxyBackup>(File.ReadAllText(BackupPath));
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(InternetSettingsPath)) {
          if (key == null) {
            throw new InvalidOperationException("Unable to open the current user's Internet settings.");
          }

          if (!IsOwnedConfiguration(key, backup)) {
            File.Delete(BackupPath);
            _isActive = false;
            Program.logger.Log(Program.localization.GetString("process_manager.system_proxy.changed_externally"));
            return true;
          }

          RestoreBackup(key, backup);
        }

        NotifySettingsChanged();
        File.Delete(BackupPath);
        _isActive = false;
        Program.logger.Log(Program.localization.GetString("process_manager.system_proxy.restored"));
        return true;
      }
      catch (Exception ex) {
        Program.logger.Log(string.Format(
          Program.localization.GetString("process_manager.system_proxy.restore_error"),
          ex.Message
        ));
        EmergencyDisable();
        return false;
      }
    }

    private static SystemProxyBackup CreateBackup(RegistryKey key, string appliedProxyServer) {
      string[] valueNames = key.GetValueNames();
      return new SystemProxyBackup {
        ProxyEnableExists = ContainsValue(valueNames, ProxyEnableName),
        ProxyEnable = ReadInt32(key, ProxyEnableName),
        ProxyServerExists = ContainsValue(valueNames, ProxyServerName),
        ProxyServer = ReadString(key, ProxyServerName),
        ProxyOverrideExists = ContainsValue(valueNames, ProxyOverrideName),
        ProxyOverride = ReadString(key, ProxyOverrideName),
        AutoConfigUrlExists = ContainsValue(valueNames, AutoConfigUrlName),
        AutoConfigUrl = ReadString(key, AutoConfigUrlName),
        AppliedProxyServer = appliedProxyServer,
        AppliedProxyOverride = DefaultProxyOverride
      };
    }

    private static bool IsOwnedConfiguration(RegistryKey key, SystemProxyBackup backup) {
      return ReadInt32(key, ProxyEnableName) == 1
        && string.Equals(ReadString(key, ProxyServerName), backup.AppliedProxyServer, StringComparison.OrdinalIgnoreCase)
        && string.Equals(ReadString(key, ProxyOverrideName), backup.AppliedProxyOverride, StringComparison.Ordinal)
        && ReadString(key, AutoConfigUrlName) == null;
    }

    private static void RestoreBackup(RegistryKey key, SystemProxyBackup backup) {
      RestoreDword(key, ProxyEnableName, backup.ProxyEnableExists, backup.ProxyEnable);
      RestoreString(key, ProxyServerName, backup.ProxyServerExists, backup.ProxyServer);
      RestoreString(key, ProxyOverrideName, backup.ProxyOverrideExists, backup.ProxyOverride);
      RestoreString(key, AutoConfigUrlName, backup.AutoConfigUrlExists, backup.AutoConfigUrl);
    }

    private static void RestoreDword(RegistryKey key, string name, bool existed, int value) {
      if (existed) {
        key.SetValue(name, value, RegistryValueKind.DWord);
      }
      else {
        key.DeleteValue(name, false);
      }
    }

    private static void RestoreString(RegistryKey key, string name, bool existed, string value) {
      if (existed) {
        key.SetValue(name, value ?? string.Empty, RegistryValueKind.String);
      }
      else {
        key.DeleteValue(name, false);
      }
    }

    private static int ReadInt32(RegistryKey key, string name) {
      object value = key.GetValue(name, 0, RegistryValueOptions.DoNotExpandEnvironmentNames);
      try {
        return Convert.ToInt32(value);
      }
      catch {
        return 0;
      }
    }

    private static string ReadString(RegistryKey key, string name) {
      object value = key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
      return value == null ? null : Convert.ToString(value);
    }

    private static bool ContainsValue(string[] valueNames, string name) {
      return valueNames.Contains(name, StringComparer.OrdinalIgnoreCase);
    }

    private static void SaveBackup(SystemProxyBackup backup) {
      string directory = Path.GetDirectoryName(BackupPath);
      if (!Directory.Exists(directory)) {
        Directory.CreateDirectory(directory);
      }

      string temporaryPath = BackupPath + ".tmp";
      File.WriteAllText(temporaryPath, JsonSerializer.Serialize(backup));
      if (File.Exists(BackupPath)) {
        File.Replace(temporaryPath, BackupPath, null);
      }
      else {
        File.Move(temporaryPath, BackupPath);
      }
    }

    private void RestoreAfterFailedEnable() {
      if (!File.Exists(BackupPath)) {
        return;
      }

      try {
        SystemProxyBackup backup = JsonSerializer.Deserialize<SystemProxyBackup>(File.ReadAllText(BackupPath));
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(InternetSettingsPath)) {
          if (key != null) {
            RestoreBackup(key, backup);
          }
        }
        NotifySettingsChanged();
        File.Delete(BackupPath);
      }
      catch {
        EmergencyDisable();
      }
      finally {
        _isActive = false;
      }
    }

    private static void EmergencyDisable() {
      try {
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(InternetSettingsPath)) {
          key?.SetValue(ProxyEnableName, 0, RegistryValueKind.DWord);
        }
        NotifySettingsChanged();
      }
      catch {
      }
    }

    private static void NotifySettingsChanged() {
      InternetSetOption(IntPtr.Zero, InternetOptionSettingsChanged, IntPtr.Zero, 0);
      InternetSetOption(IntPtr.Zero, InternetOptionRefresh, IntPtr.Zero, 0);
    }
  }
}
