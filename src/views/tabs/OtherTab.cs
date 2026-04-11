using System;
using System.Drawing;
using System.Windows.Forms;

namespace bdmanager.Views.Tabs {
  public class OtherTab : TabPage {
    private AppSettings _settings;

    public CheckBox AutoStartCheckBox { get; private set; }
    public CheckBox AutoConnectCheckBox { get; private set; }
    public CheckBox StartMinimizedCheckBox { get; private set; }
    public CheckBox MinimizeToTrayCheckBox { get; private set; }

    public OtherTab(AppSettings settings) {
      _settings = settings;
      InitializeTab();
    }

    private void InitializeTab() {
      Text = Program.localization.GetString("settings_form.other.tab");
      Name = "otherTabPage";
      BackColor = SystemColors.Control;
      Padding = new Padding(10);

      TableLayoutPanel root = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 2,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        Name = "root"
      };

      root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
      root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
      Controls.Add(root);

      GroupBox autorunGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.autorun.group"),
        Name = "autorunGroupBox",
        Dock = DockStyle.Fill,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10, 5, 10, 10),
        Margin = new Padding(0, 0, 0, 8)
      };
      root.Controls.Add(autorunGroupBox, 0, 0);

      FlowLayoutPanel autorunLayout = new FlowLayoutPanel {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        AutoSize = true,
        WrapContents = false
      };
      autorunGroupBox.Controls.Add(autorunLayout);

      AutoStartCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.autorun.auto_start"),
        Name = "autoStartCheckBox",
        Margin = new Padding(3),
        AutoSize = true
      };
      autorunLayout.Controls.Add(AutoStartCheckBox);

      StartMinimizedCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.autorun.start_minimized"),
        Name = "startMinimizedCheckBox",
        Margin = new Padding(3),
        AutoSize = true
      };
      autorunLayout.Controls.Add(StartMinimizedCheckBox);

      GroupBox behaviorGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.behavior.group"),
        Name = "behaviorGroupBox",
        Dock = DockStyle.Fill,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10, 5, 10, 10)
      };
      root.Controls.Add(behaviorGroupBox, 0, 1);

      FlowLayoutPanel behaviorLayout = new FlowLayoutPanel {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        AutoSize = true,
        WrapContents = false
      };
      behaviorGroupBox.Controls.Add(behaviorLayout);

      AutoConnectCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.behavior.auto_connect"),
        Name = "autoConnectCheckBox",
        Margin = new Padding(3),
        AutoSize = true
      };
      behaviorLayout.Controls.Add(AutoConnectCheckBox);

      MinimizeToTrayCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.behavior.minimize_to_tray"),
        Name = "minimizeToTrayCheckBox",
        Margin = new Padding(3),
        AutoSize = true
      };
      behaviorLayout.Controls.Add(MinimizeToTrayCheckBox);
    }

    public void LoadSettings(AutorunManager autorunManager) {
      bool autorunEnabled = autorunManager.IsAutorunEnabled();
      if (_settings.AutoStart != autorunEnabled) {
        _settings.AutoStart = autorunEnabled;
      }

      AutoStartCheckBox.Checked = _settings.AutoStart;
      AutoConnectCheckBox.Checked = _settings.AutoConnect;
      StartMinimizedCheckBox.Checked = _settings.StartMinimized;
      MinimizeToTrayCheckBox.Checked = _settings.MinimizeToTray;
    }

    public void SaveSettings(AutorunManager autorunManager) {
      _settings.AutoStart = AutoStartCheckBox.Checked;
      _settings.AutoConnect = AutoConnectCheckBox.Checked;
      _settings.StartMinimized = StartMinimizedCheckBox.Checked;
      _settings.MinimizeToTray = MinimizeToTrayCheckBox.Checked;

      autorunManager.SetAutorun(_settings.AutoStart);
    }
  }
}
