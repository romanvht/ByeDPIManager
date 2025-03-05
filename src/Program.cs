using System;
using System.Threading;
using System.Windows.Forms;

namespace bdmanager {
  static class Program {
    private static bool _createdNew = false;
    private static Mutex _mutex = null;
    private static MainForm _mainForm = null;

    public static string AppName = null;
    public static AppSettings settings = null;
    public static ProcessManager processManager = null;
    public static Logger logger = null;

    [STAThread]
    static void Main() {
      AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

      _mutex = new Mutex(true, AppName, out _createdNew);

      if (!_createdNew) {
        MessageBox.Show("Приложение уже запущено.", AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      Application.ApplicationExit += Application_ApplicationExit;
      Application.ThreadException += Application_ThreadException;
      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

      try {
        logger = new Logger();
        settings = AppSettings.Load();
        processManager = new ProcessManager();
        _mainForm = new MainForm();
        Application.Run(_mainForm);
      }
      catch (Exception ex) {
        MessageBox.Show($"Произошла ошибка: {ex.Message}", AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally {
        if (_mutex != null) {
          _mutex.ReleaseMutex();
          _mutex.Dispose();
        }
      }
    }

    private static void Application_ApplicationExit(object sender, EventArgs e) {
      ShutdownProcesses();
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
