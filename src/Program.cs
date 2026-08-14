using System;

namespace bdmanager {
  public static class Program {
    public static string appName;
    public static bool isAutorun;

    public static AppSettings settings;
    public static ProcessManager processManager;
    public static AutorunManager autorunManager;
    public static Localization localization;
    public static Logger logger;

    public static void ShutdownProcesses() {
      if (processManager != null && processManager.IsRunning) {
        processManager.Stop();
      }
    }
  }
}
