using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace bdmanager {
  public sealed class HotkeyManager : IDisposable {
    public const int WM_HOTKEY = 0x0312;
    private const int MOD_ALT = 0x0001;
    private const int MOD_CONTROL = 0x0002;
    private const int MOD_SHIFT = 0x0004;
    private const int MOD_WIN = 0x0008;

    private readonly IntPtr _windowHandle;
    private readonly Action _hotkeyAction;
    private int _currentHotkeyId;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr windowHandle, int id, int modifiers, int virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr windowHandle, int id);

    public HotkeyManager(IntPtr windowHandle, Action hotkeyAction) {
      _windowHandle = windowHandle;
      _hotkeyAction = hotkeyAction;
    }

    public bool RegisterHotkey(string combination) {
      UnregisterCurrentHotkey();
      if (string.IsNullOrWhiteSpace(combination)) return false;

      try {
        Tuple<int, int> parsed = ParseHotkeyCombination(combination);
        _currentHotkeyId = Math.Abs((Program.appName + combination).GetHashCode());
        if (_currentHotkeyId == 0) _currentHotkeyId = 1;

        bool registered = RegisterHotKey(_windowHandle, _currentHotkeyId, parsed.Item1, parsed.Item2);
        if (!registered) {
          int error = Marshal.GetLastWin32Error();
          Program.logger.Log(string.Format(
            Program.localization.GetString("hotkey.error"),
            combination + ", WinAPI " + error
          ));
        }

        return registered;
      }
      catch (Exception ex) {
        Program.logger.Log(string.Format(Program.localization.GetString("hotkey.error"), ex.Message));
        return false;
      }
    }

    public void ProcessMessage(int message, IntPtr wParam) {
      if (message == WM_HOTKEY && wParam.ToInt32() == _currentHotkeyId) {
        _hotkeyAction?.Invoke();
      }
    }

    public void UnregisterCurrentHotkey() {
      if (_currentHotkeyId == 0) return;
      UnregisterHotKey(_windowHandle, _currentHotkeyId);
      _currentHotkeyId = 0;
    }

    private static Tuple<int, int> ParseHotkeyCombination(string combination) {
      int modifiers = 0;
      Key mainKey = Key.None;
      KeyConverter converter = new KeyConverter();

      foreach (string rawPart in combination.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)) {
        string part = rawPart.Trim();
        switch (part.ToUpperInvariant()) {
          case "CTRL":
          case "CONTROL":
            modifiers |= MOD_CONTROL;
            break;
          case "ALT":
            modifiers |= MOD_ALT;
            break;
          case "SHIFT":
            modifiers |= MOD_SHIFT;
            break;
          case "WIN":
          case "WINDOWS":
            modifiers |= MOD_WIN;
            break;
          default:
            if (mainKey != Key.None) throw new ArgumentException(combination);
            try {
              mainKey = (Key)converter.ConvertFromInvariantString(part);
            }
            catch {
              throw new ArgumentException(combination);
            }
            break;
        }
      }

      if (mainKey == Key.None || mainKey == Key.LeftCtrl || mainKey == Key.RightCtrl ||
          mainKey == Key.LeftAlt || mainKey == Key.RightAlt ||
          mainKey == Key.LeftShift || mainKey == Key.RightShift ||
          mainKey == Key.LWin || mainKey == Key.RWin) {
        throw new ArgumentException(combination);
      }

      return Tuple.Create(modifiers, KeyInterop.VirtualKeyFromKey(mainKey));
    }

    public void Dispose() {
      UnregisterCurrentHotkey();
      GC.SuppressFinalize(this);
    }
  }
}
