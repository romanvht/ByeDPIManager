using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace bdmanager {
  static class Program {
    private const int ASFW_ANY = -1;

    private static Mutex _mutex = null;
    private static EventWaitHandle _showMainWindowEvent = null;
    private static Thread _showMainWindowThread = null;
    private static MainForm _mainForm = null;

    public static string appName = null;
    public static bool isAutorun = false;

    public static AppSettings settings = null;
    public static ProcessManager processManager = null;
    public static AutorunManager autorunManager = null;
    public static ProxyTestManager proxyTestManager = null;
    public static Localization localization = null;
    public static Logger logger = null;

    [STAThread]
    static void Main(string[] args) {
      SetProcessDPIAware();

      appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
      isAutorun = args.Any(arg => arg.ToLower() == "--autorun");

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      Application.ApplicationExit += Application_ApplicationExit;
      Application.ThreadException += Application_ThreadException;
      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

      try {
        logger = new Logger();
        settings = AppSettings.Load();
        localization = new Localization();

        if (IsRunning()) {
          SignalRunningInstance();
          return;
        }

        processManager = new ProcessManager();
        autorunManager = new AutorunManager();
        _mainForm = new MainForm();
        StartShowMainWindowListener();
        Application.Run(_mainForm);
      }
      catch (Exception ex) {
        MessageBox.Show(
          string.Format(localization.GetString("program.error"), ex.Message),
          localization.GetString("app_name"),
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
        );
      }
    }

    [DllImport("user32.dll")]
    static extern bool SetProcessDPIAware();

    [DllImport("user32.dll")]
    static extern bool AllowSetForegroundWindow(int dwProcessId);

    private static bool IsRunning() {
      _mutex = new Mutex(true, appName, out bool createdNew);
      return !createdNew;
    }

    private static string GetShowMainWindowEventName() {
      return appName + ".ShowMainWindow";
    }

    private static void SignalRunningInstance() {
      try {
        AllowSetForegroundWindow(ASFW_ANY);
        using (EventWaitHandle showMainWindowEvent = EventWaitHandle.OpenExisting(GetShowMainWindowEventName())) {
          showMainWindowEvent.Set();
        }
      }
      catch (WaitHandleCannotBeOpenedException) {
      }
    }

    private static void StartShowMainWindowListener() {
      _showMainWindowEvent = new EventWaitHandle(false, EventResetMode.AutoReset, GetShowMainWindowEventName());
      _showMainWindowThread = new Thread(WaitForShowMainWindowSignal) {
        IsBackground = true
      };
      _showMainWindowThread.Start();
    }

    private static void WaitForShowMainWindowSignal() {
      while (true) {
        try {
          _showMainWindowEvent.WaitOne();
        }
        catch (ObjectDisposedException) {
          return;
        }

        MainForm mainForm = _mainForm;
        if (mainForm == null || mainForm.IsDisposed) {
          continue;
        }

        try {
          mainForm.BeginInvoke(new Action(mainForm.ShowMainWindow));
        }
        catch (InvalidOperationException) {
        }
      }
    }

    private static void Application_ApplicationExit(object sender, EventArgs e) {
      ShutdownProcesses();
      if (_showMainWindowEvent != null) {
        _showMainWindowEvent.Dispose();
        _showMainWindowEvent = null;
      }

      if (_mutex != null) {
        _mutex.ReleaseMutex();
        _mutex.Dispose();
      }
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) {
      ShutdownProcesses();
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
      ShutdownProcesses();
    }

    public static void ShutdownProcesses() {
      if (processManager != null && processManager.IsRunning) {
        processManager.Stop();
      }
    }
  }
}
