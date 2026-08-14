using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace bdmanager.Views.Tabs {
  public partial class ProxiFyreTab : UserControl {
    private static readonly Regex IntegerRegex = new Regex("^[0-9]+$");
    private readonly AppSettings _settings = Program.settings;

    public ProxiFyreTab() {
      InitializeComponent();
    }

    public void LoadSettings() {
      ProxiFyrePathTextBox.Text = _settings.ProxiFyrePath;
      ProxiFyrePortTextBox.Text = _settings.ProxiFyrePort.ToString();
      ProxyLanCheckBox.IsChecked = _settings.ProxiFyreLan;
      DisableProxiFyreCheckBox.IsChecked = _settings.DisableProxiFyre;
      AppListBox.Items.Clear();
      foreach (string app in _settings.ProxifiedApps ?? new List<string>()) AppListBox.Items.Add(app);
    }

    public void SaveSettings() {
      _settings.ProxiFyreLan = ProxyLanCheckBox.IsChecked == true;
      _settings.DisableProxiFyre = DisableProxiFyreCheckBox.IsChecked == true;
      _settings.ProxiFyrePath = ProxiFyrePathTextBox.Text;
      _settings.ProxiFyrePort = ParsePort(ProxiFyrePortTextBox.Text, _settings.ProxiFyrePort);
      _settings.ProxifiedApps.Clear();
      foreach (object item in AppListBox.Items) _settings.ProxifiedApps.Add(item.ToString());
    }

    private static int ParsePort(string text, int fallback) {
      if (!int.TryParse(text, out int value)) return fallback;
      return Math.Max(1, Math.Min(65535, value));
    }

    private void IntegerTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
      e.Handled = !IntegerRegex.IsMatch(e.Text);
    }

    private void BrowseProxiFyre_Click(object sender, RoutedEventArgs e) {
      BrowseForExe(ProxiFyrePathTextBox, Program.localization.GetString("settings_form.proxifyre.browse_title"));
    }

    private void BrowseAppFile_Click(object sender, RoutedEventArgs e) {
      if (BrowseForExe(AppTextBox, Program.localization.GetString("settings_form.apps.browse_title"))) AddApp();
    }

    private void BrowseAppFolder_Click(object sender, RoutedEventArgs e) {
      if (BrowseForFolder(AppTextBox, Program.localization.GetString("settings_form.apps.browse_folder_title"))) AddApp();
    }

    private bool BrowseForExe(TextBox target, string title) {
      OpenFileDialog dialog = new OpenFileDialog { Filter = "*.exe|*.exe|*.*|*.*", Title = title };
      Window owner = Window.GetWindow(this);
      if (dialog.ShowDialog(owner) != true) return false;

      if (IsCurrentApplication(dialog.FileName)) {
        MessageBox.Show(owner, Program.localization.GetString("settings_form.path_self_error"),
          Program.localization.GetString("settings_form.title"), MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
      }
      target.Text = dialog.FileName;
      return true;
    }

    private bool BrowseForFolder(TextBox target, string title) {
      Window owner = Window.GetWindow(this);
      IntPtr ownerHandle = owner == null ? IntPtr.Zero : new WindowInteropHelper(owner).Handle;
      if (!NativeFolderDialog.TryShow(ownerHandle, title, target.Text, out string selectedPath)) return false;
      target.Text = selectedPath;
      return true;
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

    private void AddApp_Click(object sender, RoutedEventArgs e) {
      AddApp();
    }

    private void AddApp() {
      string app = AppTextBox.Text.Trim();
      if (string.IsNullOrWhiteSpace(app)) return;
      if (AppListBox.Items.Cast<object>().Any(item => string.Equals(item.ToString(), app, StringComparison.OrdinalIgnoreCase))) {
        MessageBox.Show(Window.GetWindow(this), Program.localization.GetString("settings_form.apps.already_added"),
          Program.localization.GetString("settings_form.title"), MessageBoxButton.OK, MessageBoxImage.Information);
        return;
      }
      AppListBox.Items.Add(app);
      AppTextBox.Clear();
    }

    private void RemoveApp_Click(object sender, RoutedEventArgs e) {
      if (AppListBox.SelectedItem != null) AppListBox.Items.Remove(AppListBox.SelectedItem);
    }
  }
}
