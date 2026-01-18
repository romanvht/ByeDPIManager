using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace bdmanager.Views.Tabs {
  public class AboutTab : TabPage {
    public AboutTab() {
      InitializeTab();
    }

    private void InitializeTab() {
      Text = Program.localization.GetString("settings_form.about.tab");
      Name = "aboutTabPage";
      BackColor = SystemColors.Control;
      Padding = new Padding(10);

      GroupBox aboutGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.about.group"),
        Name = "aboutGroupBox",
        Dock = DockStyle.Fill,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10)
      };
      Controls.Add(aboutGroupBox);

      TableLayoutPanel aboutLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        RowCount = 3,
        ColumnStyles = {
          new ColumnStyle(SizeType.AutoSize),
          new ColumnStyle(SizeType.Percent, 100F)
        },
        RowStyles = {
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.AutoSize)
        },
        Name = "aboutLayout",
        AutoSize = true
      };
      aboutGroupBox.Controls.Add(aboutLayout);

      Assembly asm = Assembly.GetExecutingAssembly();
      string version = asm.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
      string author = asm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

      Label versionLabel = new Label {
        Text = Program.localization.GetString("settings_form.about.version_label"),
        Name = "versionLabel",
        Margin = new Padding(0, 3, 0, 3),
      };
      aboutLayout.Controls.Add(versionLabel, 0, 0);

      Label versionValueLabel = new Label {
        Text = version,
        Name = "versionValueLabel",
        Margin = new Padding(10, 3, 0, 3),
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
      };
      aboutLayout.Controls.Add(versionValueLabel, 1, 0);

      Label developerLabel = new Label {
        Text = Program.localization.GetString("settings_form.about.developer_label"),
        Name = "developerLabel",
        Margin = new Padding(0, 3, 0, 3),
      };
      aboutLayout.Controls.Add(developerLabel, 0, 1);

      Label developerValueLabel = new Label {
        Text = author,
        Name = "developerValueLabel",
        Margin = new Padding(10, 3, 0, 3),
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
      };
      aboutLayout.Controls.Add(developerValueLabel, 1, 1);

      Label githubLabel = new Label {
        Text = Program.localization.GetString("settings_form.about.github_label"),
        Name = "githubLabel",
        Margin = new Padding(0, 3, 0, 3),
      };
      aboutLayout.Controls.Add(githubLabel, 0, 2);

      LinkLabel githubLinkLabel = new LinkLabel {
        Text = Program.localization.GetString("settings_form.about.github_link"),
        Name = "githubLinkLabel",
        Margin = new Padding(10, 3, 0, 3),
        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
      };
      githubLinkLabel.LinkClicked += (s, e) => {
        System.Diagnostics.Process.Start(githubLinkLabel.Text);
      };
      aboutLayout.Controls.Add(githubLinkLabel, 1, 2);
    }
  }
}
