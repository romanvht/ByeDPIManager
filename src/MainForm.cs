using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Linq;

namespace bdmanager {
  public partial class MainForm : Form {
    private bool _trayShow = false;

    private AppSettings _settings;
    private SettingsForm _settingsForm;
    private ProcessManager _processManager;
    private Logger _logger;

    private RoundButton _toggleButton;
    private RoundButton _settingsButton;
    private TextBox _logBox;
    private NotifyIcon _notifyIcon;
    private MenuItem _toggleMenuItem;
    private Panel _languagePanel;

    public MainForm() {
      InitializeApplication();
      InitializeComponent();
      InitializeLanguage();
      InitializeTray();

      DpiScaler.Scale(this);
    }

    private void InitializeApplication() {
      _settings = Program.settings;
      _settingsForm = new SettingsForm();

      _logger = Program.logger;
      _logger.LogAdded += (s, message) => AddLogToUi(message);

      _processManager = Program.processManager;
      _processManager.StatusChanged += (s, isRunning) => UpdateStatus(isRunning);

      Program.localization.LanguageChanged += (s, e) => UpdateLocale();
    }

    private void InitializeComponent() {
      SuspendLayout();

      Text = Program.localization.GetString("app_name");
      Size = new Size(480, 400);
      StartPosition = FormStartPosition.CenterScreen;
      FormBorderStyle = FormBorderStyle.FixedSingle;
      MaximizeBox = false;
      FormClosing += MainForm_FormClosing;
      Load += MainForm_Load;
      Icon = GetIconFromResources();

      BackColor = Color.FromArgb(30, 30, 30);
      ForeColor = Color.White;

      TableLayoutPanel mainLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 4,
        ColumnStyles = {
            new ColumnStyle(SizeType.Percent, 100F)
        },
        RowStyles = {
            new RowStyle(SizeType.Percent, 30F),
            new RowStyle(SizeType.Percent, 15F),
            new RowStyle(SizeType.Percent, 50F),
            new RowStyle(SizeType.Percent, 5F)
        },
        Padding = new Padding(10)
      };
      Controls.Add(mainLayout);

      _toggleButton = new RoundButton {
        Text = Program.localization.GetString("main_form.connect"),
        Size = new Size(160, 80),
        BackColor = Color.FromArgb(45, 45, 45),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 11, FontStyle.Bold),
        BorderRadius = 30,
        BorderSize = 1,
        BorderColor = Color.FromArgb(100, 100, 100),
        Anchor = AnchorStyles.None
      };
      _toggleButton.Click += ToggleButton_Click;
      mainLayout.Controls.Add(_toggleButton, 0, 0);

