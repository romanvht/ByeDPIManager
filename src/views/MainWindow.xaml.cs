using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace bdmanager.Views {
  public partial class MainWindow : Window {
    private const int MaxPendingLogLines = 2000;
    private const int MaxLogLinesPerFlush = 250;
    private const int MaxVisibleLogChars = 50000;
    private const int RetainedLogChars = 40000;

    private readonly AppSettings _settings;
    private readonly ProcessManager _processManager;
    private readonly Logger _logger;
    private readonly object _logQueueLock = new object();
    private readonly Queue<string> _pendingLogLines = new Queue<string>();
    private Forms.NotifyIcon _notifyIcon;
    private Forms.MenuItem _toggleMenuItem;
    private HotkeyManager _hotkeyManager;
    private HwndSource _windowSource;
    private DispatcherTimer _logFlushTimer;
    private int _droppedLogLines;
    private bool _trayTipShown;
    private bool _allowClose;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr windowHandle);

    public MainWindow() {
      InitializeComponent();
      DarkTitleBar.Apply(this);
      _settings = Program.settings;
      _processManager = Program.processManager;
      _logger = Program.logger;

      _logger.LogAdded += Logger_LogAdded;
      _processManager.StatusChanged += ProcessManager_StatusChanged;
      _processManager.ProxiFyreUnexpectedStopped += ProcessManager_ProxiFyreUnexpectedStopped;
      Program.localization.LanguageChanged += Localization_LanguageChanged;

      InitializeLogFlushTimer();
      InitializeTray();
      UpdateLocale();
      UpdateStatus(_processManager.IsRunning);
    }

    private void InitializeTray() {
      Forms.ContextMenu trayMenu = new Forms.ContextMenu();
      Forms.MenuItem openItem = new Forms.MenuItem();
      openItem.Click += (sender, args) => Dispatcher.BeginInvoke(new Action(ShowMainWindow));

      _toggleMenuItem = new Forms.MenuItem();
      _toggleMenuItem.Click += (sender, args) => Dispatcher.BeginInvoke(new Action(ToggleConnection));

      Forms.MenuItem exitItem = new Forms.MenuItem();
      exitItem.Click += (sender, args) => Dispatcher.BeginInvoke(new Action(ExitApplication));

      trayMenu.MenuItems.Add(openItem);
      trayMenu.MenuItems.Add(_toggleMenuItem);
      trayMenu.MenuItems.Add(exitItem);

      _notifyIcon = new Forms.NotifyIcon {
        Icon = GetTrayIcon(false),
        Visible = true,
        ContextMenu = trayMenu
      };
      _notifyIcon.DoubleClick += (sender, args) => Dispatcher.BeginInvoke(new Action(ShowMainWindow));
    }

    private void MainWindow_SourceInitialized(object sender, EventArgs e) {
      IntPtr handle = new WindowInteropHelper(this).Handle;
      _windowSource = HwndSource.FromHwnd(handle);
      _windowSource?.AddHook(WindowMessageHook);
      _hotkeyManager = new HotkeyManager(handle, ToggleConnection);
      RegisterConfiguredHotkey();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
      _logger.Log(Program.localization.GetString("main_form.app_started"));
      _processManager.CleanupOnStartup();

      if (_settings.AutoStart && Program.isAutorun && _settings.StartMinimized) {
        _logger.Log(Program.localization.GetString("main_form.quiet_mode"));
        Hide();
        _trayTipShown = true;
      }

      if (_settings.AutoConnect) {
        _logger.Log(Program.localization.GetString("main_form.auto_connect"));
        ToggleConnection();
      }
    }

    private IntPtr WindowMessageHook(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled) {
      _hotkeyManager?.ProcessMessage(message, wParam);
      return IntPtr.Zero;
    }

    private void RegisterConfiguredHotkey() {
      _hotkeyManager?.UnregisterCurrentHotkey();
      if (!string.IsNullOrWhiteSpace(_settings.Hotkey)) {
        _hotkeyManager?.RegisterHotkey(_settings.Hotkey);
      }
    }

    private void MainWindow_Closing(object sender, CancelEventArgs e) {
      if (!_allowClose && _settings.MinimizeToTray) {
        e.Cancel = true;
        Hide();

        if (!_trayTipShown) {
          _notifyIcon.ShowBalloonTip(
            3000,
            Program.localization.GetString("app_name"),
            Program.localization.GetString("main_form.app_minimized"),
            Forms.ToolTipIcon.Info
          );
          _trayTipShown = true;
        }
        return;
      }

      _hotkeyManager?.Dispose();
      _windowSource?.RemoveHook(WindowMessageHook);
      _logFlushTimer?.Stop();
      _logger.LogAdded -= Logger_LogAdded;
      _notifyIcon.Visible = false;
      _notifyIcon.Dispose();
    }

    public void ShowMainWindow() {
      if (!IsVisible) Show();
      if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
      Activate();
      Topmost = true;
      Topmost = false;
      Focus();
      SetForegroundWindow(new WindowInteropHelper(this).Handle);
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e) {
      ToggleConnection();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e) {
      if (_processManager.IsRunning) {
        MessageBox.Show(
          this,
          Program.localization.GetString("main_form.disconnect_first"),
          Program.localization.GetString("app_name"),
          MessageBoxButton.OK,
          MessageBoxImage.Warning
        );
        return;
      }

      SettingsWindow settingsWindow = new SettingsWindow { Owner = this };
      if (settingsWindow.ShowDialog() == true) RegisterConfiguredHotkey();
    }

    private void ToggleConnection() {
      if (!Dispatcher.CheckAccess()) {
        Dispatcher.BeginInvoke(new Action(ToggleConnection));
        return;
      }

      ToggleButton.IsEnabled = false;
      try {
        if (_processManager.IsRunning) _processManager.Stop();
        else _processManager.Start();
      }
      finally {
        ToggleButton.IsEnabled = true;
        UpdateStatus(_processManager.IsRunning);
      }
    }

    private void Logger_LogAdded(object sender, string message) {
      string line = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine;
      lock (_logQueueLock) {
        if (_pendingLogLines.Count >= MaxPendingLogLines) {
          _pendingLogLines.Dequeue();
          _droppedLogLines++;
        }
        _pendingLogLines.Enqueue(line);
      }
    }

    private void InitializeLogFlushTimer() {
      _logFlushTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher) {
        Interval = TimeSpan.FromMilliseconds(100)
      };
      _logFlushTimer.Tick += (sender, args) => FlushPendingLogs();
      _logFlushTimer.Start();
    }

    private void FlushPendingLogs() {
      List<string> lines = new List<string>(MaxLogLinesPerFlush);
      int droppedLines;

      lock (_logQueueLock) {
        droppedLines = _droppedLogLines;
        _droppedLogLines = 0;
        while (lines.Count < MaxLogLinesPerFlush && _pendingLogLines.Count > 0) {
          lines.Add(_pendingLogLines.Dequeue());
        }
      }

      if (lines.Count == 0 && droppedLines == 0) return;

      StringBuilder text = new StringBuilder();
      if (droppedLines > 0) {
        text.Append('[').Append(DateTime.Now.ToString("HH:mm:ss")).Append("] ")
          .AppendFormat(Program.localization.GetString("main_form.log_lines_skipped"), droppedLines)
          .AppendLine();
      }
      foreach (string line in lines) text.Append(line);

      LogTextBox.AppendText(text.ToString());
      TrimVisibleLog();
      LogTextBox.ScrollToEnd();
    }

    private void TrimVisibleLog() {
      if (LogTextBox.Text.Length <= MaxVisibleLogChars) return;

      int startIndex = LogTextBox.Text.Length - RetainedLogChars;
      int nextLineIndex = LogTextBox.Text.IndexOf('\n', startIndex);
      if (nextLineIndex >= 0) startIndex = nextLineIndex + 1;
      LogTextBox.Text = LogTextBox.Text.Substring(startIndex);
      LogTextBox.CaretIndex = LogTextBox.Text.Length;
    }

    private void ProcessManager_StatusChanged(object sender, bool isRunning) {
      Dispatcher.BeginInvoke(new Action(() => UpdateStatus(isRunning)));
    }

    private void UpdateStatus(bool isRunning) {
      ToggleButton.Content = Program.localization.GetString(isRunning ? "main_form.disconnect" : "main_form.connect");
      _toggleMenuItem.Text = Program.localization.GetString(isRunning ? "main_form.disconnect" : "main_form.connect");
      _notifyIcon.Text = LimitTrayText(Program.localization.GetString(isRunning ? "main_form.connected" : "main_form.disconnected"));
      Drawing.Icon previousIcon = _notifyIcon.Icon;
      _notifyIcon.Icon = GetTrayIcon(isRunning);
      previousIcon?.Dispose();

      ToggleButton.Background = (Brush)FindResource(isRunning
        ? "DisconnectButtonBrush"
        : "ConnectButtonBrush");
      ToggleButton.BorderBrush = (Brush)FindResource(isRunning
        ? "DisconnectButtonBorderBrush"
        : "ConnectButtonBorderBrush");
    }

    private void ProcessManager_ProxiFyreUnexpectedStopped(object sender, EventArgs e) {
      Dispatcher.BeginInvoke(new Action(() => {
        _notifyIcon.ShowBalloonTip(
          4000,
          Program.localization.GetString("main_form.proxifyre_dependency_title"),
          Program.localization.GetString("main_form.proxifyre_dependency_tray"),
          Forms.ToolTipIcon.Warning
        );
        DependencyWindow dialog = new DependencyWindow { Owner = this };
        dialog.ShowDialog();
      }));
    }

    private void Localization_LanguageChanged(object sender, EventArgs e) {
      Dispatcher.BeginInvoke(new Action(UpdateLocale));
    }

    private void UpdateLocale() {
      Title = Program.localization.GetString("app_name");
      SettingsButton.Content = Program.localization.GetString("main_form.settings");
      UpdateStatus(_processManager.IsRunning);

      if (_notifyIcon.ContextMenu != null) {
        _notifyIcon.ContextMenu.MenuItems[0].Text = Program.localization.GetString("tray_menu.open");
        _notifyIcon.ContextMenu.MenuItems[2].Text = Program.localization.GetString("tray_menu.exit");
      }

      foreach (Button button in new[] { RuButton, EnButton, TrButton }) {
        string brushKey = string.Equals(button.Tag as string, Program.localization.CurrentLanguage,
          StringComparison.OrdinalIgnoreCase) ? "DarkAccentBrush" : "DarkTextBrush";
        button.Foreground = (Brush)FindResource(brushKey);
      }
    }

    private void LanguageButton_Click(object sender, RoutedEventArgs e) {
      Button button = (Button)sender;
      string language = button.Tag as string;
      if (language == null || language == Program.localization.CurrentLanguage) return;
      Program.localization.ChangeLanguage(language);
      _settings.Language = language;
      _settings.Save();
    }

    private Drawing.Icon GetTrayIcon(bool connected) {
      string name = connected ? "bdmanager.tray-on.ico" : "bdmanager.tray-off.ico";
      Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
      return stream == null ? (Drawing.Icon)Drawing.SystemIcons.Application.Clone() : new Drawing.Icon(stream);
    }

    private static string LimitTrayText(string text) {
      if (string.IsNullOrEmpty(text)) return "ByeDPI Manager";
      return text.Length > 63 ? text.Substring(0, 63) : text;
    }

    private void ExitApplication() {
      _allowClose = true;
      Application.Current.Shutdown();
    }

    public void PrepareForShutdown() {
      _allowClose = true;
    }
  }
}
