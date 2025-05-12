using SocksSharp;
using SocksSharp.Proxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bdmanager {
  public class ProxyTestManager {
    public const string PROXY_TEST_FOLDER = "./proxytest";

    public const string PROXY_TEST_CMDS = PROXY_TEST_FOLDER + "/cmds.txt";
    public const string PROXY_TEST_SITES = PROXY_TEST_FOLDER + "/sites.txt";
    public const string PROXY_TEST_LATEST_LOG = PROXY_TEST_FOLDER + "/proxytest.log";

    private readonly object _logLock = new object();

    public bool IsTesting { get; private set; }
    
    public Button ProxyTestStartButton { get; set; }
    public RichTextBox ProxyLogsRichBox { get; set; }
    public Label ProxyTestProgressLabel { get; set; }
    
    private AppSettings _settings = Program.settings;
    private Process _byeDpiProcess;
    private CancellationTokenSource _cancellationTokenSource;
    
    public event EventHandler<string> LogAdded;

    public ProxyTestManager() {
      _settings = Program.settings;
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
          AppendLogLine("Файл со списком сайтов не найден. Создайте файл sites.txt в папке proxytest и добавьте в него список доменов для проверки.", FontStyle.Bold);
          return Array.Empty<string>();
        }
        
        return File.ReadAllLines(PROXY_TEST_SITES);
      }
      catch (Exception) {
        AppendLogLine("Ошибка при чтении файла со списком сайтов.", FontStyle.Bold);
        return Array.Empty<string>();
      }
    }

    private Task<string[]> GetDomainsAsync() => Task.Run(() => GetDomains());

    private string[] GetCommands() {
      try {
        if (!File.Exists(PROXY_TEST_CMDS)) {
          AppendLogLine("Файл с командами не найден. Создайте файл cmds.txt в папке proxytest и добавьте в него нужные команды для ByeDPI.", FontStyle.Bold);
          return Array.Empty<string>();
        }
        
        return File.ReadAllLines(PROXY_TEST_CMDS);
      }
      catch (Exception) {
        AppendLogLine("Ошибка при чтении файла с командами.", FontStyle.Bold);
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
          MessageBox.Show("Тестирование уже запущено.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        AppendLogLine("Команды с успехом более 50%, применяйте от большего к меньшему", FontStyle.Bold);
        AppendLogLine(string.Empty);

        for (int i = 0; i < sortedResults.Length; i++) {
          var result = sortedResults[i];
          int orderNumber = i + 1;
          AppendLogLine($"{result.Key}");
          AppendLogLine($"{result.Value}%");
          AppendLogLine(string.Empty);
        }

        if (sortedResults.Length == 0) {
          AppendLogLine("Ни одна команда не дала результат более 50%", FontStyle.Bold);
        }
      }
      catch (OperationCanceledException) {
        AppendLogLine("Тест остановлен.", FontStyle.Bold);
      }
      catch (Exception ex) {
        AppendLogLine($"Тест прокси остановлен. Произошла ошибка: {ex.Message}", FontStyle.Bold);
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
          AppendLogLine($"Ошибка при отмене операций: {ex.Message}");
        }
        
        try {
          StopByeDpi();
        }
        catch (Exception ex) {
          AppendLogLine($"Ошибка при остановке ByeDPI: {ex.Message}");
        }
      }
      catch (Exception ex) {
        AppendLogLine($"Ошибка при остановке тестирования: {ex.Message}");
      }
      finally {
        IsTesting = false;
        
        Action updateButtons = () => {
          try {
            if (ProxyTestStartButton != null && !ProxyTestStartButton.IsDisposed) {
              ProxyTestStartButton.Text = "Старт";
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

    private void StartByeDpi(string arguments) {
      if (!File.Exists(_settings.ByeDpiPath)) {
        MessageBox.Show($"Файл ByeDPI не найден: {_settings.ByeDpiPath}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        StopTesting();
        return;
      }

      try {
        _byeDpiProcess = new Process {
          StartInfo = new ProcessStartInfo {
            FileName = _settings.ByeDpiPath,
            WorkingDirectory = Path.GetDirectoryName(_settings.ByeDpiPath),
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true
          },
          EnableRaisingEvents = true
        };

        _byeDpiProcess.Start();
      }
      catch (Exception byeDpiEx) {
        MessageBox.Show($"Ошибка при запуске ByeDPI: {byeDpiEx.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        StopByeDpi();
      }
    }

    private void StopByeDpi() {
      if (_byeDpiProcess?.HasExited == false) {
        try {
          _byeDpiProcess.Kill();
          _byeDpiProcess = null;
        }
        catch (Exception ex) {
          AppendLogLine($"Ошибка при остановке ByeDPI: {ex.Message}", FontStyle.Bold);
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

        StartByeDpi(f_command);
        AppendLogLine(f_command, FontStyle.Bold);

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
                  AppendLogLine($"Ошибка при проверке домена: {ex.Message}");
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
          AppendLogLine($"Ошибка при тестировании команды {command}: {ex.Message}", FontStyle.Bold);
        }
        
        StopByeDpi();
        
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

    private void AppendLogLine(string text, FontStyle? fontStyle = null) {
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
              if (fontStyle.HasValue) {
                ProxyLogsRichBox.SelectionFont = new Font(ProxyLogsRichBox.Font, fontStyle.Value);
              }
              
              ProxyLogsRichBox.AppendText(text);
              ProxyLogsRichBox.AppendText(Environment.NewLine);
              
              if (fontStyle.HasValue) {
                ProxyLogsRichBox.SelectionFont = new Font(ProxyLogsRichBox.Font, FontStyle.Regular);
              }
              
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
