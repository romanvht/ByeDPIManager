using System;
using System.Drawing;
using System.Windows.Forms;
using bdmanager.Views.Tabs;

namespace bdmanager.Views {
  public partial class SettingsForm : Form {
    private AppSettings _settings;
    private AutorunManager _autorunManager;

    private TabControl _tabControl;

    private ByeDpiTab _byeDpiTab;
    private ProxiFyreTab _proxiFyreTab;
    private AutorunTab _autorunTab;
    private ProxyTestTab _proxyTestTab;
    private AboutTab _aboutTab;

    public SettingsForm() {
      _settings = Program.settings;
      _autorunManager = Program.autorunManager;
      InitializeComponent();

      DpiScaler.Scale(this);
    }

    private void InitializeComponent() {
      SuspendLayout();

      Text = Program.localization.GetString("settings_form.title");
      MinimumSize = new Size(550, 550);
      StartPosition = FormStartPosition.CenterParent;
      FormBorderStyle = FormBorderStyle.Sizable;
      MaximizeBox = true;
      MinimizeBox = true;
      ShowIcon = false;
      Load += SettingsForm_Load;
      FormClosing += SettingsForm_FormClosing;
      BackColor = SystemColors.Control;
      ForeColor = SystemColors.ControlText;
      Padding = new Padding(10);

      _tabControl = new TabControl {
        Name = "tabControl",
        Dock = DockStyle.Fill
      };
      Controls.Add(_tabControl);

      InitializeTabs();

      InitializeButtons();

      ResumeLayout(false);
      PerformLayout();
    }

    private void InitializeTabs() {
      _byeDpiTab = new ByeDpiTab(_settings);
      _tabControl.TabPages.Add(_byeDpiTab);

      _proxiFyreTab = new ProxiFyreTab(_settings);
      _tabControl.TabPages.Add(_proxiFyreTab);

      _autorunTab = new AutorunTab(_settings);
      _tabControl.TabPages.Add(_autorunTab);

      _proxyTestTab = new ProxyTestTab(_settings);
      _tabControl.TabPages.Add(_proxyTestTab);

      _aboutTab = new AboutTab();
      _tabControl.TabPages.Add(_aboutTab);
    }

    private void InitializeButtons() {
      FlowLayoutPanel formButtonsPanel = new FlowLayoutPanel {
        FlowDirection = FlowDirection.RightToLeft,
        Dock = DockStyle.Bottom,
        AutoSize = true,
        Padding = new Padding(0, 10, 0, 0),
        Name = "formButtonsPanel"
      };
      Controls.Add(formButtonsPanel);

      Button okButton = new Button {
        Text = Program.localization.GetString("settings_form.buttons.ok"),
        DialogResult = DialogResult.OK,
        Name = "okButton",
        Margin = new Padding(3),
        AutoSize = true
      };
      okButton.Click += OkButton_Click;
      formButtonsPanel.Controls.Add(okButton);

      Button cancelButton = new Button {
        Text = Program.localization.GetString("settings_form.buttons.cancel"),
        DialogResult = DialogResult.Cancel,
        Name = "cancelButton",
        Margin = new Padding(3),
        AutoSize = true
      };
      formButtonsPanel.Controls.Add(cancelButton);

      AcceptButton = okButton;
      CancelButton = cancelButton;
    }

    private void SettingsForm_Load(object sender, EventArgs e) {
      _byeDpiTab.LoadSettings();
      _proxiFyreTab.LoadSettings();
      _autorunTab.LoadSettings(_autorunManager);
      _proxyTestTab.LoadSettings();
    }

    private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e) {
      _proxyTestTab.Cleanup();
    }

    private void OkButton_Click(object sender, EventArgs e) {
      _byeDpiTab.SaveSettings();
      _proxiFyreTab.SaveSettings();
      _autorunTab.SaveSettings(_autorunManager);
      _proxyTestTab.SaveSettings();

      _settings.Save();

      Program.logger.Log(Program.localization.GetString("main_form.settings_saved"));
    }
  }
}
