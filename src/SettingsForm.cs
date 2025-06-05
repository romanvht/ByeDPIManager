using System;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;

namespace bdmanager {
  public partial class SettingsForm : Form {
    private AppSettings _settings;
    private AutorunManager _autorunManager;
    private ProxyTestManager _proxyTestManager;

    private TabControl _tabControl;

    private TextBox _byeDpiPathTextBox;
    private TextBox _byeDpiArgsTextBox;

    private TextBox _proxiFyrePathTextBox;
    private NumericUpDown _proxiFyrePortNumBox;
    private ListBox _appListBox;
    private TextBox _appTextBox;
    private RichTextBox _proxyLogsRichBox;
    private Label _proxyTestProgressLabel;

    private CheckBox _autoStartCheckBox;
    private CheckBox _autoConnectCheckBox;
    private CheckBox _StartMinimizedCheckBox;

    private NumericUpDown _delayNumericUpDown;
    private NumericUpDown _requestsCountNumericUpDown;
    private CheckBox _fullLogCheckBox;

    public SettingsForm() {
      _settings = Program.settings;
      _autorunManager = Program.autorunManager;
      _proxyTestManager = new ProxyTestManager();
      InitializeComponent();
    }

    private void InitializeComponent() {
      SuspendLayout();

      Text = Program.localization.GetString("settings_form.title");
      Size = new Size(510, 490);
      StartPosition = FormStartPosition.CenterParent;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      Load += SettingsForm_Load;
      FormClosing += SettingsForm_FormClosing;
      BackColor = SystemColors.Control;
      ForeColor = SystemColors.ControlText;

      // Tabs
      _tabControl = new TabControl {
        Location = new Point(15, 15),
        Size = new Size(460, 380),
        Name = "tabControl"
      };
      Controls.Add(_tabControl);

      // ByeDPI
      TabPage byeDpiTabPage = new TabPage {
        Text = Program.localization.GetString("settings_form.byedpi.tab"),
        Name = "byeDpiTabPage",
        BackColor = SystemColors.Control
      };
      _tabControl.TabPages.Add(byeDpiTabPage);

      GroupBox byeDpiGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.byedpi.group"),
        Location = new Point(10, 10),
        Size = new Size(430, 330),
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Name = "byeDpiGroupBox"
      };
      byeDpiTabPage.Controls.Add(byeDpiGroupBox);

      Label byeDpiPathLabel = new Label {
        Text = Program.localization.GetString("settings_form.byedpi.path_label"),
        Location = new Point(15, 25),
        Size = new Size(100, 20),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "byeDpiPathLabel"
      };
      byeDpiGroupBox.Controls.Add(byeDpiPathLabel);

      _byeDpiPathTextBox = new TextBox {
        Location = new Point(120, 25),
        Size = new Size(250, 50),
        Name = "byeDpiPathTextBox"
      };
      byeDpiGroupBox.Controls.Add(_byeDpiPathTextBox);

      Button byeDpiBrowseButton = new Button {
        Text = "...",
        Location = new Point(380, 24),
        Size = new Size(30, 23),
        Name = "byeDpiBrowseButton"
      };
      byeDpiBrowseButton.Click += (s, e) => BrowseForExe(_byeDpiPathTextBox,
        Program.localization.GetString("settings_form.byedpi.browse_title"));
      byeDpiGroupBox.Controls.Add(byeDpiBrowseButton);

      Label byeDpiArgsLabel = new Label {
        Text = Program.localization.GetString("settings_form.byedpi.args_label"),
        Location = new Point(15, 60),
        Size = new Size(100, 20),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "byeDpiArgsLabel"
      };
      byeDpiGroupBox.Controls.Add(byeDpiArgsLabel);

      _byeDpiArgsTextBox = new TextBox {
        Location = new Point(120, 60),
        Size = new Size(290, 250),
        Name = "byeDpiArgsTextBox",
        Multiline = true,
        ScrollBars = ScrollBars.Vertical
      };
      byeDpiGroupBox.Controls.Add(_byeDpiArgsTextBox);

