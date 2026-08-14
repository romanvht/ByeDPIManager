using System.ComponentModel;
using System.Windows;

namespace bdmanager.Views {
  public partial class SettingsWindow : Window {
    private readonly AppSettings _settings = Program.settings;

    public SettingsWindow() {
      InitializeComponent();
      ProxyTestSettingsTab.TargetByeDpiTab = ByeDpiSettingsTab;
    }

    private void SettingsWindow_Loaded(object sender, RoutedEventArgs e) {
      ByeDpiSettingsTab.LoadSettings();
      ProxiFyreSettingsTab.LoadSettings();
      OtherSettingsTab.LoadSettings();
      ProxyTestSettingsTab.LoadSettings();
    }

    private void SettingsWindow_Closing(object sender, CancelEventArgs e) {
      ProxyTestSettingsTab.Cleanup();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e) {
      ByeDpiSettingsTab.SaveSettings();
      ProxiFyreSettingsTab.SaveSettings();
      OtherSettingsTab.SaveSettings();
      ProxyTestSettingsTab.SaveSettings();
      _settings.Save();
      Program.logger.Log(Program.localization.GetString("main_form.settings_saved"));
      DialogResult = true;
    }
  }
}
