using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace bdmanager.src.ProxyTest {
  public partial class ProxyTestSettingsForm : Form {
    private ProxyTestSettings _proxyTestSettings;
    public ProxyTestSettings ProxyTestSettings { get => _proxyTestSettings; }

    public ProxyTestSettingsForm() {
      InitializeComponent();

      delayNumericUpDown.Maximum = int.MaxValue;

      requestsCountNumericUpDown.Minimum = 1;
      requestsCountNumericUpDown.Maximum = int.MaxValue;

      _proxyTestSettings = GetSettings();
      UpdateGuiSettings();
    }

    public static ProxyTestSettings GetSettings() {
      try {
        if (!File.Exists(ProxyTestManager.PROXY_TEST_CONFIG))
          return new ProxyTestSettings();

        string settingsJson = File.ReadAllText(ProxyTestManager.PROXY_TEST_CONFIG);
        ProxyTestSettings proxyTestSettings = JsonSerializer.Deserialize<ProxyTestSettings>(settingsJson);
        return proxyTestSettings;
      }
      catch {
        return new ProxyTestSettings();
      }
    }

    private void SaveSettings() {
      try {
        ApplySettings();
        string settingsJson = JsonSerializer.Serialize(_proxyTestSettings);

        Directory.CreateDirectory(ProxyTestManager.PROXY_TEST_FOLDER);

        File.WriteAllText(ProxyTestManager.PROXY_TEST_CONFIG, settingsJson);
      }
      catch {
        MessageBox.Show("Возникла ошибка при сохранении настроек.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
    }

    private void UpdateGuiSettings() {
      delayNumericUpDown.Value = _proxyTestSettings.Delay;
      requestsCountNumericUpDown.Value = _proxyTestSettings.RequestsCount;
      fullLogCheckBox.Checked = _proxyTestSettings.FullLog;

      customDomainsCheckBox.Checked = _proxyTestSettings.UseCustomDomains;
      customDomainsRichTextBox.Lines = _proxyTestSettings.CustomDomains;

      customCommandsCheckBox.Checked = _proxyTestSettings.UseCustomCommands;
      customCommandsRichTextBox.Lines = _proxyTestSettings.CustomCommands;
    }

    private void ApplySettings() {
      _proxyTestSettings.Delay = (int)delayNumericUpDown.Value;
      _proxyTestSettings.RequestsCount = (int)requestsCountNumericUpDown.Value;
      _proxyTestSettings.FullLog = fullLogCheckBox.Checked;

      _proxyTestSettings.UseCustomDomains = customDomainsCheckBox.Checked;
      _proxyTestSettings.CustomDomains = customDomainsRichTextBox.Lines;

      _proxyTestSettings.UseCustomCommands = customCommandsCheckBox.Checked;
      _proxyTestSettings.CustomCommands = customCommandsRichTextBox.Lines;
    }

    private void ApplyButton_Click(object sender, System.EventArgs e) {
      SaveSettings();
    }
  }
}
