using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace bdmanager {
  public class HotkeyTextBox : TextBox {
    private Keys _hotkey;
    public Keys Hotkey => _hotkey;

    public HotkeyTextBox() {
      KeyDown += HotkeyTextBox_KeyDown;
      KeyUp += HotkeyTextBox_KeyUp;
      GotFocus += HotkeyTextBox_GotFocus;
      LostFocus += HotkeyTextBox_LostFocus;
    }

    private void HotkeyTextBox_GotFocus(object sender, EventArgs e) {
      Text = Program.localization.GetString("hotkey.wait_input");
    }

    private void HotkeyTextBox_LostFocus(object sender, EventArgs e) {
      if (string.IsNullOrWhiteSpace(Text) || Text == Program.localization.GetString("hotkey.wait_input")) {
        Text = Program.settings.Hotkey;
      }
    }

    private void HotkeyTextBox_KeyDown(object sender, KeyEventArgs e) {
      e.SuppressKeyPress = true;

      var modifiers = new List<string>();
      if (e.Control) modifiers.Add("Ctrl");
      if (e.Alt) modifiers.Add("Alt");
      if (e.Shift) modifiers.Add("Shift");

      if (e.KeyCode != Keys.ControlKey &&
          e.KeyCode != Keys.ShiftKey &&
          e.KeyCode != Keys.Menu) {
        modifiers.Add(e.KeyCode.ToString());
      }

      _hotkey = e.KeyData;
      Text = string.Join("+", modifiers);
    }

    private void HotkeyTextBox_KeyUp(object sender, KeyEventArgs e) {

    }
  }
}
