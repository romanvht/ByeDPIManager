using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace bdmanager.Views.Tabs {
  public partial class AboutTab : UserControl {
    public AboutTab() {
      InitializeComponent();
      Assembly assembly = Assembly.GetExecutingAssembly();
      System.Version version = assembly.GetName().Version;
      VersionText.Text = version.ToString(3);
      DeveloperText.Text = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "romanvht";
      ConfigureLink(GithubLink, "settings_form.about.github_link", "https://github.com/romanvht/ByeDPIManager");
      ConfigureLink(CloudTipsLink, "settings_form.about.donate_cloudtips", "https://pay.cloudtips.ru/p/92c754db");
      ConfigureLink(BoostyLink, "settings_form.about.donate_boosty", "https://boosty.to/romanvht/donate");
      ConfigureLink(TelegramDonateLink, "settings_form.about.donate_telegram", "https://t.me/romanvht_donate_bot");
    }

    private static void ConfigureLink(Hyperlink link, string localizationKey, string url) {
      link.Inlines.Add(Program.localization.GetString(localizationKey));
      link.Tag = url;
    }

    private void OpenLink_Click(object sender, RoutedEventArgs e) {
      string url = (sender as Hyperlink)?.Tag as string;
      if (!string.IsNullOrWhiteSpace(url)) Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
  }
}
