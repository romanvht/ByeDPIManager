using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace bdmanager.Views.Tabs {
  public partial class ProxyTestTab : UserControl {
    private static readonly Regex IntegerRegex = new Regex("^[0-9]+$");
    private readonly AppSettings _settings = Program.settings;
    private readonly ProxyTestManager _manager = new ProxyTestManager();
    private readonly ObservableCollection<ProxyTestResult> _results = new ObservableCollection<ProxyTestResult>();

    public ByeDpiTab TargetByeDpiTab { get; set; }

    public ProxyTestTab() {
      InitializeComponent();
      ResultsDataGrid.ItemsSource = _results;
      _manager.LogAdded += Manager_LogAdded;
      _manager.ResultUpdated += Manager_ResultUpdated;
      _manager.ProgressChanged += Manager_ProgressChanged;
      _manager.TestingStateChanged += Manager_TestingStateChanged;
      _manager.ErrorOccurred += Manager_ErrorOccurred;
    }

    public void LoadSettings() {
      DelayTextBox.Text = _settings.ProxyTestDelay.ToString();
      RequestsCountTextBox.Text = _settings.ProxyTestRequestsCount.ToString();
      SniTextBox.Text = string.IsNullOrWhiteSpace(_settings.ProxyTestSni) ? "google.com" : _settings.ProxyTestSni;
      ProxyTestLogsTextBox.Text = ProxyTestManager.GetLatestLogs();
      ReplaceResults(_manager.LoadResults());
    }

    public void SaveSettings() {
      _settings.ProxyTestDelay = ParseInteger(DelayTextBox.Text, _settings.ProxyTestDelay, 0, int.MaxValue);
      _settings.ProxyTestRequestsCount = ParseInteger(RequestsCountTextBox.Text, _settings.ProxyTestRequestsCount, 1, int.MaxValue);
      _settings.ProxyTestSni = string.IsNullOrWhiteSpace(SniTextBox.Text) ? "google.com" : SniTextBox.Text.Trim();
    }

    public void Cleanup() {
      if (_manager.IsTesting) _manager.StopTesting();
      _manager.SaveResults();
      _manager.LogAdded -= Manager_LogAdded;
      _manager.ResultUpdated -= Manager_ResultUpdated;
      _manager.ProgressChanged -= Manager_ProgressChanged;
      _manager.TestingStateChanged -= Manager_TestingStateChanged;
      _manager.ErrorOccurred -= Manager_ErrorOccurred;
    }

    private static int ParseInteger(string text, int fallback, int minimum, int maximum) {
      if (!int.TryParse(text, out int value)) return fallback;
      return Math.Max(minimum, Math.Min(maximum, value));
    }

    private void IntegerTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
      e.Handled = !IntegerRegex.IsMatch(e.Text);
    }

    private async void ProxyTestStart_Click(object sender, RoutedEventArgs e) {
      if (_manager.IsTesting) {
        _manager.StopTesting();
        return;
      }

      SaveSettings();
      _settings.Save();
      _results.Clear();
      ProxyTestLogsTextBox.Clear();
      await _manager.StartTesting();
    }

    private void Manager_LogAdded(object sender, string text) {
      Dispatcher.BeginInvoke(new Action(() => {
        ProxyTestLogsTextBox.AppendText(text + Environment.NewLine);
        ProxyTestLogsTextBox.ScrollToEnd();
      }));
    }

    private void Manager_ResultUpdated(object sender, ProxyTestResult result) {
      Dispatcher.BeginInvoke(new Action(() => ReplaceResults(
        _results.Where(item => item.Strategy != result.Strategy)
          .Concat(new[] { result }).OrderByDescending(item => item.SuccessRate)
      )));
    }

    private void Manager_ProgressChanged(object sender, ProxyTestProgress progress) {
      Dispatcher.BeginInvoke(new Action(() => ProxyTestProgressText.Text = progress.Completed + "/" + progress.Total));
    }

    private void Manager_TestingStateChanged(object sender, bool testing) {
      Dispatcher.BeginInvoke(new Action(() => {
        ProxyTestStartButton.Content = Program.localization.GetString(
          testing ? "settings_form.proxy_test.stop" : "settings_form.proxy_test.start");
        if (!testing) ProxyTestProgressText.Text = string.Empty;
      }));
    }

    private void Manager_ErrorOccurred(object sender, string message) {
      Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(Window.GetWindow(this), message,
        Program.localization.GetString("settings_form.title"), MessageBoxButton.OK, MessageBoxImage.Error)));
    }

    private void ReplaceResults(IEnumerable<ProxyTestResult> results) {
      List<ProxyTestResult> snapshot = results.OrderByDescending(item => item.SuccessRate).ToList();
      _results.Clear();
      foreach (ProxyTestResult result in snapshot) _results.Add(result);
    }

    private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
      DataGridRow row = GetClickedRow(e);
      if (row != null) row.IsSelected = true;
    }

    private void ResultsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      if (GetClickedRow(e) != null) ApplySelectedResult();
    }

    private void ApplyResult_Click(object sender, RoutedEventArgs e) {
      ApplySelectedResult();
    }

    private void ApplySelectedResult() {
      ProxyTestResult result = ResultsDataGrid.SelectedItem as ProxyTestResult;
      if (result == null) return;
      _settings.ByeDpiArguments = result.Strategy;
      _settings.ByeDpiHistory = HistoryManager.Ensure(_settings.ByeDpiHistory);
      HistoryManager.AddOrUpdate(_settings.ByeDpiHistory, result.Strategy);
      _settings.Save();
      TargetByeDpiTab?.UpdateArguments(result.Strategy);
    }

    private void CopyResult_Click(object sender, RoutedEventArgs e) {
      ProxyTestResult result = ResultsDataGrid.SelectedItem as ProxyTestResult;
      if (result == null) return;
      try { Clipboard.SetText(result.Strategy); } catch { }
    }

    private void AlternativeLink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
      e.Handled = true;
      Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
    }

    private void EditDomains_Click(object sender, RoutedEventArgs e) {
      OpenEditableFile(ProxyTestManager.PROXY_TEST_SITES, "proxy_test.sites_file_read_error");
    }

    private void EditCommands_Click(object sender, RoutedEventArgs e) {
      OpenEditableFile(ProxyTestManager.PROXY_TEST_CMDS, "proxy_test.cmds_file_not_found");
    }

    private static void OpenEditableFile(string path, string errorKey) {
      try {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        if (!File.Exists(path)) File.WriteAllText(path, string.Empty);
        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
      }
      catch {
        Program.logger.Log(Program.localization.GetString(errorKey));
      }
    }

    private DataGridRow GetClickedRow(MouseButtonEventArgs e) {
      return ItemsControl.ContainerFromElement(ResultsDataGrid, e.OriginalSource as DependencyObject) as DataGridRow;
    }
  }
}
