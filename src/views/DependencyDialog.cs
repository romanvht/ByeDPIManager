using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace bdmanager.Views {
  public class DependencyDialog : Form {
    private readonly Color _backgroundColor = Color.FromArgb(30, 30, 30);
    private readonly Color _panelColor = Color.FromArgb(20, 20, 20);
    private readonly Color _cardColor = Color.FromArgb(38, 38, 38);
    private readonly Color _borderColor = Color.FromArgb(100, 100, 100);
    private readonly Color _mutedTextColor = Color.FromArgb(190, 190, 190);
    private readonly Color _accentColor = Color.LimeGreen;
    private readonly Color _warningColor = Color.FromArgb(255, 193, 7);

    private readonly DependencyLink[] _links = {
      new DependencyLink(".NET Framework 4.8", "main_form.proxifyre_dependency_link_net", "https://dotnet.microsoft.com/ru-ru/download/dotnet-framework/thank-you/net48-offline-installer"),
      new DependencyLink("Windows Packet Filter", "main_form.proxifyre_dependency_link_wpf", "https://github.com/wiresock/ndisapi/releases"),
      new DependencyLink("Visual C++ Redistributable 2022", "main_form.proxifyre_dependency_link_vc", "https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version"),
      new DependencyLink("GitHub README", "main_form.proxifyre_dependency_link_readme", "https://github.com/romanvht/ByeDPIManager")
    };

    public DependencyDialog() {
      InitializeComponent();

      DpiScaler.Scale(this);
    }

    private void InitializeComponent() {
      Text = Program.localization.GetString("main_form.proxifyre_dependency_title");
      StartPosition = FormStartPosition.CenterParent;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      ShowInTaskbar = false;
      Size = new Size(600, 670);
      BackColor = _backgroundColor;
      ForeColor = Color.White;
      Font = new Font("Segoe UI", 9F);

      TableLayoutPanel layout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 4,
        BackColor = _backgroundColor,
        Padding = new Padding(18),
        RowStyles = {
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.Percent, 100F),
          new RowStyle(SizeType.AutoSize)
        }
      };
      Controls.Add(layout);

      RoundedPanel headerPanel = new RoundedPanel {
        Dock = DockStyle.Top,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        MinimumSize = new Size(0, 80),
        BackColor = _panelColor,
        BorderColor = _borderColor,
        BorderRadius = 8,
        Margin = new Padding(0, 0, 0, 12)
      };
      layout.Controls.Add(headerPanel, 0, 0);

      TableLayoutPanel headerLayout = new TableLayoutPanel {
        Dock = DockStyle.Top,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 2,
        RowCount = 2,
        BackColor = Color.Transparent,
        Padding = new Padding(16, 13, 16, 13),
        ColumnStyles = {
          new ColumnStyle(SizeType.Absolute, 48F),
          new ColumnStyle(SizeType.Percent, 100F)
        },
        RowStyles = {
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.AutoSize)
        }
      };
      headerPanel.Controls.Add(headerLayout);

      Label iconLabel = new Label {
        Text = "!",
        Dock = DockStyle.Fill,
        ForeColor = _warningColor,
        Font = new Font("Segoe UI", 24F, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleCenter,
        Margin = new Padding(0, 0, 10, 0)
      };
      headerLayout.Controls.Add(iconLabel, 0, 0);
      headerLayout.SetRowSpan(iconLabel, 2);

      Label titleLabel = CreateLabel(
        Program.localization.GetString("main_form.proxifyre_dependency_heading"),
        Color.White,
        new Font("Segoe UI", 13F, FontStyle.Bold)
      );
      titleLabel.Margin = new Padding(0, 0, 0, 6);
      headerLayout.Controls.Add(titleLabel, 1, 0);

      Label summaryLabel = CreateLabel(
        Program.localization.GetString("main_form.proxifyre_dependency_summary"),
        _mutedTextColor,
        new Font("Segoe UI", 9F)
      );
      summaryLabel.MaximumSize = new Size(470, 0);
      headerLayout.Controls.Add(summaryLabel, 1, 1);

      RoundedPanel detailsPanel = new RoundedPanel {
        Dock = DockStyle.Top,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        MinimumSize = new Size(0, 95),
        BackColor = _cardColor,
        BorderColor = _borderColor,
        BorderRadius = 8,
        Margin = new Padding(0, 0, 0, 12)
      };
      layout.Controls.Add(detailsPanel, 0, 1);

      TableLayoutPanel detailsLayout = new TableLayoutPanel {
        Dock = DockStyle.Top,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 1,
        RowCount = 3,
        BackColor = Color.Transparent,
        Padding = new Padding(16, 12, 16, 6),
        RowStyles = {
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.AutoSize)
        }
      };
      detailsPanel.Controls.Add(detailsLayout);

      Label messageLabel = CreateLabel(
        Program.localization.GetString("main_form.proxifyre_dependency_warning"),
        Color.White,
        new Font("Segoe UI", 9F, FontStyle.Bold)
      );
      messageLabel.Margin = new Padding(0, 0, 0, 8);
      detailsLayout.Controls.Add(messageLabel, 0, 0);

      AddInfoLine(detailsLayout, Program.localization.GetString("main_form.proxifyre_dependency_redist"));
      AddInfoLine(detailsLayout, Program.localization.GetString("main_form.proxifyre_dependency_restart"));

      TableLayoutPanel linksPanel = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = _links.Length + 1,
        AutoScroll = true,
        BackColor = _backgroundColor,
        Margin = new Padding(0, 0, 0, 14),
        Padding = new Padding(0, 0, 0, 0),
        ColumnStyles = {
          new ColumnStyle(SizeType.Percent, 100F)
        }
      };
      layout.Controls.Add(linksPanel, 0, 2);

      for (int index = 0; index < _links.Length; index++) {
        linksPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        linksPanel.Controls.Add(CreateLinkCard(_links[index]), 0, index);
      }
      linksPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

      FlowLayoutPanel footerPanel = new FlowLayoutPanel {
        Dock = DockStyle.Top,
        Height = 42,
        FlowDirection = FlowDirection.RightToLeft,
        WrapContents = false,
        AutoSize = true,
        BackColor = _backgroundColor,
        Margin = new Padding(0)
      };
      layout.Controls.Add(footerPanel, 0, 3);

      RoundButton okButton = new RoundButton {
        Text = Program.localization.GetString("settings_form.buttons.ok"),
        DialogResult = DialogResult.OK,
        Size = new Size(110, 38),
        BackColor = Color.FromArgb(60, 60, 60),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        BorderRadius = 18,
        BorderSize = 1,
        BorderColor = _borderColor,
        Margin = new Padding(0)
      };
      footerPanel.Controls.Add(okButton);

      AcceptButton = okButton;
      CancelButton = okButton;
    }

    private Label CreateLabel(string text, Color color, Font font) {
      return new Label {
        Text = text,
        AutoSize = true,
        ForeColor = color,
        BackColor = Color.Transparent,
        Font = font,
        MaximumSize = new Size(540, 0),
        Margin = new Padding(0)
      };
    }

    private void AddInfoLine(TableLayoutPanel layout, string text) {
      Label lineLabel = CreateLabel(text, _mutedTextColor, new Font("Segoe UI", 9F));
      lineLabel.Margin = new Padding(0, 0, 0, 5);
      layout.Controls.Add(lineLabel, 0, layout.Controls.Count);
    }

    private Control CreateLinkCard(DependencyLink link) {
      RoundedPanel card = new RoundedPanel {
        Dock = DockStyle.Fill,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        MinimumSize = new Size(0, 74),
        BackColor = _panelColor,
        BorderColor = _borderColor,
        BorderRadius = 8,
        Margin = new Padding(0, 0, 0, 10),
        Tag = link.Url,
        Cursor = Cursors.Hand
      };
      card.Click += DependencyCardClicked;

      TableLayoutPanel cardLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 2,
        RowCount = 1,
        BackColor = Color.Transparent,
        Padding = new Padding(16, 10, 14, 10),
        ColumnStyles = {
          new ColumnStyle(SizeType.Percent, 100F),
          new ColumnStyle(SizeType.Absolute, 78F)
        },
        RowStyles = {
          new RowStyle(SizeType.AutoSize)
        },
        Cursor = Cursors.Hand,
        Tag = link.Url
      };
      card.Controls.Add(cardLayout);
      cardLayout.Click += DependencyCardClicked;

      TableLayoutPanel textLayout = new TableLayoutPanel {
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        Anchor = AnchorStyles.Left,
        ColumnCount = 1,
        RowCount = 2,
        BackColor = Color.Transparent,
        Cursor = Cursors.Hand,
        Tag = link.Url,
        RowStyles = {
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.AutoSize)
        }
      };
      textLayout.Click += DependencyCardClicked;
      cardLayout.Controls.Add(textLayout, 0, 0);

      LinkLabel titleLink = new LinkLabel {
        Text = link.Name,
        AutoSize = true,
        LinkColor = _accentColor,
        ActiveLinkColor = Color.White,
        VisitedLinkColor = _accentColor,
        LinkBehavior = LinkBehavior.HoverUnderline,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        Tag = link.Url,
        Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
        Margin = new Padding(0, 0, 0, 2),
        Cursor = Cursors.Hand
      };
      titleLink.LinkClicked += DependencyLinkClicked;
      textLayout.Controls.Add(titleLink, 0, 0);

      Label descriptionLabel = CreateLabel(
        Program.localization.GetString(link.DescriptionKey),
        _mutedTextColor,
        new Font("Segoe UI", 8.5F)
      );
      descriptionLabel.MaximumSize = new Size(420, 0);
      descriptionLabel.Dock = DockStyle.Top;
      descriptionLabel.TextAlign = ContentAlignment.TopLeft;
      descriptionLabel.Cursor = Cursors.Hand;
      descriptionLabel.Tag = link.Url;
      descriptionLabel.Click += DependencyCardClicked;
      textLayout.Controls.Add(descriptionLabel, 0, 1);

      Label openLabel = new Label {
        Text = Program.localization.GetString("main_form.proxifyre_dependency_open"),
        Dock = DockStyle.Fill,
        ForeColor = _mutedTextColor,
        BackColor = Color.Transparent,
        Font = new Font("Segoe UI", 8F, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleRight,
        Margin = new Padding(10, 0, 0, 0),
        Cursor = Cursors.Hand,
        Tag = link.Url
      };
      openLabel.Click += DependencyCardClicked;
      cardLayout.Controls.Add(openLabel, 1, 0);

      return card;
    }

    private void DependencyLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
      LinkLabel linkLabel = (LinkLabel)sender;
      string url = (string)linkLabel.Tag;

      OpenUrl(url);
    }

    private void DependencyCardClicked(object sender, EventArgs e) {
      Control control = (Control)sender;
      while (control != null && !(control.Tag is string)) {
        control = control.Parent;
      }

      if (control != null) {
        OpenUrl((string)control.Tag);
      }
    }

    private void OpenUrl(string url) {
      Process.Start(new ProcessStartInfo {
        FileName = url,
        UseShellExecute = true
      });
    }

  }
}
