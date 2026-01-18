using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace bdmanager {
  public class HotkeyManager : IDisposable {
    private const int WM_HOTKEY = 0x0312;
    private const int MOD_ALT = 0x0001;
    private const int MOD_CONTROL = 0x0002;
    private const int MOD_SHIFT = 0x0004;
    private const int MOD_WIN = 0x0008;

    private int _currentHotkeyId = 0;
    private readonly IntPtr _windowHandle;
    private readonly Action _hotkeyAction;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public HotkeyManager(IntPtr windowHandle, Action hotkeyAction) {
      _windowHandle = windowHandle;
      _hotkeyAction = hotkeyAction;
    }

    public bool RegisterHotkey(string hotkeyCombination) {
      UnregisterCurrentHotkey();

      if (string.IsNullOrWhiteSpace(hotkeyCombination))
        return false;

      try {
        _currentHotkeyId = GetHashCode();
        var (modifiers, vk) = ParseHotkeyCombination(hotkeyCombination);
        bool registered = RegisterHotKey(_windowHandle, _currentHotkeyId, modifiers, vk);

        if (!registered) {
          int error = Marshal.GetLastWin32Error();
          Program.logger.Log(string.Format(Program.localization.GetString("hotkey.error"), $"{hotkeyCombination}, WinAPI {error}"));
          return false;
        }

        return true;
      }
      catch (Exception ex) {
        Program.logger.Log(string.Format(Program.localization.GetString("hotkey.error"), ex.Message));
        return false;
      }
    }

    public void UnregisterCurrentHotkey() {
      if (_currentHotkeyId != 0) {
        UnregisterHotKey(_windowHandle, _currentHotkeyId);
        _currentHotkeyId = 0;
      }
    }

    public void ProcessMessage(Message m) {
      if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == _currentHotkeyId)
        _hotkeyAction?.Invoke();
    }

    private (int modifiers, int vk) ParseHotkeyCombination(string combination) {
      int modifiers = 0;
      Keys mainKey = Keys.None;

      string[] parts = combination.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
      foreach (var raw in parts) {
        var part = raw.Trim();
        Keys parsed;

        switch (part.ToUpperInvariant()) {
          case "CTRL":
          case "CONTROL":
            parsed = Keys.ControlKey;
            break;
          case "ALT":
            parsed = Keys.Menu;
            break;
          case "SHIFT":
            parsed = Keys.ShiftKey;
            break;
          case "WIN":
          case "WINDOWS":
            parsed = Keys.LWin;
            break;
          default:
            if (!Enum.TryParse(part, true, out parsed))
              throw new ArgumentException($"{combination}");
            break;
        }

        if (parsed == Keys.ControlKey)
          modifiers |= MOD_CONTROL;
        else if (parsed == Keys.Menu)
          modifiers |= MOD_ALT;
        else if (parsed == Keys.ShiftKey)
          modifiers |= MOD_SHIFT;
        else if (parsed == Keys.LWin || parsed == Keys.RWin)
          modifiers |= MOD_WIN;
        else {
          if (mainKey != Keys.None)
            throw new ArgumentException($"{combination}");

          mainKey = parsed;
        }
      }

      if (mainKey == Keys.None)
        throw new ArgumentException($"{combination}");

      return (modifiers, (int)mainKey);
    }

    public void Dispose() {
      UnregisterCurrentHotkey();
      GC.SuppressFinalize(this);
    }
  }
}
