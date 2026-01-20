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
using System.Windows.Forms;
using bdmanager.Views.Tabs;

namespace bdmanager {
  public class ProxyTestManager {
    public static readonly string PROXY_TEST_FOLDER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "proxytest");
    public static readonly string PROXY_TEST_CMDS = Path.Combine(PROXY_TEST_FOLDER, "strategies.txt");
    public static readonly string PROXY_TEST_SITES = Path.Combine(PROXY_TEST_FOLDER, "domains.txt");
    public static readonly string PROXY_TEST_LATEST_LOG = Path.Combine(PROXY_TEST_FOLDER, "proxytest.log");
    public static readonly string PROXY_TEST_RESULTS = Path.Combine(PROXY_TEST_FOLDER, "proxytest.results");

    private readonly object _logLock = new object();
    private readonly object _dataGridLock = new object();
    public bool IsTesting { get; private set; }

    public Button ProxyTestStartButton { get; set; }
    public TextBox ProxyTestLogsBox { get; set; }
    public DataGridView ResultsDataGridView { get; set; }
    public Label ProxyTestProgressLabel { get; set; }

    private AppSettings _settings;
    private ProcessManager _processManager;
    private CancellationTokenSource _cancellationTokenSource;
    private ByeDpiTab _byeDpiTab;

    public event EventHandler<string> LogAdded;

