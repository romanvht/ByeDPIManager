using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Management;

namespace bdmanager {
  public partial class MainForm : Form {
    private string _appName = Program.appName;
    private bool _trayShow = false;
    private AppSettings _settings;
    private SettingsForm _settingsForm;
    private ProcessManager _processManager;
    private Logger _logger;
    private RoundButton _toggleButton;
    private RichTextBox _logBox;
    private NotifyIcon _notifyIcon;
    private MenuItem _toggleMenuItem;

    public MainForm() {
      InitializeApplication();
      InitializeComponent();
      InitializeTray();
    }

    private void InitializeApplication() {
      _settings = Program.settings;
      _settingsForm = new SettingsForm();

      _logger = Program.logger;
      _logger.LogAdded += (s, message) => AddLogToUi(message);

      _processManager = Program.processManager;
      _processManager.LogMessage += (s, message) => _logger.Log(message);
      _processManager.StatusChanged += (s, isRunning) => UpdateStatus(isRunning);
    }

    private void InitializeComponent() {
      SuspendLayout();

      Text = _appName;
      Size = new Size(480, 380);
      StartPosition = FormStartPosition.CenterScreen;
      FormBorderStyle = FormBorderStyle.FixedSingle;
      MaximizeBox = false;
      FormClosing += MainForm_FormClosing;
      Load += MainForm_Load;
      Icon = GetIconFromResources();

      BackColor = Color.FromArgb(30, 30, 30);
      ForeColor = Color.White;

      _toggleButton = new RoundButton {
        Text = "Подключить",
        Size = new Size(160, 80),
        Location = new Point((ClientSize.Width - 160) / 2, 40),
        BackColor = Color.FromArgb(45, 45, 45),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 11, FontStyle.Bold),
        BorderRadius = 30,
        BorderSize = 1,
        BorderColor = Color.FromArgb(100, 100, 100)
      };
      _toggleButton.Click += ToggleButton_Click;
      Controls.Add(_toggleButton);

      RoundButton settingsButton = new RoundButton {
        Text = "Настройки",
        Size = new Size(120, 40),
        Location = new Point((ClientSize.Width - 120) / 2, 140),
        BackColor = Color.FromArgb(60, 60, 60),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 9),
        BorderRadius = 20,
        BorderSize = 1,
        BorderColor = Color.FromArgb(100, 100, 100)
      };
      settingsButton.Click += SettingsButton_Click;
      Controls.Add(settingsButton);

      Panel logPanel = new Panel {
        Size = new Size(ClientSize.Width - 20, 120),
        Location = new Point(10, ClientSize.Height - 130),
        BackColor = Color.FromArgb(20, 20, 20),
        BorderStyle = BorderStyle.FixedSingle
      };
      Controls.Add(logPanel);

      _logBox = new RichTextBox {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        Font = new Font("Consolas", 9),
        BackColor = Color.FromArgb(25, 25, 25),
        ForeColor = Color.LimeGreen,
        BorderStyle = BorderStyle.None,
        Margin = new Padding(5)
      };
      logPanel.Controls.Add(_logBox);

      ResumeLayout(false);
    }

    private void InitializeTray() {
      _notifyIcon = new NotifyIcon {
        Icon = GetIconFromResources(),
        Text = _appName,
        Visible = true
      };

      ContextMenu trayMenu = new ContextMenu();

      MenuItem openMenuItem = new MenuItem("Открыть");
      openMenuItem.Click += (s, e) => {
        Show();
        WindowState = FormWindowState.Normal;
      };

      _toggleMenuItem = new MenuItem("Подключить");
      _toggleMenuItem.Click += (s, e) => {
        ToggleConnection();
      };

      MenuItem exitMenuItem = new MenuItem("Выход");
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
        WindowState = FormWindowState.Normal;
      };
    }

    private void MainForm_Load(object sender, EventArgs e) {
      _logger.Log("Приложение запущено");
      _processManager.CleanupOnStartup();
  
      if (_settings.AutoStart && Program.isAutorun) {
        if (_settings.StartMinimized) {
          _logger.Log("Запуск в тихом режиме");
          WindowState = FormWindowState.Minimized;
          Hide();
          _trayShow = true;
        }

        if (_settings.AutoConnect) {
          _logger.Log("Автоматическое подключение...");
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
          _notifyIcon.ShowBalloonTip(3000, _appName, "Приложение свернуто в трей", ToolTipIcon.Info);
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
        _logger.Log("Настройки сохранены");
      }
    }

    private void AddLogToUi(string message) {
      if (_logBox.IsDisposed) return;

      if (_logBox.InvokeRequired) {
        _logBox.Invoke(new Action(() => AddLogToUi(message)));
        return;
      }

      string timestamp = DateTime.Now.ToString("HH:mm:ss");
      string logLine = $"[{timestamp}] {message}\n";

      if (_logBox.Lines.Length > 500) {
        _logBox.Text = string.Join("\n", _logBox.Lines.Skip(_logBox.Lines.Length - 500));
      }

      _logBox.AppendText(logLine);
      _logBox.ScrollToCaret();
    }

    private void UpdateStatus(bool isRunning) {
      if (InvokeRequired) {
        Invoke(new Action(() => UpdateStatus(isRunning)));
        return;
      }

      _toggleButton.Text = isRunning ? "Отключить" : "Подключить";
      _toggleMenuItem.Text = isRunning ? "Отключить" : "Подключить";
      _notifyIcon.Text = $"{_appName}: {(isRunning ? "Подключено" : "Отключено")}";

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

    private Icon GetIconFromResources() {
      Assembly assembly = Assembly.GetExecutingAssembly();
      string resourceName = "bdmanager.icon.ico";

      using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
        if (stream != null) {
          return new Icon(stream);
        }
      }

      return SystemIcons.Application;
    }
  }
}
