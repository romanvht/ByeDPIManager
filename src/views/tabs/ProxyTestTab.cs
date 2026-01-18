using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace bdmanager.Views.Tabs {
  public class ProxyTestTab : TabPage {
    private AppSettings _settings;
    private ProxyTestManager _proxyTestManager;

    public NumericUpDown DelayNumericUpDown { get; private set; }
    public NumericUpDown RequestsCountNumericUpDown { get; private set; }
    public CheckBox FullLogCheckBox { get; private set; }
    public TextBox ProxyTestLogsBox { get; private set; }
    public Label ProxyTestProgressLabel { get; private set; }

    public ProxyTestTab(AppSettings settings) {
      _settings = settings;
      _proxyTestManager = new ProxyTestManager();
      InitializeTab();
    }

    private void InitializeTab() {
      Text = Program.localization.GetString("settings_form.proxy_test.tab");
      Name = "proxyTestTabPage";
      BackColor = SystemColors.Control;
      Padding = new Padding(10);

      TableLayoutPanel proxyTestTabLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 2,
        RowStyles = {
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.Percent, 100F)
        },
        Name = "proxyTestTabLayout"
      };
      Controls.Add(proxyTestTabLayout);

      // Proxy Settings Group
      GroupBox proxySettingsGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.proxy_test.settings_group"),
        Name = "proxySettingsGroupBox",
        Dock = DockStyle.Fill,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        MinimumSize = new Size(0, 130),
        Padding = new Padding(10, 5, 10, 10),
        Margin = new Padding(0)
      };
      proxyTestTabLayout.Controls.Add(proxySettingsGroupBox, 0, 0);

      TableLayoutPanel proxySettingsLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        RowCount = 4,
        ColumnStyles = {
          new ColumnStyle(SizeType.Percent, 100F),
          new ColumnStyle(SizeType.AutoSize),
        },
        RowStyles = {
          new RowStyle(SizeType.Percent, 100F),
          new RowStyle(SizeType.Percent, 100F),
          new RowStyle(SizeType.Percent, 100F),
          new RowStyle(SizeType.Percent, 100F),
        },
        Name = "proxySettingsLayout"
      };
      proxySettingsGroupBox.Controls.Add(proxySettingsLayout);

      Label delayLabel = new Label {
        Text = Program.localization.GetString("settings_form.proxy_test.delay_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Margin = new Padding(0),
        Dock = DockStyle.Fill
      };
      proxySettingsLayout.Controls.Add(delayLabel, 0, 0);

      DelayNumericUpDown = new NumericUpDown {
        Maximum = int.MaxValue,
        Anchor = AnchorStyles.Right | AnchorStyles.None,
        Margin = new Padding(0),
        AutoSize = true
      };
      proxySettingsLayout.Controls.Add(DelayNumericUpDown, 1, 0);

      Label requestsCountLabel = new Label {
        Text = Program.localization.GetString("settings_form.proxy_test.requests_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Margin = new Padding(0, 3, 0, 3),
        Dock = DockStyle.Fill
      };
      proxySettingsLayout.Controls.Add(requestsCountLabel, 0, 1);

      RequestsCountNumericUpDown = new NumericUpDown {
        Minimum = 1,
        Maximum = int.MaxValue,
        Anchor = AnchorStyles.Right | AnchorStyles.None,
        Margin = new Padding(0, 3, 0, 3),
        AutoSize = true
      };
      proxySettingsLayout.Controls.Add(RequestsCountNumericUpDown, 1, 1);

      FullLogCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.proxy_test.full_log"),
        Margin = new Padding(3, 3, 3, 0),
        Dock = DockStyle.Fill
      };
      proxySettingsLayout.SetColumnSpan(FullLogCheckBox, 2);
      proxySettingsLayout.Controls.Add(FullLogCheckBox, 0, 2);

      TableLayoutPanel buttonsLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        RowCount = 1,
        ColumnStyles = {
          new ColumnStyle(SizeType.Percent, 50F),
          new ColumnStyle(SizeType.Percent, 50F)
        },
        RowStyles = {
          new RowStyle(SizeType.AutoSize)
        },
        Margin = new Padding(0, 5, 0, 0),
        Name = "buttonsLayout"
      };
      proxySettingsLayout.SetColumnSpan(buttonsLayout, 2);
      proxySettingsLayout.Controls.Add(buttonsLayout, 0, 3);

      Button editDomainsButton = new Button {
        Text = Program.localization.GetString("settings_form.proxy_test.edit_domains"),
        Dock = DockStyle.Fill,
        Margin = new Padding(0, 0, 3, 0)
      };
      editDomainsButton.Click += EditDomainsButton_Click;
      buttonsLayout.Controls.Add(editDomainsButton, 0, 0);

      Button editCommandsButton = new Button {
        Text = Program.localization.GetString("settings_form.proxy_test.edit_commands"),
        Dock = DockStyle.Fill,
        Margin = new Padding(3, 0, 0, 0)
      };
      editCommandsButton.Click += EditCommandsButton_Click;
      buttonsLayout.Controls.Add(editCommandsButton, 1, 0);

      // Proxy Logs Group
      GroupBox proxyLogsGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.proxy_test.logs_group"),
        Name = "proxyLogsGroupBox",
        Dock = DockStyle.Fill,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10),
        Margin = new Padding(0, 5, 0, 0),
      };
      proxyTestTabLayout.Controls.Add(proxyLogsGroupBox, 0, 1);

      TableLayoutPanel proxyLogsLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        RowCount = 2,
        ColumnStyles = {
          new ColumnStyle(SizeType.AutoSize),
          new ColumnStyle(SizeType.Percent, 100F)
        },
        RowStyles = {
          new RowStyle(SizeType.Percent, 100F),
          new RowStyle(SizeType.AutoSize)
        },
        Name = "proxyLogsLayout"
      };
      proxyLogsGroupBox.Controls.Add(proxyLogsLayout);

      ProxyTestLogsBox = new TextBox {
        Text = ProxyTestManager.GetLatestLogs(),
        Name = "proxyLogsRichBox",
        BackColor = Color.White,
        ForeColor = Color.Black,
        ReadOnly = true,
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
        Margin = new Padding(0, 0, 0, 3)
      };
      proxyLogsLayout.SetColumnSpan(ProxyTestLogsBox, 2);
      proxyLogsLayout.Controls.Add(ProxyTestLogsBox, 0, 0);

      Button proxyTestStartButton = new Button {
        Text = Program.localization.GetString("settings_form.proxy_test.start"),
        Margin = new Padding(0, 3, 0, 0),
        AutoSize = true
      };
      proxyTestStartButton.Click += ProxyTestStartButton_Click;
      proxyLogsLayout.Controls.Add(proxyTestStartButton, 0, 1);

      ProxyTestProgressLabel = new Label {
        Text = "",
        Name = "proxyTestProgressLabel",
        TextAlign = ContentAlignment.MiddleLeft,
        Visible = false,
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
        Margin = new Padding(3, 3, 0, 0),
        AutoSize = false
      };
      proxyLogsLayout.Controls.Add(ProxyTestProgressLabel, 1, 1);
    }

    public void LoadSettings() {
      DelayNumericUpDown.Value = _settings.ProxyTestDelay;
      RequestsCountNumericUpDown.Value = _settings.ProxyTestRequestsCount;
      FullLogCheckBox.Checked = _settings.ProxyTestFullLog;
    }

    public void SaveSettings() {
      _settings.ProxyTestDelay = (int)DelayNumericUpDown.Value;
      _settings.ProxyTestRequestsCount = (int)RequestsCountNumericUpDown.Value;
      _settings.ProxyTestFullLog = FullLogCheckBox.Checked;
    }

    public void Cleanup() {
      if (_proxyTestManager.IsTesting) {
        _proxyTestManager.StopTesting();
        _proxyTestManager = null;
      }
    }

    private async void ProxyTestStartButton_Click(object sender, EventArgs e) {
      Button proxyTestStartButton = (Button)sender;
      if (!_proxyTestManager.IsTesting) {
        SaveSettings();
        _settings.Save();

        _proxyTestManager.ProxyTestStartButton = proxyTestStartButton;
        _proxyTestManager.ProxyTestLogsBox = ProxyTestLogsBox;
        _proxyTestManager.ProxyTestProgressLabel = ProxyTestProgressLabel;

        await _proxyTestManager.StartTesting();
      }
      else {
        _proxyTestManager.StopTesting();
      }
    }

    private void EditDomainsButton_Click(object sender, EventArgs e) {
      try {
        Directory.CreateDirectory(ProxyTestManager.PROXY_TEST_FOLDER);

        if (!File.Exists(ProxyTestManager.PROXY_TEST_SITES)) {
          File.WriteAllText(ProxyTestManager.PROXY_TEST_SITES, "");
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
          FileName = ProxyTestManager.PROXY_TEST_SITES,
          UseShellExecute = true
        });
      }
      catch (Exception) {
        Program.logger.Log(Program.localization.GetString("proxy_test.sites_file_read_error"));
      }
    }

    private void EditCommandsButton_Click(object sender, EventArgs e) {
      try {
        Directory.CreateDirectory(ProxyTestManager.PROXY_TEST_FOLDER);

        if (!File.Exists(ProxyTestManager.PROXY_TEST_CMDS)) {
          File.WriteAllText(ProxyTestManager.PROXY_TEST_CMDS, "");
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
          FileName = ProxyTestManager.PROXY_TEST_CMDS,
          UseShellExecute = true
        });
      }
      catch (Exception) {
        Program.logger.Log(Program.localization.GetString("proxy_test.cmds_file_not_found"));
      }
    }
  }
}
