using bdmanager.src.ProxyTest;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace bdmanager {
  public partial class SettingsForm : Form {
    private AppSettings _settings;
    private AutorunManager _autorunManager;

    private TabControl _tabControl;

    private TextBox _byeDpiPathTextBox;
    private TextBox _byeDpiArgsTextBox;

    private TextBox _proxiFyrePathTextBox;
    private NumericUpDown _proxiFyrePortNumBox;
    private ListBox _appListBox;
    private TextBox _appTextBox;
    private RichTextBox _proxyLogsRichBox;

    private CheckBox _autoStartCheckBox;
    private CheckBox _autoConnectCheckBox;
    private CheckBox _StartMinimizedCheckBox;

    public SettingsForm() {
      _settings = Program.settings;
      _autorunManager = Program.autorunManager;
      InitializeComponent();
    }

    private void InitializeComponent() {
      SuspendLayout();

      Text = "Настройки";
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
        Text = "ByeDPI",
        Name = "byeDpiTabPage",
        BackColor = SystemColors.Control
      };
      _tabControl.TabPages.Add(byeDpiTabPage);

      GroupBox byeDpiGroupBox = new GroupBox {
        Text = "Настройки ByeDPI",
        Location = new Point(10, 10),
        Size = new Size(430, 330),
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Name = "byeDpiGroupBox"
      };
      byeDpiTabPage.Controls.Add(byeDpiGroupBox);

      Label byeDpiPathLabel = new Label {
        Text = "Путь к ByeDPI:",
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
      byeDpiBrowseButton.Click += ByeDpiBrowseButton_Click;
      byeDpiGroupBox.Controls.Add(byeDpiBrowseButton);

      Label byeDpiArgsLabel = new Label {
        Text = "Аргументы:",
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
        Text = "ProxiFyre",
        Name = "proxiFyreTabPage",
        BackColor = SystemColors.Control
      };
      _tabControl.TabPages.Add(proxiFyreTabPage);

      GroupBox proxiFyreGroupBox = new GroupBox {
        Text = "Настройки ProxiFyre",
        Location = new Point(10, 10),
        Size = new Size(430, 100),
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Name = "proxiFyreGroupBox"
      };
      proxiFyreTabPage.Controls.Add(proxiFyreGroupBox);

      Label proxiFyrePathLabel = new Label {
        Text = "Путь к ProxiFyre:",
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
      proxiFyreBrowseButton.Click += ProxiFyreBrowseButton_Click;
      proxiFyreGroupBox.Controls.Add(proxiFyreBrowseButton);

      Label proxiFyrePortLabel = new Label {
        Text = "Порт:",
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
        Text = "Приложения, которые пойдут через прокси",
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
        Text = "Имя или путь к приложению:",
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
      appBrowseButton.Click += AppBrowseButton_Click;
      appsGroupBox.Controls.Add(appBrowseButton);

      Button addAppButton = new Button {
        Text = "Добавить",
        Location = new Point(15, 185),
        Size = new Size(80, 25),
        Name = "addAppButton"
      };
      addAppButton.Click += AddAppButton_Click;
      appsGroupBox.Controls.Add(addAppButton);

      Button removeAppButton = new Button {
        Text = "Удалить",
        Location = new Point(100, 185),
        Size = new Size(80, 25),
        Name = "removeAppButton"
      };
      removeAppButton.Click += RemoveAppButton_Click;
      appsGroupBox.Controls.Add(removeAppButton);

      // Autorun
      TabPage autorunTabPage = new TabPage {
        Text = "Автозапуск",
        Name = "autorunTabPage",
        BackColor = SystemColors.Control
      };
      _tabControl.TabPages.Add(autorunTabPage);

      GroupBox autorunGroupBox = new GroupBox {
        Text = "Настройки автозапуска",
        Location = new Point(10, 10),
        Size = new Size(430, 120),
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Name = "autorunGroupBox"
      };
      autorunTabPage.Controls.Add(autorunGroupBox);

      _autoStartCheckBox = new CheckBox {
        Text = "Автозапуск при старте устройства",
        Location = new Point(15, 25),
        Size = new Size(400, 20),
        Name = "autoStartCheckBox"
      };
      autorunGroupBox.Controls.Add(_autoStartCheckBox);

      _autoConnectCheckBox = new CheckBox {
        Text = "Автоматическое подключение",
        Location = new Point(15, 55),
        Size = new Size(400, 20),
        Name = "autoConnectCheckBox"
      };
      autorunGroupBox.Controls.Add(_autoConnectCheckBox);

      _StartMinimizedCheckBox = new CheckBox {
        Text = "Запускать свернутым в трей",
        Location = new Point(15, 85),
        Size = new Size(400, 20),
        Name = "StartMinimizedCheckBox"
      };
      autorunGroupBox.Controls.Add(_StartMinimizedCheckBox);

      // ProxyTest
      TabPage proxyTestTabPage = new TabPage {
        Text = "Подбор команд (Beta)",
        Name = "proxyTestTabPage",
        BackColor = SystemColors.Control
      };
      _tabControl.TabPages.Add(proxyTestTabPage);

      GroupBox proxyLogsGroupBox = new GroupBox {
        Text = "Логи",
        Location = new Point(10, 10),
        Size = new Size(430, 300),
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
        ReadOnly = true
      };
      proxyLogsGroupBox.Controls.Add(_proxyLogsRichBox);

      Button proxyTestStartButton = new Button {
        Text = ProxyTestManager.PROXY_TEST_BUTTON_START,
        Location = new Point(10, 317),
        Size = new Size(80, 25)
      };
      proxyTestStartButton.Click += ProxyTestStartButton_Click;
      proxyTestTabPage.Controls.Add(proxyTestStartButton);

      Button proxyTestSettingsButton = new Button {
        Text = "Настройки",
        Location = new Point(proxyTestStartButton.Location.X + proxyTestStartButton.Size.Width + 8, 317),
        Size = new Size(80, 25)
      };
      proxyTestSettingsButton.Click += ProxyTestSettingsButton_Click;
      proxyTestTabPage.Controls.Add(proxyTestSettingsButton);

      // Form Buttons
      Button okButton = new Button {
        Text = "ОК",
        DialogResult = DialogResult.OK,
        Location = new Point(310, 410),
        Size = new Size(80, 30),
        Name = "okButton"
      };
      okButton.Click += OkButton_Click;
      Controls.Add(okButton);

      Button cancelButton = new Button {
        Text = "Отмена",
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

    private void ProxyTestSettingsButton_Click(object sender, EventArgs e) {
      if (Program.proxyTestManager.IsTesting) {
        MessageBox.Show("Перед настройкой остановите подбор команд.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      ProxyTestSettingsForm proxyTestSettingsForm = new ProxyTestSettingsForm();
      proxyTestSettingsForm.ShowDialog();
    }

    private async void ProxyTestStartButton_Click(object sender, EventArgs e) {
      Button proxyTestStartButton = (Button)sender;
      if (!Program.proxyTestManager.IsTesting) {
        ProxyTestSettings proxyTestSettings = ProxyTestSettingsForm.GetSettings();
        Program.proxyTestManager.ProxyTestStartButton = proxyTestStartButton;
        Program.proxyTestManager.ProxyLogsRichBox = _proxyLogsRichBox;

        await Program.proxyTestManager.StartTesting(proxyTestSettings);
      }
      else {
        Program.proxyTestManager.StopTesting();
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

      _appListBox.Items.Clear();
      if (_settings.ProxifiedApps != null) {
        foreach (string app in _settings.ProxifiedApps) {
          _appListBox.Items.Add(app);
        }
      }
    }

    private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e) {
      if (Program.proxyTestManager.IsTesting) {
        MessageBox.Show("Сначала остановите подбор команд.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

    private void ByeDpiBrowseButton_Click(object sender, EventArgs e) {
      BrowseForExe(_byeDpiPathTextBox, "Выберите исполняемый файл ByeDPI");
    }

    private void ProxiFyreBrowseButton_Click(object sender, EventArgs e) {
      BrowseForExe(_proxiFyrePathTextBox, "Выберите исполняемый файл ProxiFyre");
    }

    private void AppBrowseButton_Click(object sender, EventArgs e) {
      BrowseForExe(_appTextBox, "Выберите приложение");

      if (!string.IsNullOrWhiteSpace(_appTextBox.Text)) {
        AddAppToList(_appTextBox.Text);
      }
    }

    private void AddAppToList(string appPath) {
      string appName = appPath.Trim();

      if (!_appListBox.Items.Contains(appName)) {
        _appListBox.Items.Add(appName);
        _appTextBox.Clear();
      } else {
        MessageBox.Show("Это приложение уже добавлено в список.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
