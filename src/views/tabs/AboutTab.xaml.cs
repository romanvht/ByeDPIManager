using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace bdmanager.Views.Tabs {
  public partial class AboutTab : UserControl {
    public AboutTab() {
      InitializeComponent();
      Assembly assembly = Assembly.GetExecutingAssembly();
      VersionText.Text = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? assembly.GetName().Version.ToString();
      DeveloperText.Text = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "romanvht";
      string github = Program.localization.GetString("settings_form.about.github_link");
      GithubLink.Inlines.Add(github);
      GithubLink.Tag = github;
    }

    private void GithubLink_Click(object sender, RoutedEventArgs e) {
      string url = GithubLink.Tag as string;
      if (!string.IsNullOrWhiteSpace(url)) Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
  }
}
