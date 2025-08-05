using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

namespace bdmanager {
  public class HotkeyTextBox : TextBox {
    private Keys _hotkey;
    public Keys Hotkey => _hotkey;

    public HotkeyTextBox() {
      ReadOnly = true;
      KeyDown += HotkeyTextBox_KeyDown;
      KeyUp += HotkeyTextBox_KeyUp;
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
