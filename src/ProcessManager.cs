using System;
using System.Diagnostics;
using System.IO;

namespace bdmanager {
  public class ProcessManager {
    private Process _byeDpiProcess;
    private Process _proxifyreProcess;
    private AppSettings _settings;

    public bool IsRunning => _byeDpiProcess?.HasExited == false && (_settings.DisableProxiFyre || _proxifyreProcess?.HasExited == false);
    public event EventHandler<bool> StatusChanged;

    public ProcessManager() {
      _settings = Program.settings;
    }

    public void StartByeDpi(string arguments = null) {
      try {
        if (!File.Exists(_settings.ByeDpiPath)) {
          Program.logger.Log(string.Format(
            Program.localization.GetString("settings_form.byedpi.not_found"),
            _settings.ByeDpiPath
          ));
          return;
        }

        bool useCustomArgs = !string.IsNullOrWhiteSpace(arguments);
        string args = useCustomArgs ? arguments : _settings.GetByeDpiArguments();

        _byeDpiProcess = new Process {
          StartInfo = new ProcessStartInfo {
            FileName = _settings.ByeDpiPath,
            WorkingDirectory = Path.GetDirectoryName(_settings.ByeDpiPath),
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
          },
          EnableRaisingEvents = true
        };

        if (!useCustomArgs) {
          _byeDpiProcess.OutputDataReceived += ByeDpiOutputHandler;
          _byeDpiProcess.ErrorDataReceived += ByeDpiOutputHandler;
          _byeDpiProcess.Exited += ProcessStopHandler;
        }

        _byeDpiProcess.Start();
        _byeDpiProcess.BeginOutputReadLine();
        _byeDpiProcess.BeginErrorReadLine();

        Program.logger.Log(string.Format(
          Program.localization.GetString("process_manager.byedpi.started"),
          args
        ));
      }
      catch (Exception ex) {
        Program.logger.Log(string.Format(
          Program.localization.GetString("process_manager.byedpi.start_error"),
          ex.Message
        ));
        StopByeDpi();
        throw;
      }
    }

    public void StartProxiFyre() {
      if (_settings.DisableProxiFyre) {
        return;
      }

      try {
        if (!File.Exists(_settings.ProxiFyrePath)) {
          Program.logger.Log(string.Format(
            Program.localization.GetString("settings_form.proxifyre.not_found"),
            _settings.ProxiFyrePath
          ));
          return;
        }

        if (!_settings.UpdateProxiFyreConfig()) {
          return;
        }

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

        Program.logger.Log(Program.localization.GetString("process_manager.proxifyre.started"));
      }
      catch (Exception ex) {
        Program.logger.Log(string.Format(
          Program.localization.GetString("process_manager.proxifyre.start_error"),
          ex.Message
        ));
        StopProxiFyre();
        throw;
      }
    }

    public void StopByeDpi() {
      if (_byeDpiProcess?.HasExited == false) {
        try {
          _byeDpiProcess.Kill();
          _byeDpiProcess = null;
          Program.logger.Log(Program.localization.GetString("process_manager.byedpi.stopped"));
        }
        catch (Exception ex) {
          Program.logger.Log(string.Format(
            Program.localization.GetString("process_manager.byedpi.stop_error"),
            ex.Message
          ));
        }
      }
    }

    public void StopProxiFyre() {
      if (_settings.DisableProxiFyre) {
        return;
      }

      if (_proxifyreProcess?.HasExited == false) {
        try {
          _proxifyreProcess.Kill();
          _proxifyreProcess = null;
          Program.logger.Log(Program.localization.GetString("process_manager.proxifyre.stopped"));
        }
        catch (Exception ex) {
          Program.logger.Log(string.Format(
            Program.localization.GetString("process_manager.proxifyre.stop_error"),
            ex.Message
          ));
        }
      }
    }

    public void Start() {
      try {
        StartByeDpi();

        if (_byeDpiProcess?.HasExited != false) {
          return;
        }

        StartProxiFyre();

        if (IsRunning) {
          RaiseStatusChanged(true);
        }
        else {
          Stop();
        }
      }
      catch (Exception ex) {
        Program.logger.Log(string.Format(
          Program.localization.GetString("settings_form.byedpi.start_error"),
          ex.Message
        ));
        Stop();
      }
    }

    public void Stop() {
      try {
        StopByeDpi();
        StopProxiFyre();
        RaiseStatusChanged(false);
      }
      catch (Exception ex) {
        Program.logger.Log($"{ex.Message}");
      }
    }

    private void ByeDpiOutputHandler(object sender, DataReceivedEventArgs e) {
      if (!string.IsNullOrEmpty(e.Data)) {
        Program.logger.Log(string.Format(
          Program.localization.GetString("process_manager.byedpi.output"),
          e.Data
        ));
      }
    }

    private void ProxifyreOutputHandler(object sender, DataReceivedEventArgs e) {
      if (!string.IsNullOrEmpty(e.Data)) {
        Program.logger.Log(string.Format(
          Program.localization.GetString("process_manager.proxifyre.output"),
          e.Data
        ));
      }
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
              Program.logger.Log(Program.localization.GetString("process_manager.cleanup.byedpi_stopped"));
            }
            catch (Exception ex) {
              Program.logger.Log(string.Format(
                Program.localization.GetString("process_manager.cleanup.byedpi_error"),
                ex.Message
              ));
            }
          }
        }

        var proxiFyreProcesses = Process.GetProcessesByName("ProxiFyre");
        if (proxiFyreProcesses.Length > 0) {
          processesFound = true;
          foreach (var process in proxiFyreProcesses) {
            try {
              process.Kill();
              Program.logger.Log(Program.localization.GetString("process_manager.cleanup.proxifyre_stopped"));
            }
            catch (Exception ex) {
              Program.logger.Log(string.Format(
                Program.localization.GetString("process_manager.cleanup.proxifyre_error"),
                ex.Message
              ));
            }
          }
        }

        if (processesFound) {
          RaiseStatusChanged(false);
        }
      }
      catch (Exception ex) {
        Program.logger.Log(string.Format(
          Program.localization.GetString("process_manager.cleanup.error"),
          ex.Message
        ));
      }
    }
    private void ProcessStopHandler(object sender, EventArgs e) {
      Stop();
    }

    private void RaiseStatusChanged(bool status) {
      StatusChanged?.Invoke(this, status);
    }
  }
}
