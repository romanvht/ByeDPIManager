using SocksSharp;
using SocksSharp.Proxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bdmanager.src.ProxyTest {
  public class ProxyTestManager {
    public const string PROXY_TEST_BUTTON_START = "Старт";
    public const string PROXY_TEST_BUTTON_STOP = "Стоп";

    public const string PROXY_TEST_FOLDER = "./proxytest";
    public const string PROXY_TEST_CONFIG = PROXY_TEST_FOLDER + "/proxytest_cfg.json";
    public const string PROXY_TEST_CMDS = PROXY_TEST_FOLDER + "/proxytest_cmds.txt";
    public const string PROXY_TEST_SITES = PROXY_TEST_FOLDER + "/proxytest_sites.txt";
    public const string PROXY_TEST_LATEST_LOG = PROXY_TEST_FOLDER + "/proxytest_latest.log";

    public bool IsTesting { get; private set; }
    public Button ProxyTestStartButton { get; set; }
    public RichTextBox ProxyLogsRichBox { get; set; }
    private AppSettings _settings = Program.settings;
    private Process _byeDpiProcess;
    private CancellationTokenSource _cancellationTokenSource;

    public ProxyTestManager() {
      _settings = Program.settings;
    }

    public static string GetLatestLogs() {
      try {
        if (!File.Exists(PROXY_TEST_LATEST_LOG))
          return string.Empty;
        string latestLog = File.ReadAllText(PROXY_TEST_LATEST_LOG);
        return latestLog;
      }
      catch { }
      return string.Empty;
    }

    private static void ClearLatestLogs() {
      try {
        File.Delete(PROXY_TEST_LATEST_LOG);
      }
      catch { }
    }

    private string[] GetDomains() {
      try {
        string[] domains = File.ReadAllLines(PROXY_TEST_SITES);
        return domains;
      }
      catch { }
      return Array.Empty<string>();
    }

    private Task<string[]> GetDomainsAsync() => Task.Run(() => GetDomains());

    private string[] GetCommands() {
      try {
        string[] commands = File.ReadAllLines(PROXY_TEST_CMDS);
        return commands;
      }
      catch { }
      return Array.Empty<string>();
    }

    private Task<string[]> GetCommandsAsync() => Task.Run(() => GetCommands());

    private static Uri GetValidUrl(string domain) {
      domain = domain.Trim();
      if (domain.StartsWith("http://") || domain.StartsWith("https://"))
        return new Uri(domain);
      return new Uri($"https://{domain}");
    }

    public async Task StartTesting(ProxyTestSettings proxyTestSettings) {
      try {
        IsTesting = true;
        ProxyTestStartButton.Text = PROXY_TEST_BUTTON_STOP;
        ProxyLogsRichBox.Clear();
        ClearLatestLogs();
        _cancellationTokenSource = new CancellationTokenSource();

        IEnumerable<string> commands = proxyTestSettings.UseCustomCommands ? proxyTestSettings.CustomCommands : await GetCommandsAsync();
        IEnumerable<string> domains = proxyTestSettings.UseCustomDomains ? proxyTestSettings.CustomDomains : await GetDomainsAsync();

        var commandsResults = await CheckDomainsAccessAsync(commands, domains, proxyTestSettings, _cancellationTokenSource.Token);
        var sortedResults = commandsResults.OrderByDescending(key => key.Value).ToArray();

        AppendLogLine("Команды с успехом более 50%, применяйте от большего к меньшему");
        AppendLogLine(string.Empty);

        for (int i = 0; i < sortedResults.Length; i++) {
          var result = sortedResults[i];
          int orderNumber = i + 1;
          AppendLogLine($"{orderNumber}. {result.Key}");
          AppendLogLine($"{result.Value}%");
          AppendLogLine(string.Empty);
        }

        StopTesting();
      }
      catch (Exception ex) {
        bool canceledByUser = false;
        
        if (ex is OperationCanceledException) {
          canceledByUser = true;
        }

        string errorMessage;
        if (canceledByUser)
          errorMessage = "Подбор команд остановлен.";
        else {
          errorMessage = $"Подбор команд остановлен. Произошла ошибка: {ex}";
          StopTesting();
        }

        AppendLogLine(errorMessage);
      }
    }

    public void StopTesting() {
      _cancellationTokenSource?.Cancel();
      try {
        StopByeDpi();
      }
      catch { }
      IsTesting = false;
      ProxyTestStartButton.Text = PROXY_TEST_BUTTON_START;
    }

    private void StartByeDpi(string arguments) {
      if (!File.Exists(_settings.ByeDpiPath)) {
        MessageBox.Show($"Файл ByeDPI не найден: {_settings.ByeDpiPath}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        return;
      }
    }

    private void StopByeDpi() {
      if (_byeDpiProcess?.HasExited == false) {
        try {
          _byeDpiProcess.Kill();
          _byeDpiProcess = null;

        }
        catch (Exception ex) {
          MessageBox.Show($"Ошибка при остановке ByeDPI: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    private async Task<List<KeyValuePair<string, int>>> CheckDomainsAccessAsync(IEnumerable<string> commands, IEnumerable<string> domains, ProxyTestSettings proxyTestSettings, CancellationToken cancellationToken) {
      List<KeyValuePair<string, int>> commandsResults = new List<KeyValuePair<string, int>>();

      foreach (string command in commands) {
        cancellationToken.ThrowIfCancellationRequested();

        StartByeDpi(command);
        AppendLogLine(command, FontStyle.Bold);

        int requestsCount = proxyTestSettings.RequestsCount;
        int delay = proxyTestSettings.Delay;

        int totalRequests = requestsCount * domains.Count();
        int totalSuccessRequests = 0;
        foreach (string domain in domains) {
          cancellationToken.ThrowIfCancellationRequested();

          string trimmedDomain = domain.Trim();
          int successRequests = await CheckDomainAccessAsync(trimmedDomain, requestsCount, cancellationToken);
          if (proxyTestSettings.FullLog)
            AppendLogLine($"{trimmedDomain} - {successRequests}/{requestsCount}");
          totalSuccessRequests += successRequests;
        }
        int successPercentage = (totalSuccessRequests * 100) / totalRequests;
        AppendLogLine($"{totalSuccessRequests}/{totalRequests} ({successPercentage}%)");
        if (successPercentage > 50)
          commandsResults.Add(new KeyValuePair<string, int>(command, successPercentage));
        AppendLogLine(Environment.NewLine);
        StopByeDpi();

        await Task.Delay(delay * 1000, cancellationToken);
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
        using (ProxyClientHandler<Socks5> proxyClientHandler = new ProxyClientHandler<Socks5>(proxySettings)) {
          using (HttpClient httpClient = new HttpClient(proxyClientHandler)) {
            for (int i = 0; i < requestsCount; i++) {
              var response = await httpClient.GetAsync(websiteUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

              successRequests++;
            }
          }
        }
      }
      catch { }

      return successRequests;
    }

    private void AppendLogLine(string text) {
      ProxyLogsRichBox.AppendText(text);
      ProxyLogsRichBox.AppendText(Environment.NewLine);

      try {
        File.AppendAllText(PROXY_TEST_LATEST_LOG, $"{text}{Environment.NewLine}");
      }
      catch { }
    }

    private void AppendLogLine(string text, FontStyle fontStyle) {
      ProxyLogsRichBox.SelectionFont = new Font(ProxyLogsRichBox.Font, fontStyle);
      AppendLogLine(text);
      ProxyLogsRichBox.SelectionFont = new Font(ProxyLogsRichBox.Font, FontStyle.Regular);
    }
  }
}
