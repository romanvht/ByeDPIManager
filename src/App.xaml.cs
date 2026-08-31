using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using bdmanager.Views;

namespace bdmanager {
  public partial class App : Application {
    private const int ASFW_ANY = -1;

    private Mutex _mutex;
    private EventWaitHandle _showMainWindowEvent;
    private Thread _showMainWindowThread;
    private MainWindow _mainWindow;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool AllowSetForegroundWindow(int processId);

    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);

      DispatcherUnhandledException += OnDispatcherUnhandledException;
      AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

      try {
        Program.appName = Assembly.GetExecutingAssembly().GetName().Name;
        Program.isAutorun = e.Args.Any(arg => string.Equals(arg, "--autorun", StringComparison.OrdinalIgnoreCase));
        Program.logger = new Logger();
        Program.settings = AppSettings.Load();
        Program.localization = new Localization();

        if (IsAlreadyRunning()) {
          SignalRunningInstance();
          Shutdown();
          return;
        }

        Program.processManager = new ProcessManager();
        Program.processManager.RestoreSystemProxyOnStartup();
        Program.autorunManager = new AutorunManager();

        _mainWindow = new MainWindow();
        MainWindow = _mainWindow;
        StartShowMainWindowListener();
        _mainWindow.Show();
      }
      catch (Exception ex) {
        string message = Program.localization == null
          ? ex.Message
          : string.Format(Program.localization.GetString("program.error"), ex.Message);
        MessageBox.Show(message, Program.appName ?? "ByeDPI Manager", MessageBoxButton.OK, MessageBoxImage.Error);
        Shutdown(-1);
      }
    }

    private bool IsAlreadyRunning() {
      _mutex = new Mutex(true, Program.appName, out bool createdNew);
      return !createdNew;
    }

    private string GetShowMainWindowEventName() {
      return Program.appName + ".ShowMainWindow";
    }

    private void SignalRunningInstance() {
      try {
        AllowSetForegroundWindow(ASFW_ANY);
        using (EventWaitHandle showEvent = EventWaitHandle.OpenExisting(GetShowMainWindowEventName())) {
          showEvent.Set();
        }
      }
      catch (WaitHandleCannotBeOpenedException) {
      }
    }

    private void StartShowMainWindowListener() {
      _showMainWindowEvent = new EventWaitHandle(false, EventResetMode.AutoReset, GetShowMainWindowEventName());
      _showMainWindowThread = new Thread(() => {
        while (true) {
          try {
            _showMainWindowEvent.WaitOne();
          }
          catch (ObjectDisposedException) {
            return;
          }

          Dispatcher.BeginInvoke(new Action(() => _mainWindow?.ShowMainWindow()));
        }
      }) {
        IsBackground = true,
        Name = "ShowMainWindowListener"
      };
      _showMainWindowThread.Start();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
      Program.logger?.Log(e.Exception.ToString());
      Program.ShutdownProcesses();
      e.Handled = true;
      MessageBox.Show(
        string.Format(Program.localization.GetString("program.error"), e.Exception.Message),
        Program.localization.GetString("app_name"),
        MessageBoxButton.OK,
        MessageBoxImage.Error
      );
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
      Program.logger?.Log(e.ExceptionObject?.ToString() ?? "Unhandled exception");
      Program.ShutdownProcesses();
    }

    protected override void OnSessionEnding(SessionEndingCancelEventArgs e) {
      _mainWindow?.PrepareForShutdown();
      base.OnSessionEnding(e);
    }

    protected override void OnExit(ExitEventArgs e) {
      Program.ShutdownProcesses();

      _showMainWindowEvent?.Dispose();
      _showMainWindowEvent = null;

      if (_mutex != null) {
        try {
          _mutex.ReleaseMutex();
        }
        catch (ApplicationException) {
        }
        _mutex.Dispose();
        _mutex = null;
      }

      base.OnExit(e);
    }
  }
}
