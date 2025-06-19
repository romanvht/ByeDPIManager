using SocksSharp;
using SocksSharp.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bdmanager {
  public class ProxyTestManager {
    public static readonly string PROXY_TEST_FOLDER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "proxytest");
    public static readonly string PROXY_TEST_CMDS = Path.Combine(PROXY_TEST_FOLDER, "cmds.txt");
    public static readonly string PROXY_TEST_SITES = Path.Combine(PROXY_TEST_FOLDER, "sites.txt");
    public static readonly string PROXY_TEST_LATEST_LOG = Path.Combine(PROXY_TEST_FOLDER, "test.log");

    private readonly object _logLock = new object();
    public bool IsTesting { get; private set; }

    public Button ProxyTestStartButton { get; set; }
    public RichTextBox ProxyLogsRichBox { get; set; }
    public Label ProxyTestProgressLabel { get; set; }

    private AppSettings _settings;
    private ProcessManager _processManager;
    private CancellationTokenSource _cancellationTokenSource;

    public event EventHandler<string> LogAdded;

    public ProxyTestManager() {
      _settings = Program.settings;
      _processManager = Program.processManager;
    }

    public static string GetLatestLogs() {
      try {
        if (!File.Exists(PROXY_TEST_LATEST_LOG))
          return string.Empty;

        return File.ReadAllText(PROXY_TEST_LATEST_LOG);
      }
      catch (Exception) {
        return string.Empty;
      }
    }

    private static void ClearLatestLogs() {
      try {
        if (File.Exists(PROXY_TEST_LATEST_LOG))
          File.Delete(PROXY_TEST_LATEST_LOG);
      }
      catch (Exception) { }
    }

    private string[] GetDomains() {
      try {
        if (!File.Exists(PROXY_TEST_SITES)) {
          AppendLogLine(Program.localization.GetString("proxy_test.sites_file_not_found"));
          return Array.Empty<string>();
        }

        return File.ReadAllLines(PROXY_TEST_SITES);
      }
      catch (Exception) {
        AppendLogLine(Program.localization.GetString("proxy_test.sites_file_read_error"));
        return Array.Empty<string>();
      }
    }

    private Task<string[]> GetDomainsAsync() => Task.Run(() => GetDomains());

    private string[] GetCommands() {
      try {
        if (!File.Exists(PROXY_TEST_CMDS)) {
          AppendLogLine(Program.localization.GetString("proxy_test.cmds_file_not_found"));
          return Array.Empty<string>();
        }

        return File.ReadAllLines(PROXY_TEST_CMDS);
      }
      catch (Exception) {
        AppendLogLine(Program.localization.GetString("proxy_test.cmds_file_read_error"));
        return Array.Empty<string>();
      }
    }

    private Task<string[]> GetCommandsAsync() => Task.Run(() => GetCommands());

    private static Uri GetValidUrl(string domain) {
      domain = domain.Trim();
      if (domain.StartsWith("http://") || domain.StartsWith("https://"))
        return new Uri(domain);

      return new Uri($"https://{domain}");
    }

    public async Task StartTesting() {
      try {
        if (IsTesting) {
          MessageBox.Show(
            Program.localization.GetString("proxy_test.already_running"),
            Program.localization.GetString("settings_form.title"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
          );
          return;
        }

        IsTesting = true;

        Action updateUi = () => {
          try {
            ProxyTestStartButton.Text = "Стоп";
            ProxyLogsRichBox.Clear();

            if (ProxyTestProgressLabel != null) {
              ProxyTestProgressLabel.Text = "0/0";
              ProxyTestProgressLabel.Visible = true;
            }
          } catch { }
        };

        if (ProxyTestStartButton.InvokeRequired) {
          try {
            ProxyTestStartButton.BeginInvoke(updateUi);
          } catch { }
        } else {
          updateUi();
        }

        ClearLatestLogs();
        _cancellationTokenSource = new CancellationTokenSource();

        IEnumerable<string> commands = await GetCommandsAsync();
        IEnumerable<string> domains = await GetDomainsAsync();

        if (!commands.Any() || !domains.Any()) {
          StopTesting();
          return;
        }

        int totalTests = commands.Count();
        Action updateProgress = () => {
          try {
            if (ProxyTestProgressLabel != null) {
              ProxyTestProgressLabel.Text = $"0/{totalTests}";
            }
          } catch { }
        };

        if (ProxyTestProgressLabel != null && ProxyTestProgressLabel.InvokeRequired) {
          try {
            ProxyTestProgressLabel.BeginInvoke(updateProgress);
          } catch { }
        } else {
          updateProgress();
        }

        var commandsResults = await CheckDomainsAccessAsync(commands, domains, _cancellationTokenSource.Token);

        if (!IsTesting) return;

        var sortedResults = commandsResults.OrderByDescending(key => key.Value).ToArray();

        AppendLogLine(Program.localization.GetString("proxy_test.commands_success_over_50"));
        AppendLogLine(string.Empty);

        for (int i = 0; i < sortedResults.Length; i++) {
          var result = sortedResults[i];
          int orderNumber = i + 1;
          AppendLogLine($"{result.Key}");
          AppendLogLine($"{result.Value}%");
          AppendLogLine(string.Empty);
        }

        if (sortedResults.Length == 0) {
          AppendLogLine(Program.localization.GetString("proxy_test.no_commands_over_50"));
        }
      }
      catch (OperationCanceledException) {
        AppendLogLine(Program.localization.GetString("proxy_test.stopped"));
      }
      catch (Exception ex) {
        AppendLogLine(string.Format(
          Program.localization.GetString("proxy_test.error_occurred"),
          ex.Message
        ));
      }
      finally {
        StopTesting();
      }
    }

    public void StopTesting() {
      try {
        if (!IsTesting) return;

        try {
          _cancellationTokenSource?.Cancel();
        }
        catch (Exception ex) {
          AppendLogLine(string.Format(
            Program.localization.GetString("settings_form.proxy_test.cancel_error"),
            ex.Message
          ));
        }

        try {
          _processManager.StopByeDpi();
        }
        catch (Exception ex) {
          AppendLogLine(string.Format(
            Program.localization.GetString("settings_form.byedpi.stop_error"),
            ex.Message
          ));
        }
      }
      catch (Exception ex) {
        AppendLogLine(string.Format(
          Program.localization.GetString("settings_form.proxy_test.stop_error"),
          ex.Message
        ));
      }
      finally {
        IsTesting = false;

        Action updateButtons = () => {
          try {
            if (ProxyTestStartButton != null && !ProxyTestStartButton.IsDisposed) {
              ProxyTestStartButton.Text = Program.localization.GetString("settings_form.proxy_test.start");
            }

            if (ProxyTestProgressLabel != null && !ProxyTestProgressLabel.IsDisposed) {
              ProxyTestProgressLabel.Visible = false;
            }
          }
          catch { }
        };

        if (ProxyTestStartButton != null && !ProxyTestStartButton.IsDisposed) {
          if (ProxyTestStartButton.InvokeRequired) {
            try {
              ProxyTestStartButton.BeginInvoke(updateButtons);
            }
            catch { }
          }
          else {
            updateButtons();
          }
        }
      }
    }

    private async Task<List<KeyValuePair<string, int>>> CheckDomainsAccessAsync(
      IEnumerable<string> commands,
      IEnumerable<string> domains,
      CancellationToken cancellationToken)
    {
      var commandsResults = new List<KeyValuePair<string, int>>();
      int requestsCount = _settings.ProxyTestRequestsCount;
      bool fullLog = _settings.ProxyTestFullLog;
      int totalTests = commands.Count();
      int completedTests = 0;

      var domainsList = domains.ToList();

      foreach (string command in commands) {
        cancellationToken.ThrowIfCancellationRequested();

        completedTests++;

        Action updateProgress = () => {
          try {
            if (ProxyTestProgressLabel != null) {
              ProxyTestProgressLabel.Text = $"{completedTests}/{totalTests}";
            }
          } catch { }
        };

        if (ProxyTestProgressLabel != null && ProxyTestProgressLabel.InvokeRequired) {
          try {
            ProxyTestProgressLabel.BeginInvoke(updateProgress);
          } catch { }
        } else {
          updateProgress();
        }

        var args = AppSettings.ShellSplit(command);
        var filtered = AppSettings.FilterLinuxOnlyArgs(args);
        var f_command = string.Join(" ", filtered);

        _processManager.StartByeDpi(f_command);
        AppendLogLine(f_command);

        try {
          using (var semaphore = new SemaphoreSlim(20))
          {
            int totalSuccess = 0;
            int totalProcessed = 0;
            int totalDomains = domainsList.Count;

            var allTasks = new List<Task>();
            var domainResults = new List<Tuple<string, int>>();

            foreach (string domain in domainsList) {
              await semaphore.WaitAsync(cancellationToken);

              allTasks.Add(Task.Run(async () => {
                try {
                  cancellationToken.ThrowIfCancellationRequested();
                  string trimmedDomain = domain.Trim();
                  int successCount = await CheckDomainAccessAsync(trimmedDomain, requestsCount, cancellationToken);

                  if (fullLog) {
                    AppendLogLine($"{trimmedDomain} - {successCount}/{requestsCount}");
                  }

                  lock (domainResults) {
                    domainResults.Add(Tuple.Create(trimmedDomain, successCount));
                    totalSuccess += successCount;
                    totalProcessed++;
                  }
                }
                catch (OperationCanceledException) { throw; }
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

            await Task.WhenAll(allTasks);

            int totalRequests = requestsCount * totalDomains;
            int successPct = totalRequests == 0 ? 0 : totalSuccess * 100 / totalRequests;

            AppendLogLine($"{totalSuccess}/{totalRequests} ({successPct}%)");

            if (successPct > 50) {
              commandsResults.Add(new KeyValuePair<string, int>(command, successPct));
            }

            AppendLogLine(string.Empty);
          }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) {
          AppendLogLine(string.Format(
            Program.localization.GetString("proxy_test.command_test_error"),
            command,
            ex.Message
          ));
        }

        _processManager.StopByeDpi();

        if (_settings.ProxyTestDelay > 0) {
          await Task.Delay(_settings.ProxyTestDelay * 1000, cancellationToken);
        }
      }

      return commandsResults;
    }

    private async Task<int> CheckDomainAccessAsync(string domain, int requestsCount, CancellationToken cancellationToken) {
      Uri websiteUrl = GetValidUrl(domain);
      int successRequests = 0;

      ProxySettings proxySettings = new ProxySettings() {
        Host = "127.0.0.1",
        Port = 1080
      };

      try {
        using (var proxyClientHandler = new ProxyClientHandler<Socks5>(proxySettings))
        using (var httpClient = new HttpClient(proxyClientHandler) {
          Timeout = TimeSpan.FromSeconds(3)
        })
        {
          for (int i = 0; i < requestsCount && !cancellationToken.IsCancellationRequested; i++) {
            try {
              using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
              using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
              {
                var response = await httpClient.GetAsync(websiteUrl, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token);
                successRequests++;
              }
            }
            catch (TaskCanceledException) { }
            catch (HttpRequestException) { }
            catch (OperationCanceledException) {
              if (cancellationToken.IsCancellationRequested) throw;
            }
            catch (Exception) { }
          }
        }
      }
      catch (OperationCanceledException) {
        if (cancellationToken.IsCancellationRequested) throw;
      }
      catch (Exception) { }

      return successRequests;
    }

    private void AppendLogLine(string text) {
      try {
        try {
          Directory.CreateDirectory(Path.GetDirectoryName(PROXY_TEST_LATEST_LOG));
          File.AppendAllText(PROXY_TEST_LATEST_LOG, $"{text}{Environment.NewLine}");
        }
        catch (Exception) { }

        LogAdded?.Invoke(this, text);

        if (ProxyLogsRichBox == null || ProxyLogsRichBox.IsDisposed) return;

        Action updateUi = () => {
          try {
            lock (_logLock) {
              ProxyLogsRichBox.AppendText(text);
              ProxyLogsRichBox.AppendText(Environment.NewLine);
              ProxyLogsRichBox.ScrollToCaret();
            }
          } catch (Exception) { }
        };

        if (ProxyLogsRichBox.InvokeRequired) {
          try {
            ProxyLogsRichBox.BeginInvoke(updateUi);
          } catch (Exception) { }
        } else {
          updateUi();
        }
      }
      catch (Exception) { }
    }
  }
}
