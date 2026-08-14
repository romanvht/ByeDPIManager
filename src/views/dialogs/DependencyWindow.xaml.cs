using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace bdmanager.Views {
  public partial class DependencyWindow : Window {
    public DependencyWindow() {
      InitializeComponent();
      Title = L("main_form.proxifyre_dependency_title");
      HeadingText.Text = L("main_form.proxifyre_dependency_heading");
      SummaryText.Text = L("main_form.proxifyre_dependency_summary");
      WarningText.Text = L("main_form.proxifyre_dependency_warning");
      RedistText.Text = L("main_form.proxifyre_dependency_redist");
      RestartText.Text = L("main_form.proxifyre_dependency_restart");
      OkButton.Content = L("settings_form.buttons.ok");

      string openText = L("main_form.proxifyre_dependency_open");
      LinksItemsControl.ItemsSource = new List<DependencyLinkView> {
        new DependencyLinkView(".NET Framework 4.8", L("main_form.proxifyre_dependency_link_net"), "https://dotnet.microsoft.com/download/dotnet-framework/net48", openText),
        new DependencyLinkView("Windows Packet Filter", L("main_form.proxifyre_dependency_link_wpf"), "https://github.com/wiresock/ndisapi/releases", openText),
        new DependencyLinkView("Visual C++ Redistributable 2022", L("main_form.proxifyre_dependency_link_vc"), "https://learn.microsoft.com/cpp/windows/latest-supported-vc-redist", openText),
        new DependencyLinkView("GitHub README", L("main_form.proxifyre_dependency_link_readme"), "https://github.com/romanvht/ByeDPIManager", openText)
      };
    }

    private static string L(string key) {
      return Program.localization.GetString(key);
    }

    private void DependencyCard_Click(object sender, MouseButtonEventArgs e) {
      FrameworkElement element = sender as FrameworkElement;
      string url = element?.Tag as string;
      if (string.IsNullOrWhiteSpace(url)) return;
      Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }

    private void OkButton_Click(object sender, RoutedEventArgs e) {
      DialogResult = true;
    }

    private sealed class DependencyLinkView {
      public string Name { get; }
      public string Description { get; }
      public string Url { get; }
      public string OpenText { get; }

      public DependencyLinkView(string name, string description, string url, string openText) {
        Name = name;
        Description = description;
        Url = url;
        OpenText = openText;
      }
    }
  }
}
