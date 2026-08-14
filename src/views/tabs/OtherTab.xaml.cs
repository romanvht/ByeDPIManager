using System.Windows.Controls;

namespace bdmanager.Views.Tabs {
  public partial class OtherTab : UserControl {
    private readonly AppSettings _settings = Program.settings;
    private readonly AutorunManager _autorunManager = Program.autorunManager;

    public OtherTab() {
      InitializeComponent();
    }

    public void LoadSettings() {
      bool autorunEnabled = _autorunManager.IsAutorunEnabled();
      if (_settings.AutoStart != autorunEnabled) _settings.AutoStart = autorunEnabled;
      AutoStartCheckBox.IsChecked = _settings.AutoStart;
      AutoConnectCheckBox.IsChecked = _settings.AutoConnect;
      StartMinimizedCheckBox.IsChecked = _settings.StartMinimized;
      MinimizeToTrayCheckBox.IsChecked = _settings.MinimizeToTray;
    }

    public void SaveSettings() {
      _settings.AutoStart = AutoStartCheckBox.IsChecked == true;
      _settings.AutoConnect = AutoConnectCheckBox.IsChecked == true;
      _settings.StartMinimized = StartMinimizedCheckBox.IsChecked == true;
      _settings.MinimizeToTray = MinimizeToTrayCheckBox.IsChecked == true;
      _autorunManager.SetAutorun(_settings.AutoStart);
    }
  }
}
