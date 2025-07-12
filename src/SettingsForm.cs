using System;
using System.IO;
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

    private CheckBox _disableProxiFyreCheckBox;
    private TextBox _proxiFyrePathTextBox;
    private NumericUpDown _proxiFyrePortNumBox;
    private ListBox _appListBox;
    private TextBox _appTextBox;
    private TextBox _proxyTestLogsBox;
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

      // Tabs
      _tabControl = new TabControl {
        Name = "tabControl",
        Dock = DockStyle.Fill
      };
      Controls.Add(_tabControl);

      // Init Tabs
      AddByeDPI();
      AddProxiFyre();
      AddAutorun();
      AddProxyTest();
      AddAbout();

      // Buttons
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

      ResumeLayout(false);
      PerformLayout();
    }

    private void AddByeDPI() {
      TabPage byeDpiTabPage = new TabPage {
        Text = Program.localization.GetString("settings_form.byedpi.tab"),
        Name = "byeDpiTabPage",
        BackColor = SystemColors.Control,
        Padding = new Padding(10)
      };
      _tabControl.TabPages.Add(byeDpiTabPage);

      GroupBox byeDpiGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.byedpi.group"),
        Name = "byeDpiGroupBox",
        Dock = DockStyle.Fill,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10, 5, 10, 10)
      };
      byeDpiTabPage.Controls.Add(byeDpiGroupBox);

      TableLayoutPanel byeDpiLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 3,
        RowCount = 3,
        ColumnStyles = {
          new ColumnStyle(SizeType.AutoSize),
          new ColumnStyle(SizeType.Percent, 100F),
          new ColumnStyle(SizeType.Absolute, 30)
        },
        RowStyles = {
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.Percent, 100F)
        },
        Name = "byeDpiLayout"
      };
      byeDpiGroupBox.Controls.Add(byeDpiLayout);

      Label byeDpiPathLabel = new Label {
        Text = Program.localization.GetString("settings_form.byedpi.path_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "byeDpiPathLabel",
        Margin = new Padding(0),
      };
      byeDpiLayout.Controls.Add(byeDpiPathLabel, 0, 0);

      _byeDpiPathTextBox = new TextBox {
        Name = "byeDpiPathTextBox",
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
        Margin = new Padding(0)
      };
      byeDpiLayout.Controls.Add(_byeDpiPathTextBox, 1, 0);

      Button byeDpiBrowseButton = new Button {
        Text = "...",
        Anchor = AnchorStyles.None,
        Name = "byeDpiBrowseButton",
        Margin = new Padding(0),
      };
      byeDpiBrowseButton.Click += (s, e) => BrowseForExe(_byeDpiPathTextBox, Program.localization.GetString("settings_form.byedpi.browse_title"));
      byeDpiLayout.Controls.Add(byeDpiBrowseButton, 2, 0);

      Label byeDpiArgsLabel = new Label {
        Text = Program.localization.GetString("settings_form.byedpi.args_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "byeDpiArgsLabel",
        Margin = new Padding(0, 3, 0, 3),
      };
      byeDpiLayout.SetColumnSpan(byeDpiArgsLabel, 3);
      byeDpiLayout.Controls.Add(byeDpiArgsLabel, 0, 1);

      _byeDpiArgsTextBox = new TextBox {
        Name = "byeDpiArgsTextBox",
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
        Margin = new Padding(0, 3, 0, 3)
      };
      byeDpiLayout.SetColumnSpan(_byeDpiArgsTextBox, 3);
      byeDpiLayout.Controls.Add(_byeDpiArgsTextBox, 0, 2);
    }

    private void AddProxiFyre() {
      TabPage proxiFyreTabPage = new TabPage {
        Text = Program.localization.GetString("settings_form.proxifyre.tab"),
        Name = "proxiFyreTabPage",
        BackColor = SystemColors.Control,
        Padding = new Padding(10)
      };
      _tabControl.TabPages.Add(proxiFyreTabPage);

      TableLayoutPanel proxiFyreTabLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 2,
        RowStyles = {
            new RowStyle(SizeType.AutoSize),
            new RowStyle(SizeType.Percent, 100F)
        },
        Name = "proxiFyreTabLayout"
      };
      proxiFyreTabPage.Controls.Add(proxiFyreTabLayout);

      GroupBox proxiFyreGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.proxifyre.group"),
        Name = "proxiFyreGroupBox",
        Dock = DockStyle.Fill,
        MinimumSize = new Size(0, 110),
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10, 5, 10, 10),
        Margin = new Padding(0)
      };
      proxiFyreTabLayout.Controls.Add(proxiFyreGroupBox, 0, 0);

      TableLayoutPanel proxiFyreLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 3,
        RowCount = 3,
        ColumnStyles = {
              new ColumnStyle(SizeType.AutoSize),
              new ColumnStyle(SizeType.Percent, 100F),
              new ColumnStyle(SizeType.Absolute, 30)
          },
        RowStyles = {
              new RowStyle(SizeType.Percent, 100F),
              new RowStyle(SizeType.Percent, 100F),
              new RowStyle(SizeType.Percent, 100F)
          },
        Name = "proxiFyreLayout"
      };
      proxiFyreGroupBox.Controls.Add(proxiFyreLayout);

      Label proxiFyrePathLabel = new Label {
        Text = Program.localization.GetString("settings_form.proxifyre.path_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "proxiFyrePathLabel",
        Margin = new Padding(0),
      };
      proxiFyreLayout.Controls.Add(proxiFyrePathLabel, 0, 0);

      _proxiFyrePathTextBox = new TextBox {
        Name = "proxiFyrePathTextBox",
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
        Margin = new Padding(0)
      };
      proxiFyreLayout.Controls.Add(_proxiFyrePathTextBox, 1, 0);

      Button proxiFyreBrowseButton = new Button {
        Text = "...",
        Anchor = AnchorStyles.None,
        Name = "proxiFyreBrowseButton",
        Margin = new Padding(0),
      };
      proxiFyreBrowseButton.Click += (s, e) => BrowseForExe(_proxiFyrePathTextBox, Program.localization.GetString("settings_form.proxifyre.browse_title"));
      proxiFyreLayout.Controls.Add(proxiFyreBrowseButton, 2, 0);

      Label proxiFyrePortLabel = new Label {
        Text = Program.localization.GetString("settings_form.proxifyre.port_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "proxiFyrePortLabel",
        Margin = new Padding(0, 3, 0, 3),
      };
      proxiFyreLayout.Controls.Add(proxiFyrePortLabel, 0, 1);

      _proxiFyrePortNumBox = new NumericUpDown {
        Name = "proxiFyrePortNumBox",
        Minimum = 1,
        Maximum = 65535,
        Anchor = AnchorStyles.Left | AnchorStyles.None,
        Margin = new Padding(0),
        AutoSize = true
      };
      proxiFyreLayout.Controls.Add(_proxiFyrePortNumBox, 1, 1);

      _disableProxiFyreCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.proxifyre.disable"),
        Name = "disableProxiFyreCheckBox",
        Margin = new Padding(3, 3, 3, 0),
        Dock = DockStyle.Fill
      };
      proxiFyreLayout.SetColumnSpan(_disableProxiFyreCheckBox, 3);
      proxiFyreLayout.Controls.Add(_disableProxiFyreCheckBox, 0, 2);

      GroupBox appsGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.apps.group"),
        Name = "appsGroupBox",
        Dock = DockStyle.Fill,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10),
        Margin = new Padding(0, 5, 0, 0),
      };
      proxiFyreTabLayout.Controls.Add(appsGroupBox, 0, 1);

      TableLayoutPanel appsLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 3,
        RowCount = 4,
        ColumnStyles = {
              new ColumnStyle(SizeType.AutoSize),
              new ColumnStyle(SizeType.Percent, 100F),
              new ColumnStyle(SizeType.Absolute, 30)
          },
        RowStyles = {
              new RowStyle(SizeType.Percent, 100F),
              new RowStyle(SizeType.AutoSize),
              new RowStyle(SizeType.AutoSize),
              new RowStyle(SizeType.AutoSize)
          },
        Name = "appsLayout"
      };
      appsGroupBox.Controls.Add(appsLayout);

      _appListBox = new ListBox {
        Name = "appListBox",
        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
        Margin = new Padding(3)
      };
      appsLayout.SetColumnSpan(_appListBox, 3);
      appsLayout.Controls.Add(_appListBox, 0, 0);

      Label appNameLabel = new Label {
        Text = Program.localization.GetString("settings_form.apps.name_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "appNameLabel",
        Margin = new Padding(0, 3, 0, 3),
        AutoSize = true
      };
      appsLayout.SetColumnSpan(appNameLabel, 3);
      appsLayout.Controls.Add(appNameLabel, 0, 1);

      _appTextBox = new TextBox {
        Name = "appTextBox",
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
        Margin = new Padding(0, 3, 0, 3)
      };
      appsLayout.SetColumnSpan(_appTextBox, 2);
      appsLayout.Controls.Add(_appTextBox, 0, 2);

      Button appBrowseButton = new Button {
        Text = "...",
        Anchor = AnchorStyles.None,
        Name = "appBrowseButton",
        Margin = new Padding(0)
      };
      appBrowseButton.Click += (s, e) => {
        BrowseForExe(_appTextBox, Program.localization.GetString("settings_form.apps.browse_title"));
        if (!string.IsNullOrWhiteSpace(_appTextBox.Text)) {
          AddAppToList(_appTextBox.Text);
        }
      };
      appsLayout.Controls.Add(appBrowseButton, 1, 2);

      FlowLayoutPanel appButtonsPanel = new FlowLayoutPanel {
        FlowDirection = FlowDirection.RightToLeft,
        Dock = DockStyle.Fill,
        WrapContents = false,
        AutoSize = true,
        Name = "appButtonsPanel"
      };
      appsLayout.SetColumnSpan(appButtonsPanel, 3);
      appsLayout.Controls.Add(appButtonsPanel, 0, 3);

      Button addAppButton = new Button {
        Text = Program.localization.GetString("settings_form.apps.add"),
        Name = "addAppButton",
        Margin = new Padding(3, 3, 0, 3),
      };
      addAppButton.Click += AddAppButton_Click;
      appButtonsPanel.Controls.Add(addAppButton);

      Button removeAppButton = new Button {
        Text = Program.localization.GetString("settings_form.apps.remove"),
        Name = "removeAppButton",
        Margin = new Padding(3),
      };
      removeAppButton.Click += RemoveAppButton_Click;
      appButtonsPanel.Controls.Add(removeAppButton);
    }

    private void AddAutorun() {
      TabPage autorunTabPage = new TabPage {
        Text = Program.localization.GetString("settings_form.autorun.tab"),
        Name = "autorunTabPage",
        BackColor = SystemColors.Control,
        Padding = new Padding(10)
      };
      _tabControl.TabPages.Add(autorunTabPage);

      GroupBox autorunGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.autorun.group"),
        Name = "autorunGroupBox",
        Dock = DockStyle.Fill,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10, 5, 10, 10)
      };
      autorunTabPage.Controls.Add(autorunGroupBox);

      FlowLayoutPanel autorunLayout = new FlowLayoutPanel {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        AutoSize = true,
        WrapContents = false,
        Name = "autorunLayout"
      };
      autorunGroupBox.Controls.Add(autorunLayout);

      _autoStartCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.autorun.auto_start"),
        Name = "autoStartCheckBox",
        Margin = new Padding(3),
        AutoSize = true
      };
      autorunLayout.Controls.Add(_autoStartCheckBox);

      _autoConnectCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.autorun.auto_connect"),
        Name = "autoConnectCheckBox",
        Margin = new Padding(3),
        AutoSize = true
      };
      autorunLayout.Controls.Add(_autoConnectCheckBox);

      _StartMinimizedCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.autorun.start_minimized"),
        Name = "StartMinimizedCheckBox",
        Margin = new Padding(3),
        AutoSize = true
      };
      autorunLayout.Controls.Add(_StartMinimizedCheckBox);
    }

    private void AddProxyTest() {
      TabPage proxyTestTabPage = new TabPage {
        Text = Program.localization.GetString("settings_form.proxy_test.tab"),
        Name = "proxyTestTabPage",
        BackColor = SystemColors.Control,
        Padding = new Padding(10)
      };
      _tabControl.TabPages.Add(proxyTestTabPage);

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
      proxyTestTabPage.Controls.Add(proxyTestTabLayout);

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

      _delayNumericUpDown = new NumericUpDown {
        Maximum = int.MaxValue,
        Anchor = AnchorStyles.Right | AnchorStyles.None,
        Margin = new Padding(0),
        AutoSize = true
      };
      proxySettingsLayout.Controls.Add(_delayNumericUpDown, 1, 0);

      Label requestsCountLabel = new Label {
        Text = Program.localization.GetString("settings_form.proxy_test.requests_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Margin = new Padding(0, 3, 0, 3),
        Dock = DockStyle.Fill
      };
      proxySettingsLayout.Controls.Add(requestsCountLabel, 0, 1);

      _requestsCountNumericUpDown = new NumericUpDown {
        Minimum = 1,
        Maximum = int.MaxValue,
        Anchor = AnchorStyles.Right | AnchorStyles.None,
        Margin = new Padding(0, 3, 0, 3),
        AutoSize = true
      };
      proxySettingsLayout.Controls.Add(_requestsCountNumericUpDown, 1, 1);

      _fullLogCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.proxy_test.full_log"),
        Margin = new Padding(3, 3, 3, 0),
        Dock = DockStyle.Fill
      };
      proxySettingsLayout.SetColumnSpan(_fullLogCheckBox, 2);
      proxySettingsLayout.Controls.Add(_fullLogCheckBox, 0, 2);

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

      _proxyTestLogsBox = new TextBox {
        Text = ProxyTestManager.GetLatestLogs(),
        Name = "proxyLogsRichBox",
        ReadOnly = true,
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
        Margin = new Padding(0, 0, 0, 3)
      };
      proxyLogsLayout.SetColumnSpan(_proxyTestLogsBox, 2);
      proxyLogsLayout.Controls.Add(_proxyTestLogsBox, 0, 0);

      Button proxyTestStartButton = new Button {
        Text = Program.localization.GetString("settings_form.proxy_test.start"),
        Margin = new Padding(0, 3, 0, 0),
        AutoSize = true
      };
      proxyTestStartButton.Click += ProxyTestStartButton_Click;
      proxyLogsLayout.Controls.Add(proxyTestStartButton, 0, 1);

      _proxyTestProgressLabel = new Label {
        Text = "",
        Name = "proxyTestProgressLabel",
        TextAlign = ContentAlignment.MiddleLeft,
        Visible = false,
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
        Margin = new Padding(3, 3, 0, 0),
        AutoSize = false
      };
      proxyLogsLayout.Controls.Add(_proxyTestProgressLabel, 1, 1);
    }

    private void AddAbout() {
      TabPage aboutTabPage = new TabPage {
        Text = Program.localization.GetString("settings_form.about.tab"),
        Name = "aboutTabPage",
        BackColor = SystemColors.Control,
        Padding = new Padding(10)
      };
      _tabControl.TabPages.Add(aboutTabPage);

      GroupBox aboutGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.about.group"),
        Name = "aboutGroupBox",
        Dock = DockStyle.Fill,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10)
      };
      aboutTabPage.Controls.Add(aboutGroupBox);

      TableLayoutPanel aboutLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        RowCount = 3,
        ColumnStyles = {
              new ColumnStyle(SizeType.AutoSize),
              new ColumnStyle(SizeType.Percent, 100F)
          },
        RowStyles = {
              new RowStyle(SizeType.AutoSize),
              new RowStyle(SizeType.AutoSize),
              new RowStyle(SizeType.AutoSize)
          },
        Name = "aboutLayout",
        AutoSize = true
      };
      aboutGroupBox.Controls.Add(aboutLayout);

      Assembly asm = Assembly.GetExecutingAssembly();
      string version = asm.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
      string author = asm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

      Label versionLabel = new Label {
        Text = Program.localization.GetString("settings_form.about.version_label"),
        Name = "versionLabel",
        Margin = new Padding(0, 3, 0, 3),
      };
      aboutLayout.Controls.Add(versionLabel, 0, 0);

      Label versionValueLabel = new Label {
        Text = version,
        Name = "versionValueLabel",
        Margin = new Padding(10, 3, 0, 3),
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
      };
      aboutLayout.Controls.Add(versionValueLabel, 1, 0);

      Label developerLabel = new Label {
        Text = Program.localization.GetString("settings_form.about.developer_label"),
        Name = "developerLabel",
        Margin = new Padding(0, 3, 0, 3),
      };
      aboutLayout.Controls.Add(developerLabel, 0, 1);

      Label developerValueLabel = new Label {
        Text = author,
        Name = "developerValueLabel",
        Margin = new Padding(10, 3, 0, 3),
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
      };
      aboutLayout.Controls.Add(developerValueLabel, 1, 1);

      Label githubLabel = new Label {
        Text = Program.localization.GetString("settings_form.about.github_label"),
        Name = "githubLabel",
        Margin = new Padding(0, 3, 0, 3),
      };
      aboutLayout.Controls.Add(githubLabel, 0, 2);

      LinkLabel githubLinkLabel = new LinkLabel {
        Text = Program.localization.GetString("settings_form.about.github_link"),
        Name = "githubLinkLabel",
        Margin = new Padding(10, 3, 0, 3),
        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
      };
      githubLinkLabel.LinkClicked += (s, e) => { System.Diagnostics.Process.Start(githubLinkLabel.Text); };
      aboutLayout.Controls.Add(githubLinkLabel, 1, 2);
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
        _proxyTestManager.ProxyTestLogsBox = _proxyTestLogsBox;
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
      _disableProxiFyreCheckBox.Checked = _settings.DisableProxiFyre;
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
        _proxyTestManager.StopTesting();
        _proxyTestManager = null;
      }
    }

    private void OkButton_Click(object sender, EventArgs e) {
      _settings.ByeDpiPath = _byeDpiPathTextBox.Text;
      _settings.ByeDpiArguments = _byeDpiArgsTextBox.Text;
      _settings.DisableProxiFyre = _disableProxiFyreCheckBox.Checked;
      _settings.ProxiFyrePath = _proxiFyrePathTextBox.Text;
      _settings.ProxiFyrePort = (int)_proxiFyrePortNumBox.Value;
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
        dialog.Filter = "*.exe|*.exe|*.*|*.*";
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
      }
      else {
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
