using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace bdmanager {
  public partial class MainForm {
    private void InitializeComponent() {
      SuspendLayout();

      Text = Program.localization.GetString("app_name");
      Size = new Size(480, 400);
      StartPosition = FormStartPosition.CenterScreen;
      FormBorderStyle = FormBorderStyle.FixedSingle;
      MaximizeBox = false;
      FormClosing += MainForm_FormClosing;
      Load += MainForm_Load;
      Icon = GetIconFromResources();

      BackColor = Color.FromArgb(30, 30, 30);
      ForeColor = Color.White;

      TableLayoutPanel mainLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 4,
        ColumnStyles = {
            new ColumnStyle(SizeType.Percent, 100F)
        },
        RowStyles = {
            new RowStyle(SizeType.Percent, 30F),
            new RowStyle(SizeType.Percent, 15F),
            new RowStyle(SizeType.Percent, 50F),
            new RowStyle(SizeType.Percent, 5F)
        },
        Padding = new Padding(10)
      };
      Controls.Add(mainLayout);

      _toggleButton = new RoundButton {
        Text = Program.localization.GetString("main_form.connect"),
        Size = new Size(160, 80),
        BackColor = Color.FromArgb(45, 45, 45),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 11, FontStyle.Bold),
        BorderRadius = 30,
        BorderSize = 1,
        BorderColor = Color.FromArgb(100, 100, 100),
        Anchor = AnchorStyles.None
      };
      _toggleButton.Click += ToggleButton_Click;
      mainLayout.Controls.Add(_toggleButton, 0, 0);

      _settingsButton = new RoundButton {
        Text = Program.localization.GetString("main_form.settings"),
        Size = new Size(120, 40),
        BackColor = Color.FromArgb(60, 60, 60),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 9),
        BorderRadius = 20,
        BorderSize = 1,
        BorderColor = Color.FromArgb(100, 100, 100),
        Margin = new Padding(0, 0, 0, 5),
        Anchor = AnchorStyles.None
      };
      _settingsButton.Click += SettingsButton_Click;
      mainLayout.Controls.Add(_settingsButton, 0, 1);

      Panel logPanel = new Panel {
        Dock = DockStyle.Fill,
        BackColor = Color.FromArgb(20, 20, 20),
        BorderStyle = BorderStyle.FixedSingle,
        Margin = new Padding(0, 5, 0, 5)
      };
      mainLayout.Controls.Add(logPanel, 0, 2);

      _logBox = new TextBox {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Font = new Font("Consolas", 9),
        BackColor = Color.FromArgb(25, 25, 25),
        ForeColor = Color.LimeGreen,
        BorderStyle = BorderStyle.None,
        Margin = new Padding(5)
      };
      logPanel.Controls.Add(_logBox);

      _languagePanel = new TableLayoutPanel {
          Dock = DockStyle.Fill,
          BackColor = Color.FromArgb(30, 30, 30),
          ColumnCount = 1,
          RowCount = 1,
          Padding = new Padding(0),
          Margin = new Padding(0),
          ColumnStyles = { new ColumnStyle(SizeType.Percent, 100F) },
          RowStyles = { new RowStyle(SizeType.Percent, 100F) }
      };
      mainLayout.Controls.Add(_languagePanel, 0, 3);

      _notifyIcon = new NotifyIcon {
        Icon = GetIconFromResources(),
        Visible = true
      };

      ResumeLayout(false);
    }

    private void InitializeLanguage() {
      FlowLayoutPanel flowPanel = new FlowLayoutPanel {
        AutoSize = true,
        Anchor = AnchorStyles.None
      };

      foreach (string langCode in Localization.AvailableLanguages) {
        LinkLabel langLink = new LinkLabel {
          Text = Program.localization.GetLanguageName(langCode),
          LinkColor = Color.White,
          LinkBehavior = LinkBehavior.NeverUnderline,
          VisitedLinkColor = Color.White,
          ActiveLinkColor = Color.LightGray,
          AutoSize = true,
          Font = new Font("Segoe UI", 8),
          Tag = langCode,
          Margin = new Padding(0, 0, 2, 0)
        };

        langLink.Click += (s, e) => {
          if (Program.localization.CurrentLanguage != langCode) {
            Program.localization.ChangeLanguage(langCode);
            _settings.Language = langCode;
            _settings.Save();
          }
        };

        langLink.LinkColor = Program.localization.CurrentLanguage == langCode ?
            Color.LimeGreen :
            Color.White;

        flowPanel.Controls.Add(langLink);

        if (langCode != Localization.AvailableLanguages.Last()) {
          Label separator = new Label {
            Text = "|",
            ForeColor = Color.White,
            AutoSize = true,
            Font = new Font("Segoe UI", 8),
            Margin = new Padding(0, 0, 2, 0)
          };
          flowPanel.Controls.Add(separator);
        }
      }

      _languagePanel.Controls.Add(flowPanel);
    }
  }
}
