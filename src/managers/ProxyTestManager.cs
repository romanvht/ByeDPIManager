using SocksSharp;
using SocksSharp.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bdmanager {
  public sealed class ProxyTestManager {
    private const int MaxParallelDomainChecks = 20;
    private const int RequestTimeoutSeconds = 5;
    private const int DomainTimeoutBufferSeconds = 2;

    public static readonly string PROXY_TEST_FOLDER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "proxytest");
    public static readonly string PROXY_TEST_CMDS = Path.Combine(PROXY_TEST_FOLDER, "strategies.txt");
    public static readonly string PROXY_TEST_SITES = Path.Combine(PROXY_TEST_FOLDER, "domains.txt");
    public static readonly string PROXY_TEST_LATEST_LOG = Path.Combine(PROXY_TEST_FOLDER, "proxytest.log");
    public static readonly string PROXY_TEST_RESULTS = Path.Combine(PROXY_TEST_FOLDER, "proxytest.results");

    private readonly AppSettings _settings;
    private readonly ProcessManager _processManager;
    private readonly object _resultLock = new object();
    private readonly List<ProxyTestResult> _results = new List<ProxyTestResult>();
    private CancellationTokenSource _cancellationTokenSource;

    public bool IsTesting { get; private set; }
    public event EventHandler<string> LogAdded;
    public event EventHandler<ProxyTestResult> ResultUpdated;
    public event EventHandler<ProxyTestProgress> ProgressChanged;
    public event EventHandler<bool> TestingStateChanged;
    public event EventHandler<string> ErrorOccurred;

    public ProxyTestManager() {
      _settings = Program.settings;
      _processManager = Program.processManager;
    }

    public static string GetLatestLogs() {
      try {
        return File.Exists(PROXY_TEST_LATEST_LOG)
          ? File.ReadAllText(PROXY_TEST_LATEST_LOG)
          : string.Empty;
      }
      catch {
        return string.Empty;
      }
    }

    public IReadOnlyList<ProxyTestResult> LoadResults() {
      List<ProxyTestResult> loaded = new List<ProxyTestResult>();
      try {
        if (File.Exists(PROXY_TEST_RESULTS)) {
          foreach (string line in File.ReadAllLines(PROXY_TEST_RESULTS, Encoding.UTF8)) {
            int separator = line.LastIndexOf('|');
            if (separator <= 0 || separator >= line.Length - 1) continue;

            string strategy = line.Substring(0, separator);
            string result = line.Substring(separator + 1);
            loaded.Add(ProxyTestResult.FromText(strategy, result));
          }
        }
      }
      catch {
      }

      lock (_resultLock) {
        _results.Clear();
        _results.AddRange(loaded.OrderByDescending(item => item.SuccessRate));
        return _results.ToList();
      }
    }

    public void SaveResults() {
      try {
        Directory.CreateDirectory(PROXY_TEST_FOLDER);
        List<string> lines;
        lock (_resultLock) {
          lines = _results
            .OrderByDescending(item => item.SuccessRate)
            .Select(item => item.Strategy + "|" + item.Result)
            .ToList();
        }
        File.WriteAllLines(PROXY_TEST_RESULTS, lines, Encoding.UTF8);
      }
      catch {
      }
    }

    public async Task StartTesting() {
      if (IsTesting) {
        RaiseError(Program.localization.GetString("proxy_test.already_running"));
        return;
      }

      IsTesting = true;
      TestingStateChanged?.Invoke(this, true);
      ClearLatestLogs();
      lock (_resultLock) _results.Clear();
      _cancellationTokenSource = new CancellationTokenSource();

      try {
        if (!ValidateRequiredExecutables()) return;

        string[] commands = await ReadRequiredLinesAsync(
          PROXY_TEST_CMDS,
          "proxy_test.cmds_file_not_found",
          "proxy_test.cmds_file_read_error"
        );
        string[] domains = await ReadRequiredLinesAsync(
          PROXY_TEST_SITES,
          "proxy_test.sites_file_not_found",
          "proxy_test.sites_file_read_error"
        );

        if (commands.Length == 0 || domains.Length == 0) return;

        ProgressChanged?.Invoke(this, new ProxyTestProgress(0, commands.Length));
        await CheckDomainsAccessAsync(commands, domains, _cancellationTokenSource.Token);

        if (!_cancellationTokenSource.IsCancellationRequested) {
          AppendLogLine(string.Empty);
          AppendLogLine(Program.localization.GetString("proxy_test.completed"));
          SaveResults();
        }
      }
      catch (OperationCanceledException) {
        AppendLogLine(Program.localization.GetString("proxy_test.stopped"));
      }
      catch (Exception ex) {
        string message = string.Format(Program.localization.GetString("proxy_test.error_occurred"), ex.Message);
        AppendLogLine(message);
        RaiseError(message);
      }
      finally {
        try {
          _processManager.StopByeDpi(false);
        }
        catch {
        }
        IsTesting = false;
        TestingStateChanged?.Invoke(this, false);
      }
    }

    public void StopTesting() {
      if (!IsTesting) return;

      try {
        _cancellationTokenSource?.Cancel();
        _processManager.StopByeDpi(false);
      }
      catch (Exception ex) {
        AppendLogLine(string.Format(
          Program.localization.GetString("settings_form.proxy_test.stop_error"),
          ex.Message
        ));
      }
    }

    private static void ClearLatestLogs() {
      try {
        if (File.Exists(PROXY_TEST_LATEST_LOG)) File.Delete(PROXY_TEST_LATEST_LOG);
      }
      catch {
      }
    }

    private async Task<string[]> ReadRequiredLinesAsync(string path, string missingKey, string readErrorKey) {
      return await Task.Run(() => {
        if (!File.Exists(path)) {
          string message = Program.localization.GetString(missingKey);
          RaiseError(message);
          return new string[0];
        }

        try {
          return FormatUtils.ReadLines(path);
        }
        catch {
          string message = Program.localization.GetString(readErrorKey);
          RaiseError(message);
          return new string[0];
        }
      });
    }

    private bool ValidateRequiredExecutables() {
      string byeDpiPath = _settings.GetByeDpiExecutablePath();
      if (!File.Exists(byeDpiPath)) {
        RaiseLocalizedError("settings_form.byedpi.not_found", byeDpiPath);
        return false;
      }

      if (_settings.RoutingMode == RoutingMode.ProxiFyre) {
        string proxiFyrePath = _settings.GetProxiFyreExecutablePath();
        if (!File.Exists(proxiFyrePath)) {
          RaiseLocalizedError("settings_form.proxifyre.not_found", proxiFyrePath);
          return false;
        }
      }

      return true;
    }

    private void RaiseLocalizedError(string key, params object[] args) {
      RaiseError(string.Format(Program.localization.GetString(key), args));
    }

    private void RaiseError(string message) {
      ErrorOccurred?.Invoke(this, message);
    }

    private string ApplyPlaceholders(string command) {
      string sni = string.IsNullOrWhiteSpace(_settings.ProxyTestSni)
        ? "google.com"
        : _settings.ProxyTestSni.Trim();
      return string.IsNullOrEmpty(command) ? command : command.Replace("{sni}", sni);
    }

    private static Uri GetValidUrl(string domain) {
      domain = domain.Trim();
      return domain.StartsWith("http://") || domain.StartsWith("https://")
        ? new Uri(domain)
        : new Uri("https://" + domain);
    }

    private async Task CheckDomainsAccessAsync(
      IReadOnlyList<string> commands,
      IReadOnlyList<string> domains,
      CancellationToken cancellationToken) {
      int completedTests = 0;

      foreach (string command in commands) {
        cancellationToken.ThrowIfCancellationRequested();
        completedTests++;
        ProgressChanged?.Invoke(this, new ProxyTestProgress(completedTests, commands.Count));

        string commandWithSni = ApplyPlaceholders(command);
        string filteredCommand = string.Join(" ", AppSettings.FilterLinuxOnlyArgs(AppSettings.ShellSplit(commandWithSni)));

        if (!_processManager.StartByeDpi(filteredCommand, false)) {
          string path = _settings.GetByeDpiExecutablePath();
          RaiseLocalizedError("settings_form.byedpi.not_found", path);
          throw new FileNotFoundException(
            string.Format(Program.localization.GetString("settings_form.byedpi.not_found"), path),
            path
          );
        }

        AppendLogLine(filteredCommand);

        try {
          int totalSuccess = 0;
          int totalRequests = 0;
          object totalsLock = new object();

          using (SemaphoreSlim semaphore = new SemaphoreSlim(MaxParallelDomainChecks)) {
            List<Task> tasks = new List<Task>();
            foreach (string domain in domains) {
              await semaphore.WaitAsync(cancellationToken);
              tasks.Add(Task.Run(async () => {
                try {
                  string trimmedDomain = domain.Trim();
                  int successCount = await CheckDomainAccess(
                    trimmedDomain,
                    _settings.ProxyTestRequestsCount,
                    cancellationToken
                  );
                  AppendLogLine(trimmedDomain + " - " + successCount + "/" + _settings.ProxyTestRequestsCount);
                  lock (totalsLock) {
                    totalSuccess += successCount;
                    totalRequests += _settings.ProxyTestRequestsCount;
                  }
                }
                catch (OperationCanceledException) {
                  throw;
                }
                catch (Exception ex) {
                  AppendLogLine(string.Format(
                    Program.localization.GetString("proxy_test.domain_check_error"),
                    ex.Message
                  ));
                }
                finally {
                  semaphore.Release();
                }
              }, cancellationToken));
            }

            await Task.WhenAll(tasks);
          }

          AppendLogLine(totalSuccess + "/" + totalRequests);
          AppendLogLine(string.Empty);
          AddResult(filteredCommand, totalSuccess, totalRequests);
        }
        catch (OperationCanceledException) {
          throw;
        }
        catch (Exception ex) {
          AppendLogLine(string.Format(
            Program.localization.GetString("proxy_test.command_test_error"),
            filteredCommand,
            ex.Message
          ));
        }

        _processManager.StopByeDpi(false);
        if (_settings.ProxyTestDelay > 0) {
          await Task.Delay(_settings.ProxyTestDelay * 1000, cancellationToken);
        }
      }
    }

    private void AddResult(string strategy, int success, int total) {
      ProxyTestResult result = new ProxyTestResult(strategy, success, total);
      lock (_resultLock) {
        ProxyTestResult existing = _results.FirstOrDefault(item => item.Strategy == strategy);
        if (existing != null) _results.Remove(existing);
        _results.Add(result);
        _results.Sort((left, right) => right.SuccessRate.CompareTo(left.SuccessRate));
      }
      ResultUpdated?.Invoke(this, result);
    }

    private async Task<int> CheckDomainAccess(string domain, int requestsCount, CancellationToken token) {
      int timeoutSeconds = RequestTimeoutSeconds * Math.Max(1, requestsCount) + DomainTimeoutBufferSeconds;
      Task<int> checkTask = CheckDomain(domain, requestsCount, token);
      Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), token);
      Task completed = await Task.WhenAny(checkTask, timeoutTask);
      if (completed == checkTask) return await checkTask;
      token.ThrowIfCancellationRequested();
      Task ignoredTask = checkTask.ContinueWith(
        task => { var ignored = task.Exception; },
        TaskContinuationOptions.OnlyOnFaulted
      );
      return 0;
    }

    private async Task<int> CheckDomain(string domain, int requestsCount, CancellationToken token) {
      Uri websiteUrl = GetValidUrl(domain);
      int successRequests = 0;
      ProxySettings proxySettings = new ProxySettings {
        Host = _processManager.CurrentProxyIp,
        Port = _processManager.CurrentProxyPort
      };

      try {
        using (ProxyClientHandler<Socks5> handler = new ProxyClientHandler<Socks5>(proxySettings))
        using (HttpClient client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds) }) {
          client.DefaultRequestHeaders.ConnectionClose = true;

          for (int index = 0; index < requestsCount && !token.IsCancellationRequested; index++) {
            try {
              using (CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(RequestTimeoutSeconds)))
              using (CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, token))
              using (HttpResponseMessage response = await client.GetAsync(websiteUrl, HttpCompletionOption.ResponseHeadersRead, linked.Token)) {
                long? declaredLength = response.Content.Headers.ContentLength;
                long actualLength = 0;

                try {
                  using (Stream stream = await response.Content.ReadAsStreamAsync()) {
                    byte[] buffer = new byte[8192];
                    long limit = declaredLength ?? 1024 * 1024;
                    while (actualLength < limit) {
                      int read = await stream.ReadAsync(
                        buffer,
                        0,
                        (int)Math.Min(limit - actualLength, buffer.Length),
                        linked.Token
                      );
                      if (read == 0) break;
                      actualLength += read;
                    }
                  }
                }
                catch (IOException) {
                }
                catch (OperationCanceledException) {
                  if (token.IsCancellationRequested) throw;
                }

                if (!declaredLength.HasValue || actualLength >= declaredLength.Value) successRequests++;
              }
            }
            catch (TaskCanceledException) {
            }
            catch (HttpRequestException) {
            }
            catch (OperationCanceledException) {
              if (token.IsCancellationRequested) throw;
            }
            catch {
            }
          }
        }
      }
      catch (OperationCanceledException) {
        if (token.IsCancellationRequested) throw;
      }
      catch {
      }

      return successRequests;
    }

    private void AppendLogLine(string text) {
      try {
        Directory.CreateDirectory(PROXY_TEST_FOLDER);
        File.AppendAllText(PROXY_TEST_LATEST_LOG, text + Environment.NewLine);
      }
      catch {
      }
      LogAdded?.Invoke(this, text);
    }
  }
}
