using System;
using System.Diagnostics;
using System.IO;

namespace bdmanager {
  public class ProcessManager {
    private Process _byeDpiProcess;
    private Process _proxifyreProcess;
    private AppSettings _settings;

    public event EventHandler<string> LogMessage;
    public event EventHandler<bool> StatusChanged;

    public bool IsRunning => _byeDpiProcess?.HasExited == false && _proxifyreProcess?.HasExited == false;

    public void Start() {
      _settings = Program.settings;

      try {
        if (!File.Exists(_settings.ByeDpiPath)) {
          RaiseLogMessage($"Файл ByeDPI не найден: {_settings.ByeDpiPath}");
          return;
        }

        if (!File.Exists(_settings.ProxiFyrePath)) {
          RaiseLogMessage($"Файл ProxiFyre не найден: {_settings.ProxiFyrePath}");
          return;
        }

        if (!_settings.UpdateProxiFyreConfig()) {
          return;
        }

        try {
          _byeDpiProcess = new Process {
            StartInfo = new ProcessStartInfo {
              FileName = _settings.ByeDpiPath,
              WorkingDirectory = Path.GetDirectoryName(_settings.ByeDpiPath),
              Arguments = _settings.GetByeDpiArguments(),
              UseShellExecute = false,
              CreateNoWindow = true,
              RedirectStandardOutput = true,
              RedirectStandardError = true
            },
            EnableRaisingEvents = true
          };

          _byeDpiProcess.OutputDataReceived += ByeDpiOutputHandler;
          _byeDpiProcess.ErrorDataReceived += ByeDpiOutputHandler;
          _byeDpiProcess.Exited += ProcessStopHandler;

          _byeDpiProcess.Start();
          _byeDpiProcess.BeginOutputReadLine();
          _byeDpiProcess.BeginErrorReadLine();

          RaiseLogMessage($"ByeDPI запущен: {_settings.GetByeDpiArguments()}");
        }
        catch (Exception byeDpiEx) {
          RaiseLogMessage($"Ошибка при запуске ByeDPI: {byeDpiEx.Message}");
          Stop();
          return;
        }

        try {
          _proxifyreProcess = new Process {
            StartInfo = new ProcessStartInfo {
              FileName = _settings.ProxiFyrePath,
              WorkingDirectory = Path.GetDirectoryName(_settings.ProxiFyrePath),
              UseShellExecute = false,
              CreateNoWindow = true,
              RedirectStandardOutput = true,
              RedirectStandardError = true
            },
            EnableRaisingEvents = true
          };

          _proxifyreProcess.OutputDataReceived += ProxifyreOutputHandler;
          _proxifyreProcess.ErrorDataReceived += ProxifyreOutputHandler;
          _proxifyreProcess.Exited += ProcessStopHandler;

          _proxifyreProcess.Start();
          _proxifyreProcess.BeginOutputReadLine();
          _proxifyreProcess.BeginErrorReadLine();

          RaiseLogMessage("ProxiFyre запущен");
        }
        catch (Exception proxiFyreEx) {
          RaiseLogMessage($"Ошибка при запуске ProxiFyre: {proxiFyreEx.Message}");
          Stop();
          return;
        }

        RaiseStatusChanged(true);
      }
      catch (Exception ex) {
        RaiseLogMessage($"Общая ошибка при запуске: {ex.Message}");
        Stop();
      }
    }

    public void Stop() {
      try {
        if (_byeDpiProcess?.HasExited == false) {
          try {
            _byeDpiProcess.Kill();
            _byeDpiProcess = null;

            RaiseLogMessage("ByeDPI процесс остановлен");
          }
          catch (Exception ex) {
            RaiseLogMessage($"Ошибка при остановке ByeDPI: {ex.Message}");
          }
        }

        if (_proxifyreProcess?.HasExited == false) {
          try {
            _proxifyreProcess.Kill();
            _proxifyreProcess = null;

            RaiseLogMessage("ProxiFyre процесс остановлен");
          }
          catch (Exception ex) {
            RaiseLogMessage($"Ошибка при остановке ProxiFyre: {ex.Message}");
          }
        }

        RaiseStatusChanged(false);
      }
      catch (Exception ex) {
        RaiseLogMessage($"Общая ошибка при остановке процессов: {ex.Message}");
      }
    }

    private void ByeDpiOutputHandler(object sender, DataReceivedEventArgs e) {
      if (!string.IsNullOrEmpty(e.Data)) {
        RaiseLogMessage($"ByeDPI: {e.Data}");
      }
    }

    private void ProxifyreOutputHandler(object sender, DataReceivedEventArgs e) {
      if (!string.IsNullOrEmpty(e.Data)) {
        RaiseLogMessage($"ProxiFyre: {e.Data}");
      }
    }

    private void ProcessStopHandler(object sender, EventArgs e) {
      Stop();
    }

    private void RaiseLogMessage(string message) {
      LogMessage?.Invoke(this, message);
    }

    private void RaiseStatusChanged(bool isRunning) {
      StatusChanged?.Invoke(this, isRunning);
    }

    public void CleanupOnStartup() {
      try {
        bool processesFound = false;

        var byeDpiProcesses = Process.GetProcessesByName("ciadpi");
        if (byeDpiProcesses.Length > 0) {
          processesFound = true;
          foreach (var process in byeDpiProcesses) {
            try {
              process.Kill();
              RaiseLogMessage("Остановлен запущенный процесс ByeDPI");
            }
            catch (Exception ex) {
              RaiseLogMessage($"Ошибка при остановке процесса ByeDPI: {ex.Message}");
            }
          }
        }

        var proxiFyreProcesses = Process.GetProcessesByName("ProxiFyre");
        if (proxiFyreProcesses.Length > 0) {
          processesFound = true;
          foreach (var process in proxiFyreProcesses) {
            try {
              process.Kill();
              RaiseLogMessage("Остановлен запущенный процесс ProxiFyre");
            }
            catch (Exception ex) {
              RaiseLogMessage($"Ошибка при остановке процесса ProxiFyre: {ex.Message}");
            }
          }
        }

        if (processesFound) {
          RaiseStatusChanged(false);
        }
      }
      catch (Exception ex) {
        RaiseLogMessage($"Ошибка при очистке процессов при запуске: {ex.Message}");
      }
    }
  }
}