      _settingsButton = new RoundButton {
        Text = Program.localization.GetString("main_form.settings"),
        Size = new Size(120, 40),
        BackColor = Color.FromArgb(60, 60, 60),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 9),
        BorderRadius = 20,
        BorderSize = 1,
        BorderColor = Color.FromArgb(100, 100, 100),
        Margin = new Padding(0, 0, 0, 5),
        Anchor = AnchorStyles.None
      };
      _settingsButton.Click += SettingsButton_Click;
      mainLayout.Controls.Add(_settingsButton, 0, 1);

      Panel logPanel = new Panel {
        Dock = DockStyle.Fill,
        BackColor = Color.FromArgb(20, 20, 20),
        BorderStyle = BorderStyle.FixedSingle,
        Margin = new Padding(0, 5, 0, 5)
      };
      mainLayout.Controls.Add(logPanel, 0, 2);

      _logBox = new TextBox {
        Dock = DockStyle.Fill,
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Font = new Font("Consolas", 9),
        BackColor = Color.FromArgb(25, 25, 25),
        ForeColor = Color.LimeGreen,
        BorderStyle = BorderStyle.None,
        Margin = new Padding(5)
      };
      logPanel.Controls.Add(_logBox);

      _languagePanel = new TableLayoutPanel {
          Dock = DockStyle.Fill,
          BackColor = Color.FromArgb(30, 30, 30),
          ColumnCount = 1,
          RowCount = 1,
          Padding = new Padding(0),
          Margin = new Padding(0),
          ColumnStyles = { new ColumnStyle(SizeType.Percent, 100F) },
          RowStyles = { new RowStyle(SizeType.Percent, 100F) }
      };
      mainLayout.Controls.Add(_languagePanel, 0, 3);

      _notifyIcon = new NotifyIcon {
        Icon = GetIconFromResources(),
        Visible = true
      };

      ResumeLayout(false);
    }

    private void InitializeLanguage() {
      FlowLayoutPanel flowPanel = new FlowLayoutPanel {
        AutoSize = true,
        Anchor = AnchorStyles.None
      };

      foreach (string langCode in Localization.AvailableLanguages) {
        LinkLabel langLink = new LinkLabel {
          Text = Program.localization.GetLanguageName(langCode),
          LinkColor = Color.White,
          LinkBehavior = LinkBehavior.NeverUnderline,
          VisitedLinkColor = Color.White,
          ActiveLinkColor = Color.LightGray,
          AutoSize = true,
          Font = new Font("Segoe UI", 8),
          Tag = langCode,
          Margin = new Padding(0, 0, 2, 0)
        };

        langLink.Click += (s, e) => {
          if (Program.localization.CurrentLanguage != langCode) {
            Program.localization.ChangeLanguage(langCode);
            _settings.Language = langCode;
            _settings.Save();
          }
        };

        langLink.LinkColor = Program.localization.CurrentLanguage == langCode ?
            Color.LimeGreen :
            Color.White;

        flowPanel.Controls.Add(langLink);

        if (langCode != Localization.AvailableLanguages.Last()) {
          Label separator = new Label {
            Text = "|",
            ForeColor = Color.White,
            AutoSize = true,
            Font = new Font("Segoe UI", 8),
            Margin = new Padding(0, 0, 2, 0)
          };
          flowPanel.Controls.Add(separator);
        }
      }

      _languagePanel.Controls.Add(flowPanel);
    }

    private void UpdateLocale() {
      if (InvokeRequired) {
        Invoke(new Action(UpdateLocale));
        return;
      }

      Text = Program.localization.GetString("app_name");

      _toggleButton.Text = _processManager.IsRunning ?
        Program.localization.GetString("main_form.disconnect") :
        Program.localization.GetString("main_form.connect");

      _settingsButton.Text = Program.localization.GetString("main_form.settings");

      _notifyIcon.Text = Program.localization.GetString("app_name");

      _toggleMenuItem.Text = _processManager.IsRunning ?
        Program.localization.GetString("main_form.disconnect") :
        Program.localization.GetString("main_form.connect");

      if (_notifyIcon.ContextMenu != null) {
        _notifyIcon.ContextMenu.MenuItems[0].Text = Program.localization.GetString("tray_menu.open");
        _notifyIcon.ContextMenu.MenuItems[2].Text = Program.localization.GetString("tray_menu.exit");
      }

      foreach (Control control in _languagePanel.Controls) {
        if (control is FlowLayoutPanel flowPanel) {
          foreach (Control langControl in flowPanel.Controls) {
            if (langControl is LinkLabel link) {
              string langCode = (string)link.Tag;
              link.LinkColor = Program.localization.CurrentLanguage == langCode ? Color.LimeGreen : Color.White;
            }
          }
        }
      }
    }

    private void InitializeTray() {
      ContextMenu trayMenu = new ContextMenu();

      MenuItem openMenuItem = new MenuItem(Program.localization.GetString("tray_menu.open"));
      openMenuItem.Click += (s, e) => {
        Show();
        WindowState = FormWindowState.Normal;
      };

      _toggleMenuItem = new MenuItem(Program.localization.GetString("main_form.connect"));
      _toggleMenuItem.Click += (s, e) => {
        ToggleConnection();
      };

      MenuItem exitMenuItem = new MenuItem(Program.localization.GetString("tray_menu.exit"));
      exitMenuItem.Click += (s, e) => {
        _notifyIcon.Visible = false;
        Application.Exit();
      };

      trayMenu.MenuItems.Add(openMenuItem);
      trayMenu.MenuItems.Add(_toggleMenuItem);
      trayMenu.MenuItems.Add(exitMenuItem);

      _notifyIcon.ContextMenu = trayMenu;
      _notifyIcon.DoubleClick += (s, e) => {
        Show();
        Activate();
        WindowState = FormWindowState.Normal;
      };
    }

    private void MainForm_Load(object sender, EventArgs e) {
      _logger.Log(Program.localization.GetString("main_form.app_started"));
      _processManager.CleanupOnStartup();

      if (_settings.AutoStart && Program.isAutorun) {
        if (_settings.StartMinimized) {
          _logger.Log(Program.localization.GetString("main_form.quiet_mode"));
          WindowState = FormWindowState.Minimized;
          _trayShow = true;
          BeginInvoke(new Action(() => {
            Hide();
          }));
        }

        if (_settings.AutoConnect) {
          _logger.Log(Program.localization.GetString("main_form.auto_connect"));
          ToggleConnection();
        }
      }

      UpdateStatus(_processManager.IsRunning);
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
      if (e.CloseReason == CloseReason.UserClosing) {
        e.Cancel = true;
        WindowState = FormWindowState.Minimized;
        Hide();

        if (!_trayShow) {
          _notifyIcon.ShowBalloonTip(3000, Program.localization.GetString("app_name"),
            Program.localization.GetString("main_form.app_minimized"), ToolTipIcon.Info);
          _trayShow = true;
        }
      }
      else {
        Application.Exit();
      }
    }

    private void ToggleButton_Click(object sender, EventArgs e) {
      ToggleConnection();
    }

    private void SettingsButton_Click(object sender, EventArgs e) {
      if (_processManager.IsRunning) {
        MessageBox.Show(
            Program.localization.GetString("main_form.disconnect_first"),
            Program.localization.GetString("app_name"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );
        return;
      }

      OpenSettings();
    }

    private void ToggleConnection() {
      _toggleButton.Enabled = false;

      if (_processManager.IsRunning) {
        _processManager.Stop();
      }
      else {
        _processManager.Start();
      }

      _toggleButton.Enabled = true;
    }

    private void OpenSettings() {
      _settingsForm = new SettingsForm();

      if (_settingsForm.ShowDialog() == DialogResult.OK) {
        _settings.Save();
        _logger.Log(Program.localization.GetString("main_form.settings_saved"));
      }
    }

    private void AddLogToUi(string message) {
      if (_logBox.IsDisposed) return;

      if (_logBox.InvokeRequired) {
        _logBox.Invoke(new Action(() => AddLogToUi(message)));
        return;
      }

      string timestamp = DateTime.Now.ToString("HH:mm:ss");
      string logLine = $"[{timestamp}] {message}\r\n";

      if (_logBox.Lines.Length > 500) {
        _logBox.Text = string.Join("\r\n", _logBox.Lines.Skip(_logBox.Lines.Length - 500));
      }

      _logBox.AppendText(logLine);
      _logBox.ScrollToCaret();
    }

    private void UpdateStatus(bool isRunning) {
      if (InvokeRequired) {
        Invoke(new Action(() => UpdateStatus(isRunning)));
        return;
      }

      _toggleButton.Text = isRunning ?
        Program.localization.GetString("main_form.disconnect") :
        Program.localization.GetString("main_form.connect");

      _toggleMenuItem.Text = isRunning ?
        Program.localization.GetString("main_form.disconnect") :
        Program.localization.GetString("main_form.connect");

      _notifyIcon.Text = isRunning ?
        Program.localization.GetString("main_form.connected") :
        Program.localization.GetString("main_form.disconnected");

      _notifyIcon.Icon = isRunning ? GetIconFromResources(true) : GetIconFromResources();

      if (isRunning) {
        _toggleButton.BackColor = Color.FromArgb(200, 50, 50);
        _toggleButton.HoverColor = Color.FromArgb(220, 60, 60);
        _toggleButton.PressedColor = Color.FromArgb(180, 40, 40);
        _toggleButton.BorderColor = Color.FromArgb(240, 70, 70);
      }
      else {
        _toggleButton.BackColor = Color.FromArgb(40, 120, 50);
        _toggleButton.HoverColor = Color.FromArgb(50, 140, 60);
        _toggleButton.PressedColor = Color.FromArgb(30, 100, 40);
        _toggleButton.BorderColor = Color.FromArgb(60, 160, 70);
      }
    }

    private Icon GetIconFromResources(bool isConnected = false) {
      Assembly assembly = Assembly.GetExecutingAssembly();

      string resourceName = isConnected ?
        "bdmanager.tray-on.ico" :
        "bdmanager.tray-off.ico";

      using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
        if (stream != null) {
          return new Icon(stream);
        }
      }
      return SystemIcons.Application;
    }
  }
}
