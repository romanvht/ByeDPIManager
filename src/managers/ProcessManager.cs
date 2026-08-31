using System;
using System.Diagnostics;
using System.IO;

namespace bdmanager {
  public class ProcessManager {
    private Process _byeDpiProcess;
    private Process _proxifyreProcess;
    private readonly AppSettings _settings;
    private readonly SystemProxyManager _systemProxyManager;
    private bool _isStopping;
    private bool _suppressByeDpiOutput;
    private bool _suppressProxiFyreOutput;

    public string CurrentProxyIp { get; private set; } = "127.0.0.1";
    public int CurrentProxyPort { get; private set; } = 1080;

    public bool IsRunning {
      get {
        if (_byeDpiProcess?.HasExited != false) {
          return false;
        }

        switch (_settings.RoutingMode) {
          case RoutingMode.Disabled:
            return true;
          case RoutingMode.SystemProxy:
            return _systemProxyManager.IsActive;
          default:
            return _proxifyreProcess?.HasExited == false;
        }
      }
    }
    public event EventHandler<bool> StatusChanged;
    public event EventHandler ProxiFyreUnexpectedStopped;

    public ProcessManager() {
      _settings = Program.settings;
      _systemProxyManager = new SystemProxyManager();
    }

    public bool StartByeDpi(string arguments = null, bool logStatus = true) {
      try {
        string byeDpiPath = _settings.GetByeDpiExecutablePath();

        if (!File.Exists(byeDpiPath)) {
          Program.logger.Log(string.Format(
            Program.localization.GetString("settings_form.byedpi.not_found"),
            byeDpiPath
          ));
          return false;
        }

        bool useCustomArgs = !string.IsNullOrWhiteSpace(arguments);
        string strategy = useCustomArgs ? arguments : _settings.ByeDpiArguments;
        string args = _settings.GetByeDpiArguments(strategy);
        CurrentProxyIp = _settings.GetProxyIp(args);
        CurrentProxyPort = _settings.GetProxyPort(args);

        _byeDpiProcess = new Process {
          StartInfo = new ProcessStartInfo {
            FileName = byeDpiPath,
            WorkingDirectory = Path.GetDirectoryName(byeDpiPath),
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
          },
          EnableRaisingEvents = true
        };
        _suppressByeDpiOutput = false;

        if (!useCustomArgs) {
          _byeDpiProcess.OutputDataReceived += ByeDpiOutputHandler;
          _byeDpiProcess.ErrorDataReceived += ByeDpiOutputHandler;
          _byeDpiProcess.Exited += ProcessStopHandler;
        }

        _byeDpiProcess.Start();
        _byeDpiProcess.BeginOutputReadLine();
        _byeDpiProcess.BeginErrorReadLine();

        if (logStatus) {
          Program.logger.Log(string.Format(
            Program.localization.GetString("process_manager.byedpi.started"),
            args
          ));
        }

        return true;
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

    public bool StartProxiFyre() {
      try {
        string proxiFyrePath = _settings.GetProxiFyreExecutablePath();

        if (!File.Exists(proxiFyrePath)) {
          Program.logger.Log(string.Format(
            Program.localization.GetString("settings_form.proxifyre.not_found"),
            proxiFyrePath
          ));
          return false;
        }

        if (!ProxiFyreConfig.UpdateConfig(_settings, CurrentProxyIp, CurrentProxyPort)) {
          return false;
        }

        _proxifyreProcess = new Process {
          StartInfo = new ProcessStartInfo {
            FileName = proxiFyrePath,
            WorkingDirectory = Path.GetDirectoryName(proxiFyrePath),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
          },
          EnableRaisingEvents = true
        };
        _suppressProxiFyreOutput = false;

        _proxifyreProcess.OutputDataReceived += ProxifyreOutputHandler;
        _proxifyreProcess.ErrorDataReceived += ProxifyreOutputHandler;
        _proxifyreProcess.Exited += ProcessStopHandler;

        _proxifyreProcess.Start();
        _proxifyreProcess.BeginOutputReadLine();
        _proxifyreProcess.BeginErrorReadLine();

        Program.logger.Log(Program.localization.GetString("process_manager.proxifyre.started"));
        return true;
      }
      catch (Exception ex) {
        Program.logger.Log(string.Format(
          Program.localization.GetString("process_manager.proxifyre.start_error"),
          ex.Message
        ));
        RaiseProxiFyreUnexpectedStopped();
        StopProxiFyre();
        throw;
      }
    }

    public void StopByeDpi(bool logStatus = true) {
      _suppressByeDpiOutput = true;
      if (_byeDpiProcess?.HasExited == false) {
        try {
          _byeDpiProcess.Exited -= ProcessStopHandler;
          _byeDpiProcess.OutputDataReceived -= ByeDpiOutputHandler;
          _byeDpiProcess.ErrorDataReceived -= ByeDpiOutputHandler;
          CancelAsyncReads(_byeDpiProcess);
          _byeDpiProcess.Kill();
          _byeDpiProcess = null;
          if (logStatus) {
            Program.logger.Log(Program.localization.GetString("process_manager.byedpi.stopped"));
          }
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
      _suppressProxiFyreOutput = true;
      if (_proxifyreProcess?.HasExited == false) {
        try {
          _proxifyreProcess.Exited -= ProcessStopHandler;
          _proxifyreProcess.OutputDataReceived -= ProxifyreOutputHandler;
          _proxifyreProcess.ErrorDataReceived -= ProxifyreOutputHandler;
          CancelAsyncReads(_proxifyreProcess);
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
        if (!StartByeDpi() || _byeDpiProcess?.HasExited != false) {
          return;
        }

        switch (_settings.RoutingMode) {
          case RoutingMode.Disabled:
            break;
          case RoutingMode.SystemProxy:
            _systemProxyManager.Enable(CurrentProxyIp, CurrentProxyPort);
            break;
          default:
            StartProxiFyre();
            break;
        }

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
        _isStopping = true;
        _systemProxyManager.Disable();
        StopProxiFyre();
        StopByeDpi();
        RaiseStatusChanged(false);
      }
      catch (Exception ex) {
        Program.logger.Log($"{ex.Message}");
      }
      finally {
        _isStopping = false;
      }
    }

    private void ByeDpiOutputHandler(object sender, DataReceivedEventArgs e) {
      if (!_suppressByeDpiOutput && sender == _byeDpiProcess && !string.IsNullOrEmpty(e.Data)) {
        Program.logger.Log(string.Format(
          Program.localization.GetString("process_manager.byedpi.output"),
          e.Data
        ));
      }
    }

    private void ProxifyreOutputHandler(object sender, DataReceivedEventArgs e) {
      if (!_suppressProxiFyreOutput && sender == _proxifyreProcess && !string.IsNullOrEmpty(e.Data)) {
        Program.logger.Log(string.Format(
          Program.localization.GetString("process_manager.proxifyre.output"),
          e.Data
        ));
      }
    }

    private static void CancelAsyncReads(Process process) {
      try {
        process.CancelOutputRead();
      }
      catch (InvalidOperationException) {
      }

      try {
        process.CancelErrorRead();
      }
      catch (InvalidOperationException) {
      }
    }

    public void RestoreSystemProxyOnStartup() {
      _systemProxyManager.RestoreIfNeeded();
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
      if (sender == _proxifyreProcess && !_isStopping && _settings.RoutingMode == RoutingMode.ProxiFyre) {
        Program.logger.Log(Program.localization.GetString("process_manager.proxifyre.unexpected_stop"));
        RaiseProxiFyreUnexpectedStopped();
      }

      Stop();
    }

    private void RaiseStatusChanged(bool status) {
      StatusChanged?.Invoke(this, status);
    }

    private void RaiseProxiFyreUnexpectedStopped() {
      ProxiFyreUnexpectedStopped?.Invoke(this, EventArgs.Empty);
    }
  }
}
