using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace bdmanager {
  static class Program {
    private static Mutex _mutex = null;

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
          MessageBox.Show(
            localization.GetString("program.already_running"),
            localization.GetString("app_name"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
          );
          return;
        }

        processManager = new ProcessManager();
        autorunManager = new AutorunManager();
        Application.Run(new MainForm());
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

    private static bool IsRunning() {
      _mutex = new Mutex(true, appName, out bool createdNew);
      return !createdNew;
    }

    private static void Application_ApplicationExit(object sender, EventArgs e) {
      ShutdownProcesses();
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