      // ProxiFyre
      TabPage proxiFyreTabPage = new TabPage {
        Text = Program.localization.GetString("settings_form.proxifyre.tab"),
        Name = "proxiFyreTabPage",
        BackColor = SystemColors.Control
      };
      _tabControl.TabPages.Add(proxiFyreTabPage);

      GroupBox proxiFyreGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.proxifyre.group"),
        Location = new Point(10, 10),
        Size = new Size(430, 100),
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Name = "proxiFyreGroupBox"
      };
      proxiFyreTabPage.Controls.Add(proxiFyreGroupBox);

      Label proxiFyrePathLabel = new Label {
        Text = Program.localization.GetString("settings_form.proxifyre.path_label"),
        Location = new Point(15, 25),
        Size = new Size(100, 20),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "proxiFyrePathLabel"
      };
      proxiFyreGroupBox.Controls.Add(proxiFyrePathLabel);

      _proxiFyrePathTextBox = new TextBox {
        Location = new Point(120, 25),
        Size = new Size(250, 23),
        Name = "proxiFyrePathTextBox"
      };
      proxiFyreGroupBox.Controls.Add(_proxiFyrePathTextBox);

      Button proxiFyreBrowseButton = new Button {
        Text = "...",
        Location = new Point(380, 24),
        Size = new Size(30, 23),
        Name = "proxiFyreBrowseButton"
      };
      proxiFyreBrowseButton.Click += (s, e) => BrowseForExe(_proxiFyrePathTextBox,
        Program.localization.GetString("settings_form.proxifyre.browse_title"));
      proxiFyreGroupBox.Controls.Add(proxiFyreBrowseButton);

      Label proxiFyrePortLabel = new Label {
        Text = Program.localization.GetString("settings_form.proxifyre.port_label"),
        Location = new Point(15, 60),
        Size = new Size(100, 20),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "proxiFyrePortLabel"
      };
      proxiFyreGroupBox.Controls.Add(proxiFyrePortLabel);

      _proxiFyrePortNumBox = new NumericUpDown {
        Location = new Point(120, 60),
        Size = new Size(80, 23),
        Name = "proxiFyrePortNumBox",
        Minimum = 1,
        Maximum = 65535
      };
      proxiFyreGroupBox.Controls.Add(_proxiFyrePortNumBox);

