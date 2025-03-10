using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace bdmanager {
  public class AutorunManager {
    private const string RunKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
    private readonly string _appName;
    private readonly string _appPath;

    public AutorunManager() {
      _appName = Program.appName;
      _appPath = Application.ExecutablePath;
    }

    public bool IsAutorunEnabled() {
      using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, false)) {
        string expectedValue = $"\"{_appPath}\" --autorun";
        object value = key?.GetValue(_appName);
        return value != null && value.ToString() == expectedValue;
      }
    }

    public void SetAutorun(bool enabled) {
      using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, true)) {
        if (key == null) return;

        if (enabled) {
          key.SetValue(_appName, $"\"{_appPath}\" --autorun");
        } else {
          key.DeleteValue(_appName, false);
        }
      }
    }
  }
}