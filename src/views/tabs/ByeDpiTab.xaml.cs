using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace bdmanager.Views.Tabs {
  public partial class ByeDpiTab : UserControl {
    private static readonly Regex IntegerRegex = new Regex("^[0-9]+$");
    private readonly AppSettings _settings = Program.settings;
    private readonly ObservableCollection<HistoryRow> _historyRows = new ObservableCollection<HistoryRow>();

    public ByeDpiTab() {
      InitializeComponent();
      HistoryDataGrid.ItemsSource = _historyRows;
    }

    public void LoadSettings() {
      ByeDpiPathTextBox.Text = _settings.ByeDpiPath;
      HotkeyTextBox.Text = _settings.Hotkey;
      ByeDpiArgsTextBox.Text = _settings.ByeDpiArguments;
      ByeDpiIpTextBox.Text = _settings.ByeDpiIp;
      ByeDpiPortTextBox.Text = _settings.ByeDpiPort.ToString();
      _settings.ByeDpiHistory = HistoryManager.Ensure(_settings.ByeDpiHistory);
      HistoryManager.AddOrUpdate(_settings.ByeDpiHistory, _settings.ByeDpiArguments);
      RefreshHistory();
    }

    public void SaveSettings() {
      _settings.ByeDpiPath = ByeDpiPathTextBox.Text;
      _settings.Hotkey = HotkeyTextBox.Text;
      _settings.ByeDpiArguments = ByeDpiArgsTextBox.Text;
      _settings.ByeDpiIp = ParseIp(ByeDpiIpTextBox.Text, _settings.ByeDpiIp);
      _settings.ByeDpiPort = ParsePort(ByeDpiPortTextBox.Text, _settings.ByeDpiPort);
      _settings.ByeDpiHistory = HistoryManager.Ensure(_settings.ByeDpiHistory);
      HistoryManager.AddOrUpdate(_settings.ByeDpiHistory, _settings.ByeDpiArguments);
      RefreshHistory();
    }

    public void UpdateArguments(string arguments) {
      if (!Dispatcher.CheckAccess()) {
        Dispatcher.BeginInvoke(new Action(() => UpdateArguments(arguments)));
        return;
      }
      ByeDpiArgsTextBox.Text = arguments;
      RefreshHistory();
    }

    private static string ParseIp(string text, string fallback) {
      string value = (text ?? string.Empty).Trim().Trim('[', ']');
      return IPAddress.TryParse(value, out IPAddress ignoredAddress) ? value : fallback;
    }

    private static int ParsePort(string text, int fallback) {
      if (!int.TryParse(text, out int value)) return fallback;
      return Math.Max(1, Math.Min(65535, value));
    }

    private void IntegerTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
      e.Handled = !IntegerRegex.IsMatch(e.Text);
    }

    private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
      Key key = e.Key == Key.System ? e.SystemKey : e.Key;
      if (key == Key.Back || key == Key.Delete) {
        HotkeyTextBox.Clear();
        e.Handled = true;
        return;
      }

      if (key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftAlt || key == Key.RightAlt ||
          key == Key.LeftShift || key == Key.RightShift || key == Key.LWin || key == Key.RWin || key == Key.Tab) return;

      List<string> parts = new List<string>();
      ModifierKeys modifiers = Keyboard.Modifiers;
      if ((modifiers & ModifierKeys.Control) != 0) parts.Add("Ctrl");
      if ((modifiers & ModifierKeys.Alt) != 0) parts.Add("Alt");
      if ((modifiers & ModifierKeys.Shift) != 0) parts.Add("Shift");
      if ((modifiers & ModifierKeys.Windows) != 0) parts.Add("Win");
      parts.Add(key.ToString());
      HotkeyTextBox.Text = string.Join("+", parts);
      HotkeyTextBox.CaretIndex = HotkeyTextBox.Text.Length;
      e.Handled = true;
    }

    private void BrowseByeDpi_Click(object sender, RoutedEventArgs e) {
      OpenFileDialog dialog = new OpenFileDialog {
        Filter = "*.exe|*.exe|*.*|*.*",
        Title = Program.localization.GetString("settings_form.byedpi.browse_title")
      };
      Window owner = Window.GetWindow(this);
      if (dialog.ShowDialog(owner) != true) return;

      if (IsCurrentApplication(dialog.FileName)) {
        MessageBox.Show(owner, Program.localization.GetString("settings_form.path_self_error"),
          Program.localization.GetString("settings_form.title"), MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      ByeDpiPathTextBox.Text = dialog.FileName;
    }

    private static bool IsCurrentApplication(string path) {
      try {
        return string.Equals(Path.GetFullPath(path), Path.GetFullPath(Assembly.GetEntryAssembly().Location),
          StringComparison.OrdinalIgnoreCase);
      }
      catch {
        return false;
      }
    }

    private void RefreshHistory() {
      string current = (ByeDpiArgsTextBox.Text ?? string.Empty).Trim();
      _historyRows.Clear();
      foreach (HistoryItem item in HistoryManager.GetItems(_settings.ByeDpiHistory)) {
        _historyRows.Add(new HistoryRow(item, current));
      }
    }

    private HistoryItem GetSelectedItem() {
      return (HistoryDataGrid.SelectedItem as HistoryRow)?.Item;
    }

    private void HistoryDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      if (GetClickedRow(e) != null) ApplySelectedItem();
    }

    private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
      DataGridRow row = GetClickedRow(e);
      if (row != null) row.IsSelected = true;
    }

    private void ApplyHistory_Click(object sender, RoutedEventArgs e) {
      ApplySelectedItem();
    }

    private void ApplySelectedItem() {
      HistoryItem item = GetSelectedItem();
      if (item == null) return;
      ByeDpiArgsTextBox.Text = item.Arguments;
      _settings.ByeDpiArguments = item.Arguments;
      HistoryManager.AddOrUpdate(_settings.ByeDpiHistory, item.Arguments, item.Name);
      _settings.Save();
      RefreshHistory();
    }

    private void TogglePinHistory_Click(object sender, RoutedEventArgs e) {
      HistoryItem item = GetSelectedItem();
      if (item == null) return;
      item.IsPinned = !item.IsPinned;
      _settings.Save();
      RefreshHistory();
    }

    private void RenameHistory_Click(object sender, RoutedEventArgs e) {
      HistoryItem item = GetSelectedItem();
      if (item == null) return;
      RenameWindow dialog = new RenameWindow(item.Name) { Owner = Window.GetWindow(this) };
      if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.Value)) return;
      item.Name = dialog.Value.Trim();
      _settings.Save();
      RefreshHistory();
    }

    private void DeleteHistory_Click(object sender, RoutedEventArgs e) {
      HistoryItem item = GetSelectedItem();
      if (item == null) return;
      HistoryManager.Remove(_settings.ByeDpiHistory, item);
      _settings.Save();
      RefreshHistory();
    }

    private DataGridRow GetClickedRow(MouseButtonEventArgs e) {
      return ItemsControl.ContainerFromElement(HistoryDataGrid, e.OriginalSource as DependencyObject) as DataGridRow;
    }

    private sealed class HistoryRow {
      public HistoryItem Item { get; }
      public string Marker { get; }
      public string DisplayText { get; }

      public HistoryRow(HistoryItem item, string currentArguments) {
        Item = item;
        Marker = string.Equals(item.Arguments.Trim(), currentArguments, StringComparison.Ordinal)
          ? "✓" : item.IsPinned ? "★" : string.Empty;
        DisplayText = string.IsNullOrWhiteSpace(item.Name)
          ? item.Arguments : item.Name + ":" + Environment.NewLine + item.Arguments;
      }
    }
  }
}