      // Apps
      GroupBox appsGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.apps.group"),
        Location = new Point(10, 120),
        Size = new Size(430, 225),
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Name = "appsGroupBox"
      };
      proxiFyreTabPage.Controls.Add(appsGroupBox);

      _appListBox = new ListBox {
        Location = new Point(15, 25),
        Size = new Size(400, 130),
        Name = "appListBox"
      };
      appsGroupBox.Controls.Add(_appListBox);

      Label appNameLabel = new Label {
        Text = Program.localization.GetString("settings_form.apps.name_label"),
        Location = new Point(15, 155),
        Size = new Size(160, 20),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "appNameLabel"
      };
      appsGroupBox.Controls.Add(appNameLabel);

      _appTextBox = new TextBox {
        Location = new Point(175, 155),
        Size = new Size(200, 23),
        Name = "appTextBox"
      };
      appsGroupBox.Controls.Add(_appTextBox);

      Button appBrowseButton = new Button {
        Text = "...",
        Location = new Point(385, 154),
        Size = new Size(30, 23),
        Name = "appBrowseButton"
      };
      appBrowseButton.Click += (s, e) => {
        BrowseForExe(_appTextBox, Program.localization.GetString("settings_form.apps.browse_title"));
        if (!string.IsNullOrWhiteSpace(_appTextBox.Text)) {
          AddAppToList(_appTextBox.Text);
        }
      };
      appsGroupBox.Controls.Add(appBrowseButton);

      Button addAppButton = new Button {
        Text = Program.localization.GetString("settings_form.apps.add"),
        Location = new Point(15, 185),
        Size = new Size(80, 25),
        Name = "addAppButton"
      };
      addAppButton.Click += AddAppButton_Click;
      appsGroupBox.Controls.Add(addAppButton);

      Button removeAppButton = new Button {
        Text = Program.localization.GetString("settings_form.apps.remove"),
        Location = new Point(100, 185),
        Size = new Size(80, 25),
        Name = "removeAppButton"
      };
      removeAppButton.Click += RemoveAppButton_Click;
      appsGroupBox.Controls.Add(removeAppButton);

      // Autorun
      TabPage autorunTabPage = new TabPage {
        Text = Program.localization.GetString("settings_form.autorun.tab"),
        Name = "autorunTabPage",
        BackColor = SystemColors.Control
      };
      _tabControl.TabPages.Add(autorunTabPage);

      GroupBox autorunGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.autorun.group"),
        Location = new Point(10, 10),
        Size = new Size(430, 120),
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Name = "autorunGroupBox"
      };
      autorunTabPage.Controls.Add(autorunGroupBox);

      _autoStartCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.autorun.auto_start"),
        Location = new Point(15, 25),
        Size = new Size(400, 20),
        Name = "autoStartCheckBox"
      };
      autorunGroupBox.Controls.Add(_autoStartCheckBox);

      _autoConnectCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.autorun.auto_connect"),
        Location = new Point(15, 55),
        Size = new Size(400, 20),
        Name = "autoConnectCheckBox"
      };
      autorunGroupBox.Controls.Add(_autoConnectCheckBox);

      _StartMinimizedCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.autorun.start_minimized"),
        Location = new Point(15, 85),
        Size = new Size(400, 20),
        Name = "StartMinimizedCheckBox"
      };
      autorunGroupBox.Controls.Add(_StartMinimizedCheckBox);

      // ProxyTest
      TabPage proxyTestTabPage = new TabPage {
        Text = Program.localization.GetString("settings_form.proxy_test.tab"),
        Name = "proxyTestTabPage",
        BackColor = SystemColors.Control
      };
      _tabControl.TabPages.Add(proxyTestTabPage);

      GroupBox proxySettingsGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.proxy_test.settings_group"),
        Location = new Point(10, 10),
        Size = new Size(430, 120),
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Name = "proxySettingsGroupBox"
      };
      proxyTestTabPage.Controls.Add(proxySettingsGroupBox);

      Label delayLabel = new Label {
        Text = Program.localization.GetString("settings_form.proxy_test.delay_label"),
        Location = new Point(10, 20),
        Size = new Size(250, 20)
      };
      proxySettingsGroupBox.Controls.Add(delayLabel);

      _delayNumericUpDown = new NumericUpDown {
        Location = new Point(300, 18),
        Size = new Size(120, 20),
        Maximum = int.MaxValue
      };
      proxySettingsGroupBox.Controls.Add(_delayNumericUpDown);

      Label requestsCountLabel = new Label {
        Text = Program.localization.GetString("settings_form.proxy_test.requests_label"),
        Location = new Point(10, 50),
        Size = new Size(250, 20)
      };
      proxySettingsGroupBox.Controls.Add(requestsCountLabel);

      _requestsCountNumericUpDown = new NumericUpDown {
        Location = new Point(300, 48),
        Size = new Size(120, 20),
        Minimum = 1,
        Maximum = int.MaxValue
      };
      proxySettingsGroupBox.Controls.Add(_requestsCountNumericUpDown);

      _fullLogCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.proxy_test.full_log"),
        Location = new Point(10, 80),
        Size = new Size(400, 20)
      };
      proxySettingsGroupBox.Controls.Add(_fullLogCheckBox);

      GroupBox proxyLogsGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.proxy_test.logs_group"),
        Location = new Point(10, 140),
        Size = new Size(430, 170),
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Name = "proxyLogsGroupBox"
      };
      proxyTestTabPage.Controls.Add(proxyLogsGroupBox);

      _proxyLogsRichBox = new RichTextBox {
        ForeColor = SystemColors.ControlText,
        BorderStyle = BorderStyle.FixedSingle,
        Text = ProxyTestManager.GetLatestLogs(),
        Name = "proxyLogsRichBox",
        Dock = DockStyle.Fill,
        ReadOnly = true,
        ShortcutsEnabled = true,
        DetectUrls = false,
        EnableAutoDragDrop = false
      };
      proxyLogsGroupBox.Controls.Add(_proxyLogsRichBox);

      Button proxyTestStartButton = new Button {
        Text = Program.localization.GetString("settings_form.proxy_test.start"),
        Location = new Point(10, 317),
        Size = new Size(80, 25)
      };
      proxyTestStartButton.Click += ProxyTestStartButton_Click;
      proxyTestTabPage.Controls.Add(proxyTestStartButton);

      _proxyTestProgressLabel = new Label {
        Text = "",
        Location = new Point(100, 317),
        Size = new Size(100, 25),
        TextAlign = ContentAlignment.MiddleLeft,
        Visible = false,
        Name = "proxyTestProgressLabel"
      };
      proxyTestTabPage.Controls.Add(_proxyTestProgressLabel);

      // About
      TabPage aboutTabPage = new TabPage {
        Text = Program.localization.GetString("settings_form.about.tab"),
        Name = "aboutTabPage",
        BackColor = SystemColors.Control
      };
      _tabControl.TabPages.Add(aboutTabPage);

      GroupBox aboutGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.about.group"),
        Location = new Point(10, 10),
        Size = new Size(430, 200),
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Name = "aboutGroupBox"
      };
      aboutTabPage.Controls.Add(aboutGroupBox);

      Assembly asm = Assembly.GetExecutingAssembly();
      string version = asm.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
      string author = asm.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

      Label versionLabel = new Label {
        Text = Program.localization.GetString("settings_form.about.version_label"),
        Location = new Point(15, 30),
        Size = new Size(80, 20),
        Name = "versionLabel"
      };
      aboutGroupBox.Controls.Add(versionLabel);

      Label versionValueLabel = new Label {
        Text = version,
        Location = new Point(100, 30),
        Size = new Size(300, 20),
        Name = "versionValueLabel"
      };
      aboutGroupBox.Controls.Add(versionValueLabel);

      Label developerLabel = new Label {
        Text = Program.localization.GetString("settings_form.about.developer_label"),
        Location = new Point(15, 60),
        Size = new Size(80, 20),
        Name = "developerLabel"
      };
      aboutGroupBox.Controls.Add(developerLabel);

      Label developerValueLabel = new Label {
        Text = author,
        Location = new Point(100, 60),
        Size = new Size(300, 20),
        Name = "developerValueLabel"
      };
      aboutGroupBox.Controls.Add(developerValueLabel);

      Label githubLabel = new Label {
        Text = Program.localization.GetString("settings_form.about.github_label"),
        Location = new Point(15, 90),
        Size = new Size(80, 20),
        Name = "githubLabel"
      };
      aboutGroupBox.Controls.Add(githubLabel);

      LinkLabel githubLinkLabel = new LinkLabel {
        Text = Program.localization.GetString("settings_form.about.github_link"),
        Location = new Point(100, 90),
        Size = new Size(300, 20),
        Name = "githubLinkLabel"
      };
      githubLinkLabel.LinkClicked += (s, e) => { System.Diagnostics.Process.Start(githubLinkLabel.Text); };
      aboutGroupBox.Controls.Add(githubLinkLabel);

      // Form Buttons
      Button okButton = new Button {
        Text = Program.localization.GetString("settings_form.buttons.ok"),
        DialogResult = DialogResult.OK,
        Location = new Point(310, 410),
        Size = new Size(80, 30),
        Name = "okButton"
      };
      okButton.Click += OkButton_Click;
      Controls.Add(okButton);

      Button cancelButton = new Button {
        Text = Program.localization.GetString("settings_form.buttons.cancel"),
        DialogResult = DialogResult.Cancel,
        Location = new Point(400, 410),
        Size = new Size(80, 30),
        Name = "cancelButton"
      };
      Controls.Add(cancelButton);

      AcceptButton = okButton;
      CancelButton = cancelButton;

      ResumeLayout(false);
    }

    private async void ProxyTestStartButton_Click(object sender, EventArgs e) {
      Button proxyTestStartButton = (Button)sender;
      if (!_proxyTestManager.IsTesting) {
        OkButton_Click(sender, e);
        _settings.Save();

        _settings.ProxyTestDelay = (int)_delayNumericUpDown.Value;
        _settings.ProxyTestRequestsCount = (int)_requestsCountNumericUpDown.Value;
        _settings.ProxyTestFullLog = _fullLogCheckBox.Checked;
        _settings.Save();

        _proxyTestManager.ProxyTestStartButton = proxyTestStartButton;
        _proxyTestManager.ProxyLogsRichBox = _proxyLogsRichBox;
        _proxyTestManager.ProxyTestProgressLabel = _proxyTestProgressLabel;

        await _proxyTestManager.StartTesting();
      }
      else {
        _proxyTestManager.StopTesting();
      }
    }

    private void SettingsForm_Load(object sender, EventArgs e) {
      _byeDpiPathTextBox.Text = _settings.ByeDpiPath;
      _byeDpiArgsTextBox.Text = _settings.ByeDpiArguments;
      _proxiFyrePathTextBox.Text = _settings.ProxiFyrePath;
      _proxiFyrePortNumBox.Value = _settings.ProxiFyrePort;

      bool autorunEnabled = _autorunManager.IsAutorunEnabled();
      if (_settings.AutoStart != autorunEnabled) {
        _settings.AutoStart = autorunEnabled;
      }

      _autoStartCheckBox.Checked = _settings.AutoStart;
      _autoConnectCheckBox.Checked = _settings.AutoConnect;
      _StartMinimizedCheckBox.Checked = _settings.StartMinimized;

      _delayNumericUpDown.Value = _settings.ProxyTestDelay;
      _requestsCountNumericUpDown.Value = _settings.ProxyTestRequestsCount;
      _fullLogCheckBox.Checked = _settings.ProxyTestFullLog;

      _appListBox.Items.Clear();
      if (_settings.ProxifiedApps != null) {
        foreach (string app in _settings.ProxifiedApps) {
          _appListBox.Items.Add(app);
        }
      }
    }

    private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e) {
      if (_proxyTestManager.IsTesting) {
        MessageBox.Show(
          Program.localization.GetString("settings_form.proxy_test.warning"),
          Program.localization.GetString("settings_form.title"),
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
        );
        e.Cancel = true;
        return;
      }
    }

    private void OkButton_Click(object sender, EventArgs e) {
      _settings.ByeDpiPath = _byeDpiPathTextBox.Text;
      _settings.ByeDpiArguments = _byeDpiArgsTextBox.Text;
      _settings.ProxiFyrePath = _proxiFyrePathTextBox.Text;
      _settings.ProxiFyrePort = (int) _proxiFyrePortNumBox.Value;
      _settings.ProxifiedApps.Clear();

      foreach (string app in _appListBox.Items) {
        _settings.ProxifiedApps.Add(app);
      }

      _settings.AutoStart = _autoStartCheckBox.Checked;
      _settings.AutoConnect = _autoConnectCheckBox.Checked;
      _settings.StartMinimized = _StartMinimizedCheckBox.Checked;

      _settings.ProxyTestDelay = (int)_delayNumericUpDown.Value;
      _settings.ProxyTestRequestsCount = (int)_requestsCountNumericUpDown.Value;
      _settings.ProxyTestFullLog = _fullLogCheckBox.Checked;

      _autorunManager.SetAutorun(_settings.AutoStart);
    }

    private void BrowseForExe(TextBox targetTextBox, string title) {
      using (OpenFileDialog dialog = new OpenFileDialog()) {
        dialog.Filter = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*";
        dialog.Title = title;

        if (dialog.ShowDialog() == DialogResult.OK) {
          targetTextBox.Text = dialog.FileName;
        }
      }
    }

    private void AddAppToList(string appPath) {
      string appName = appPath.Trim();

      if (!_appListBox.Items.Contains(appName)) {
        _appListBox.Items.Add(appName);
        _appTextBox.Clear();
      } else {
        MessageBox.Show(
          Program.localization.GetString("settings_form.apps.already_added"),
          Program.localization.GetString("settings_form.title"),
          MessageBoxButtons.OK,
          MessageBoxIcon.Information
        );
      }
    }

    private void AddAppButton_Click(object sender, EventArgs e) {
      if (!string.IsNullOrWhiteSpace(_appTextBox.Text)) {
        AddAppToList(_appTextBox.Text);
      }
    }

    private void RemoveAppButton_Click(object sender, EventArgs e) {
      if (_appListBox.SelectedIndex >= 0) {
        _appListBox.Items.RemoveAt(_appListBox.SelectedIndex);
      }
    }
  }
}
