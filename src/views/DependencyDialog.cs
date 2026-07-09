using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace bdmanager.Views {
  public class DependencyDialog : Form {
    private readonly DependencyLink[] _links = {
      new DependencyLink(".NET Framework 4.8", "https://dotnet.microsoft.com/ru-ru/download/dotnet-framework/thank-you/net48-offline-installer"),
      new DependencyLink("Windows Packet Filter", "https://github.com/wiresock/ndisapi/releases"),
      new DependencyLink("Visual C++ Redistributable 2022", "https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version"),
      new DependencyLink("Github Readme", "https://github.com/romanvht/ByeDPIManager")
    };

    public DependencyDialog() {
      InitializeComponent();
    }

    private void InitializeComponent() {
      Text = Program.localization.GetString("main_form.proxifyre_dependency_title");
      StartPosition = FormStartPosition.CenterParent;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      ShowInTaskbar = false;
      Size = new Size(520, 330);
      BackColor = SystemColors.Control;
      ForeColor = SystemColors.ControlText;

      TableLayoutPanel layout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 3,
        Padding = new Padding(16),
        RowStyles = {
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.Percent, 100F),
          new RowStyle(SizeType.AutoSize)
        }
      };
      Controls.Add(layout);

      Label messageLabel = new Label {
        Text = Program.localization.GetString("main_form.proxifyre_dependency_warning"),
        AutoSize = true,
        MaximumSize = new Size(470, 0),
        Margin = new Padding(0, 0, 0, 12)
      };
      layout.Controls.Add(messageLabel, 0, 0);

      FlowLayoutPanel linksPanel = new FlowLayoutPanel {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        Margin = new Padding(0, 0, 0, 12)
      };
      layout.Controls.Add(linksPanel, 0, 1);

      foreach (DependencyLink link in _links) {
        LinkLabel linkLabel = new LinkLabel {
          Text = link.Name,
          AutoSize = true,
          Tag = link.Url,
          Margin = new Padding(0, 0, 0, 8)
        };
        linkLabel.LinkClicked += DependencyLinkClicked;
        linksPanel.Controls.Add(linkLabel);
      }

      Button okButton = new Button {
        Text = Program.localization.GetString("settings_form.buttons.ok"),
        DialogResult = DialogResult.OK,
        Anchor = AnchorStyles.Right,
        Width = 90
      };
      layout.Controls.Add(okButton, 0, 2);

      AcceptButton = okButton;
      CancelButton = okButton;
    }

    private void DependencyLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
      LinkLabel linkLabel = (LinkLabel)sender;
      string url = (string)linkLabel.Tag;

      Process.Start(new ProcessStartInfo {
        FileName = url,
        UseShellExecute = true
      });
    }

    private class DependencyLink {
      public string Name { get; }
      public string Url { get; }

      public DependencyLink(string name, string url) {
        Name = name;
        Url = url;
      }
    }
  }
}
