using System;
using System.Drawing;
using System.Windows.Forms;

namespace bdmanager.Views.Tabs {
  public class ByeDpiTab : TabPage {
    private AppSettings _settings;

    public TextBox ByeDpiPathTextBox { get; private set; }
    public HotkeyTextBox HotkeyTextBox { get; private set; }
    public TextBox ByeDpiArgsTextBox { get; private set; }

    public ByeDpiTab(AppSettings settings) {
      _settings = settings;
      InitializeTab();
    }

    private void InitializeTab() {
      Text = Program.localization.GetString("settings_form.byedpi.tab");
      Name = "byeDpiTabPage";
      BackColor = SystemColors.Control;
      Padding = new Padding(10);

      GroupBox byeDpiGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.byedpi.group"),
        Name = "byeDpiGroupBox",
        Dock = DockStyle.Fill,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10, 5, 10, 10)
      };
      Controls.Add(byeDpiGroupBox);

      TableLayoutPanel byeDpiLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 3,
        RowCount = 4,
        ColumnStyles = {
          new ColumnStyle(SizeType.AutoSize),
          new ColumnStyle(SizeType.Percent, 100F),
          new ColumnStyle(SizeType.Absolute, 30)
        },
        RowStyles = {
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.Percent, 100F)
        },
        Name = "byeDpiLayout"
      };
      byeDpiGroupBox.Controls.Add(byeDpiLayout);

      // Path
      Label byeDpiPathLabel = new Label {
        Text = Program.localization.GetString("settings_form.byedpi.path_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "byeDpiPathLabel",
        Margin = new Padding(0),
      };
      byeDpiLayout.Controls.Add(byeDpiPathLabel, 0, 0);

      ByeDpiPathTextBox = new TextBox {
        Name = "byeDpiPathTextBox",
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
        Margin = new Padding(0)
      };
      byeDpiLayout.Controls.Add(ByeDpiPathTextBox, 1, 0);

      Button byeDpiBrowseButton = new Button {
        Text = "...",
        Anchor = AnchorStyles.None,
        Name = "byeDpiBrowseButton",
        Margin = new Padding(0),
      };
      byeDpiBrowseButton.Click += (s, e) => BrowseForExe(
        ByeDpiPathTextBox,
        Program.localization.GetString("settings_form.byedpi.browse_title")
      );
      byeDpiLayout.Controls.Add(byeDpiBrowseButton, 2, 0);

      // Hotkey
      Label hotkeyLabel = new Label {
        Text = Program.localization.GetString("settings_form.byedpi.hotkey_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "hotkeyLabel",
        Margin = new Padding(0, 3, 0, 3),
      };
      byeDpiLayout.Controls.Add(hotkeyLabel, 0, 1);

      HotkeyTextBox = new HotkeyTextBox {
        Name = "hotkeyTextBox",
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
        Margin = new Padding(0, 3, 0, 3)
      };
      byeDpiLayout.Controls.Add(HotkeyTextBox, 1, 1);

      // Arguments
      Label byeDpiArgsLabel = new Label {
        Text = Program.localization.GetString("settings_form.byedpi.args_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "byeDpiArgsLabel",
        Margin = new Padding(0, 3, 0, 3),
      };
      byeDpiLayout.SetColumnSpan(byeDpiArgsLabel, 3);
      byeDpiLayout.Controls.Add(byeDpiArgsLabel, 0, 2);

      ByeDpiArgsTextBox = new TextBox {
        Name = "byeDpiArgsTextBox",
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
        Margin = new Padding(0, 3, 0, 3)
      };
      byeDpiLayout.SetColumnSpan(ByeDpiArgsTextBox, 3);
      byeDpiLayout.Controls.Add(ByeDpiArgsTextBox, 0, 3);
    }

    public void LoadSettings() {
      ByeDpiPathTextBox.Text = _settings.ByeDpiPath;
      HotkeyTextBox.Text = _settings.Hotkey;
      ByeDpiArgsTextBox.Text = _settings.ByeDpiArguments;
    }

    public void SaveSettings() {
      _settings.ByeDpiPath = ByeDpiPathTextBox.Text;
      _settings.Hotkey = HotkeyTextBox.Text;
      _settings.ByeDpiArguments = ByeDpiArgsTextBox.Text;
    }

    private void BrowseForExe(TextBox targetTextBox, string title) {
      using (OpenFileDialog dialog = new OpenFileDialog()) {
        dialog.Filter = "*.exe|*.exe|*.*|*.*";
        dialog.Title = title;

        if (dialog.ShowDialog() == DialogResult.OK) {
          targetTextBox.Text = dialog.FileName;
        }
      }
    }
  }
}
