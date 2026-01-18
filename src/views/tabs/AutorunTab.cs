using System;
using System.Drawing;
using System.Windows.Forms;

namespace bdmanager.Views.Tabs {
  public class AutorunTab : TabPage {
    private AppSettings _settings;

    public CheckBox AutoStartCheckBox { get; private set; }
    public CheckBox AutoConnectCheckBox { get; private set; }
    public CheckBox StartMinimizedCheckBox { get; private set; }

    public AutorunTab(AppSettings settings) {
      _settings = settings;
      InitializeTab();
    }

    private void InitializeTab() {
      Text = Program.localization.GetString("settings_form.autorun.tab");
      Name = "autorunTabPage";
      BackColor = SystemColors.Control;
      Padding = new Padding(10);

      GroupBox autorunGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.autorun.group"),
        Name = "autorunGroupBox",
        Dock = DockStyle.Fill,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10, 5, 10, 10)
      };
      Controls.Add(autorunGroupBox);

      FlowLayoutPanel autorunLayout = new FlowLayoutPanel {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        AutoSize = true,
        WrapContents = false,
        Name = "autorunLayout"
      };
      autorunGroupBox.Controls.Add(autorunLayout);

      AutoStartCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.autorun.auto_start"),
        Name = "autoStartCheckBox",
        Margin = new Padding(3),
        AutoSize = true
      };
      autorunLayout.Controls.Add(AutoStartCheckBox);

      AutoConnectCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.autorun.auto_connect"),
        Name = "autoConnectCheckBox",
        Margin = new Padding(3),
        AutoSize = true
      };
      autorunLayout.Controls.Add(AutoConnectCheckBox);

      StartMinimizedCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.autorun.start_minimized"),
        Name = "StartMinimizedCheckBox",
        Margin = new Padding(3),
        AutoSize = true
      };
      autorunLayout.Controls.Add(StartMinimizedCheckBox);
    }

    public void LoadSettings(AutorunManager autorunManager) {
      bool autorunEnabled = autorunManager.IsAutorunEnabled();
      if (_settings.AutoStart != autorunEnabled) {
        _settings.AutoStart = autorunEnabled;
      }

      AutoStartCheckBox.Checked = _settings.AutoStart;
      AutoConnectCheckBox.Checked = _settings.AutoConnect;
      StartMinimizedCheckBox.Checked = _settings.StartMinimized;
    }

    public void SaveSettings(AutorunManager autorunManager) {
      _settings.AutoStart = AutoStartCheckBox.Checked;
      _settings.AutoConnect = AutoConnectCheckBox.Checked;
      _settings.StartMinimized = StartMinimizedCheckBox.Checked;

      autorunManager.SetAutorun(_settings.AutoStart);
    }
  }
}