    public ProxyTestManager(ByeDpiTab byeDpiTab) {
      _settings = Program.settings;
      _processManager = Program.processManager;
      _byeDpiTab = byeDpiTab;
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

    public void SaveResults(DataGridView dataGridView) {
      try {
        if (dataGridView == null || dataGridView.IsDisposed) return;

        Directory.CreateDirectory(PROXY_TEST_FOLDER);

        using (StreamWriter writer = new StreamWriter(PROXY_TEST_RESULTS, false, Encoding.UTF8)) {
          foreach (DataGridViewRow row in dataGridView.Rows) {
            if (row.Tag == null || !(row.Tag is string)) continue;

            string strategy = (string)row.Tag;
            string result = row.Cells[1].Value?.ToString() ?? "";
            writer.WriteLine($"{strategy}|{result}");
          }
        }
      }
      catch (Exception) {
      }
    }

    public void LoadResults(DataGridView dataGridView) {
      try {
        if (dataGridView == null || dataGridView.IsDisposed) return;
        if (!File.Exists(PROXY_TEST_RESULTS)) return;

        dataGridView.SuspendLayout();
        dataGridView.Rows.Clear();

        var rowsToAdd = new List<Tuple<string, string, double>>();

        using (StreamReader reader = new StreamReader(PROXY_TEST_RESULTS, Encoding.UTF8)) {
          string line;
          while ((line = reader.ReadLine()) != null) {
            var parts = line.Split('|');
            if (parts.Length == 2) {
              string strategy = parts[0];
              string result = parts[1];

              double percent = 0;
              var resultParts = result.Split('/');
              if (resultParts.Length == 2) {
                int.TryParse(resultParts[0], out int success);
                int.TryParse(resultParts[1], out int total);
                percent = total > 0 ? (double)success / total : 0;
              }

              rowsToAdd.Add(Tuple.Create(strategy, result, percent));
            }
          }
        }

        rowsToAdd = rowsToAdd.OrderByDescending(x => x.Item3).ToList();

        foreach (var item in rowsToAdd) {
          int rowIndex = dataGridView.Rows.Add(item.Item1, item.Item2);
          dataGridView.Rows[rowIndex].Tag = item.Item1;
        }

        dataGridView.ResumeLayout();
      }
      catch (Exception) {
      }
    }

    private string[] GetDomains() {
      try {
        if (!File.Exists(PROXY_TEST_SITES)) {
          MessageBox.Show(
            _byeDpiTab?.FindForm(),
            Program.localization.GetString("proxy_test.sites_file_not_found"),
            Program.localization.GetString("settings_form.title"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
          );
          return Array.Empty<string>();
        }

        return FormatUtils.ReadLines(PROXY_TEST_SITES);
      }
      catch (Exception) {
        MessageBox.Show(
          _byeDpiTab?.FindForm(),
          Program.localization.GetString("proxy_test.sites_file_read_error"),
          Program.localization.GetString("settings_form.title"),
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
        );
        return Array.Empty<string>();
      }
    }

    private Task<string[]> GetDomainsAsync() => Task.Run(() => GetDomains());

    private string[] GetCommands() {
      try {
        if (!File.Exists(PROXY_TEST_CMDS)) {
          MessageBox.Show(
            _byeDpiTab?.FindForm(),
            Program.localization.GetString("proxy_test.cmds_file_not_found"),
            Program.localization.GetString("settings_form.title"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
          );
          return Array.Empty<string>();
        }

        return FormatUtils.ReadLines(PROXY_TEST_CMDS);
      }
      catch (Exception) {
        MessageBox.Show(
          _byeDpiTab?.FindForm(),
          Program.localization.GetString("proxy_test.cmds_file_read_error"),
          Program.localization.GetString("settings_form.title"),
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
        );
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
            _byeDpiTab?.FindForm(),
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
            ProxyTestStartButton.Text = Program.localization.GetString("settings_form.proxy_test.stop");
            ProxyTestLogsBox.Clear();

            if (ResultsDataGridView != null && !ResultsDataGridView.IsDisposed) {
              ResultsDataGridView.Rows.Clear();
            }

            if (ProxyTestProgressLabel != null) {
              ProxyTestProgressLabel.Text = "0/0";
              ProxyTestProgressLabel.Visible = true;
            }
          }
          catch { }
        };

        if (ProxyTestStartButton.InvokeRequired) {
          try {
            ProxyTestStartButton.BeginInvoke(updateUi);
          }
          catch { }
        }
        else {
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
          }
          catch { }
        };

        if (ProxyTestProgressLabel != null && ProxyTestProgressLabel.InvokeRequired) {
          try {
            ProxyTestProgressLabel.BeginInvoke(updateProgress);
          }
          catch { }
        }
        else {
          updateProgress();
        }

        await CheckDomainsAccessAsync(commands, domains, _cancellationTokenSource.Token);

        if (!IsTesting) return;

        AppendLogLine(string.Empty);
        AppendLogLine(Program.localization.GetString("proxy_test.completed"));

        Action saveResults = () => {
          try {
            if (ResultsDataGridView != null && !ResultsDataGridView.IsDisposed) {
              SaveResults(ResultsDataGridView);
            }
          }
          catch { }
        };

        if (ResultsDataGridView != null && !ResultsDataGridView.IsDisposed) {
          if (ResultsDataGridView.InvokeRequired) {
            try {
              ResultsDataGridView.Invoke(saveResults);
            }
            catch { }
          }
          else {
            saveResults();
          }
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

    private async Task CheckDomainsAccessAsync(
      IEnumerable<string> commands,
      IEnumerable<string> domains,
      CancellationToken cancellationToken) {
      int requestsCount = _settings.ProxyTestRequestsCount;
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
          }
          catch { }
        };

        if (ProxyTestProgressLabel != null && ProxyTestProgressLabel.InvokeRequired) {
          try {
            ProxyTestProgressLabel.BeginInvoke(updateProgress);
          }
          catch { }
        }
        else {
          updateProgress();
        }

        var args = AppSettings.ShellSplit(command);
        var filtered = AppSettings.FilterLinuxOnlyArgs(args);
        var f_command = string.Join(" ", filtered);

        _processManager.StartByeDpi(f_command);
        AppendLogLine(f_command);

        try {
          using (var semaphore = new SemaphoreSlim(20)) {
            int totalSuccess = 0;
            int totalRequests = 0;

            var allTasks = new List<Task>();

            foreach (string domain in domainsList) {
              await semaphore.WaitAsync(cancellationToken);

              allTasks.Add(Task.Run(async () => {
                try {
                  cancellationToken.ThrowIfCancellationRequested();
                  string trimmedDomain = domain.Trim();
                  int successCount = await CheckDomainAccessAsync(trimmedDomain, requestsCount, cancellationToken);

                  AppendLogLine($"{trimmedDomain} - {successCount}/{requestsCount}");

                  lock (this) {
                    totalSuccess += successCount;
                    totalRequests += requestsCount;
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

            string result = $"{totalSuccess}/{totalRequests}";
            AppendLogLine(result);
            AppendLogLine(string.Empty);

            AddResult(command, totalSuccess, totalRequests);
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
    }

    private void AddResult(string command, int totalSuccess, int totalRequests) {
      Action updateDataGrid = () => {
        try {
          if (ResultsDataGridView == null || ResultsDataGridView.IsDisposed) return;

          lock (_dataGridLock) {
            ResultsDataGridView.SuspendLayout();

            DataGridViewRow existingRow = null;
            foreach (DataGridViewRow row in ResultsDataGridView.Rows) {
              if (row.Tag != null && row.Tag is string && (string)row.Tag == command) {
                existingRow = row;
                break;
              }
            }

            string result = $"{totalSuccess}/{totalRequests}";

            if (existingRow != null) {
              existingRow.Cells[1].Value = result;
            }
            else {
              int rowIndex = ResultsDataGridView.Rows.Add(command, result);
              ResultsDataGridView.Rows[rowIndex].Tag = command;
            }

            var rows = new List<Tuple<DataGridViewRow, double>>();
            foreach (DataGridViewRow row in ResultsDataGridView.Rows) {
              string res = row.Cells[1].Value?.ToString() ?? "";
              double percent = 0;
              var parts = res.Split('/');
              if (parts.Length == 2) {
                int.TryParse(parts[0], out int success);
                int.TryParse(parts[1], out int total);
                percent = total > 0 ? (double)success / total : 0;
              }
              rows.Add(Tuple.Create(row, percent));
            }

            var sortedRows = rows.OrderByDescending(x => x.Item2).ToList();

            ResultsDataGridView.Rows.Clear();
            foreach (var item in sortedRows) {
              ResultsDataGridView.Rows.Add(item.Item1);
            }

            ResultsDataGridView.ResumeLayout();
          }
        }
        catch { }
      };

      if (ResultsDataGridView != null && !ResultsDataGridView.IsDisposed) {
        if (ResultsDataGridView.InvokeRequired) {
          try {
            ResultsDataGridView.BeginInvoke(updateDataGrid);
          }
          catch { }
        }
        else {
          updateDataGrid();
        }
      }
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
          Timeout = TimeSpan.FromSeconds(5)
        }) {
          httpClient.DefaultRequestHeaders.ConnectionClose = true;

          for (int i = 0; i < requestsCount && !cancellationToken.IsCancellationRequested; i++) {
            try {
              using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
              using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken)) {
                var response = await httpClient.GetAsync(websiteUrl, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token);

                int responseCode = (int)response.StatusCode;
                long? declaredLength = response.Content.Headers.ContentLength;
                long actualLength = 0;

                try {
                  using (var stream = await response.Content.ReadAsStreamAsync()) {
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    long limit = declaredLength ?? (1024 * 1024);

                    while (actualLength < limit) {
                      long remaining = limit - actualLength;
                      int toRead = (int)Math.Min(remaining, buffer.Length);

                      bytesRead = await stream.ReadAsync(buffer, 0, toRead, linkedCts.Token);
                      if (bytesRead == 0) break;

                      actualLength += bytesRead;
                    }
                  }
                }
                catch (IOException) {
                }
                catch (OperationCanceledException) {
                  if (cancellationToken.IsCancellationRequested) throw;
                }

                if (!declaredLength.HasValue || actualLength >= declaredLength.Value) {
                  successRequests++;
                }
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

        if (ProxyTestLogsBox == null || ProxyTestLogsBox.IsDisposed) return;

        Action updateUi = () => {
          try {
            lock (_logLock) {
              ProxyTestLogsBox.AppendText(text);
              ProxyTestLogsBox.AppendText(Environment.NewLine);
              ProxyTestLogsBox.ScrollToCaret();
            }
          }
          catch (Exception) { }
        };

        if (ProxyTestLogsBox.InvokeRequired) {
          try {
            ProxyTestLogsBox.BeginInvoke(updateUi);
          }
          catch (Exception) { }
        }
        else {
          updateUi();
        }
      }
      catch (Exception) { }
    }
  }
}
