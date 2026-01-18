using System;
using System.Drawing;
using System.Windows.Forms;

namespace bdmanager.Views.Tabs {
  public class ProxiFyreTab : TabPage {
    private AppSettings _settings;

    public CheckBox DisableProxiFyreCheckBox { get; private set; }
    public TextBox ProxiFyrePathTextBox { get; private set; }
    public NumericUpDown ProxiFyrePortNumBox { get; private set; }
    public ListBox AppListBox { get; private set; }
    public TextBox AppTextBox { get; private set; }

    public ProxiFyreTab(AppSettings settings) {
      _settings = settings;
      InitializeTab();
    }

    private void InitializeTab() {
      Text = Program.localization.GetString("settings_form.proxifyre.tab");
      Name = "proxiFyreTabPage";
      BackColor = SystemColors.Control;
      Padding = new Padding(10);

      TableLayoutPanel proxiFyreTabLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 2,
        RowStyles = {
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.Percent, 100F)
        },
        Name = "proxiFyreTabLayout"
      };
      Controls.Add(proxiFyreTabLayout);

      // ProxiFyre Settings Group
      GroupBox proxiFyreGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.proxifyre.group"),
        Name = "proxiFyreGroupBox",
        Dock = DockStyle.Fill,
        MinimumSize = new Size(0, 110),
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10, 5, 10, 10),
        Margin = new Padding(0)
      };
      proxiFyreTabLayout.Controls.Add(proxiFyreGroupBox, 0, 0);

      TableLayoutPanel proxiFyreLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 3,
        RowCount = 3,
        ColumnStyles = {
          new ColumnStyle(SizeType.AutoSize),
          new ColumnStyle(SizeType.Percent, 100F),
          new ColumnStyle(SizeType.Absolute, 30)
        },
        RowStyles = {
          new RowStyle(SizeType.Percent, 100F),
          new RowStyle(SizeType.Percent, 100F),
          new RowStyle(SizeType.Percent, 100F)
        },
        Name = "proxiFyreLayout"
      };
      proxiFyreGroupBox.Controls.Add(proxiFyreLayout);

      Label proxiFyrePathLabel = new Label {
        Text = Program.localization.GetString("settings_form.proxifyre.path_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "proxiFyrePathLabel",
        Margin = new Padding(0),
      };
      proxiFyreLayout.Controls.Add(proxiFyrePathLabel, 0, 0);

      ProxiFyrePathTextBox = new TextBox {
        Name = "proxiFyrePathTextBox",
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
        Margin = new Padding(0)
      };
      proxiFyreLayout.Controls.Add(ProxiFyrePathTextBox, 1, 0);

      Button proxiFyreBrowseButton = new Button {
        Text = "...",
        Anchor = AnchorStyles.None,
        Name = "proxiFyreBrowseButton",
        Margin = new Padding(0),
      };
      proxiFyreBrowseButton.Click += (s, e) => BrowseForExe(
        ProxiFyrePathTextBox,
        Program.localization.GetString("settings_form.proxifyre.browse_title")
      );
      proxiFyreLayout.Controls.Add(proxiFyreBrowseButton, 2, 0);

      Label proxiFyrePortLabel = new Label {
        Text = Program.localization.GetString("settings_form.proxifyre.port_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "proxiFyrePortLabel",
        Margin = new Padding(0, 3, 0, 3),
      };
      proxiFyreLayout.Controls.Add(proxiFyrePortLabel, 0, 1);

      ProxiFyrePortNumBox = new NumericUpDown {
        Name = "proxiFyrePortNumBox",
        Minimum = 1,
        Maximum = 65535,
        Anchor = AnchorStyles.Left | AnchorStyles.None,
        Margin = new Padding(0),
        AutoSize = true
      };
      proxiFyreLayout.Controls.Add(ProxiFyrePortNumBox, 1, 1);

      DisableProxiFyreCheckBox = new CheckBox {
        Text = Program.localization.GetString("settings_form.proxifyre.disable"),
        Name = "disableProxiFyreCheckBox",
        Margin = new Padding(3, 3, 3, 0),
        Dock = DockStyle.Fill
      };
      proxiFyreLayout.SetColumnSpan(DisableProxiFyreCheckBox, 3);
      proxiFyreLayout.Controls.Add(DisableProxiFyreCheckBox, 0, 2);

      // Apps Group
      GroupBox appsGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.apps.group"),
        Name = "appsGroupBox",
        Dock = DockStyle.Fill,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10),
        Margin = new Padding(0, 5, 0, 0),
      };
      proxiFyreTabLayout.Controls.Add(appsGroupBox, 0, 1);

      TableLayoutPanel appsLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 3,
        RowCount = 4,
        ColumnStyles = {
          new ColumnStyle(SizeType.AutoSize),
          new ColumnStyle(SizeType.Percent, 100F),
          new ColumnStyle(SizeType.Absolute, 30)
        },
        RowStyles = {
          new RowStyle(SizeType.Percent, 100F),
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.AutoSize),
          new RowStyle(SizeType.AutoSize)
        },
        Name = "appsLayout"
      };
      appsGroupBox.Controls.Add(appsLayout);

      AppListBox = new ListBox {
        Name = "appListBox",
        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
        Margin = new Padding(3)
      };
      appsLayout.SetColumnSpan(AppListBox, 3);
      appsLayout.Controls.Add(AppListBox, 0, 0);

      Label appNameLabel = new Label {
        Text = Program.localization.GetString("settings_form.apps.name_label"),
        TextAlign = ContentAlignment.MiddleLeft,
        Name = "appNameLabel",
        Margin = new Padding(0, 3, 0, 3),
        AutoSize = true
      };
      appsLayout.SetColumnSpan(appNameLabel, 3);
      appsLayout.Controls.Add(appNameLabel, 0, 1);

      AppTextBox = new TextBox {
        Name = "appTextBox",
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
        Margin = new Padding(0, 3, 0, 3)
      };
      appsLayout.SetColumnSpan(AppTextBox, 2);
      appsLayout.Controls.Add(AppTextBox, 0, 2);

      Button appBrowseButton = new Button {
        Text = "...",
        Anchor = AnchorStyles.None,
        Name = "appBrowseButton",
        Margin = new Padding(0)
      };
      appBrowseButton.Click += (s, e) => {
        BrowseForExe(AppTextBox, Program.localization.GetString("settings_form.apps.browse_title"));
        if (!string.IsNullOrWhiteSpace(AppTextBox.Text)) {
          AddAppToList(AppTextBox.Text);
        }
      };
      appsLayout.Controls.Add(appBrowseButton, 2, 2);

      FlowLayoutPanel appButtonsPanel = new FlowLayoutPanel {
        FlowDirection = FlowDirection.RightToLeft,
        Dock = DockStyle.Fill,
        WrapContents = false,
        AutoSize = true,
        Name = "appButtonsPanel"
      };
      appsLayout.SetColumnSpan(appButtonsPanel, 3);
      appsLayout.Controls.Add(appButtonsPanel, 0, 3);

      Button addAppButton = new Button {
        Text = Program.localization.GetString("settings_form.apps.add"),
        Name = "addAppButton",
        Margin = new Padding(3, 3, 0, 3),
      };
      addAppButton.Click += AddAppButton_Click;
      appButtonsPanel.Controls.Add(addAppButton);

      Button removeAppButton = new Button {
        Text = Program.localization.GetString("settings_form.apps.remove"),
        Name = "removeAppButton",
        Margin = new Padding(3),
      };
      removeAppButton.Click += RemoveAppButton_Click;
      appButtonsPanel.Controls.Add(removeAppButton);
    }

    public void LoadSettings() {
      DisableProxiFyreCheckBox.Checked = _settings.DisableProxiFyre;
      ProxiFyrePathTextBox.Text = _settings.ProxiFyrePath;
      ProxiFyrePortNumBox.Value = _settings.ProxiFyrePort;

      AppListBox.Items.Clear();
      if (_settings.ProxifiedApps != null) {
        foreach (string app in _settings.ProxifiedApps) {
          AppListBox.Items.Add(app);
        }
      }
    }

    public void SaveSettings() {
      _settings.DisableProxiFyre = DisableProxiFyreCheckBox.Checked;
      _settings.ProxiFyrePath = ProxiFyrePathTextBox.Text;
      _settings.ProxiFyrePort = (int)ProxiFyrePortNumBox.Value;

      _settings.ProxifiedApps.Clear();
      foreach (string app in AppListBox.Items) {
        _settings.ProxifiedApps.Add(app);
      }
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

    private void AddAppToList(string appPath) {
      string appName = appPath.Trim();

      if (!AppListBox.Items.Contains(appName)) {
        AppListBox.Items.Add(appName);
        AppTextBox.Clear();
      }
      else {
        MessageBox.Show(
          Program.localization.GetString("settings_form.apps.already_added"),
          Program.localization.GetString("settings_form.title"),
          MessageBoxButtons.OK,
          MessageBoxIcon.Information
        );
      }
    }

    private void AddAppButton_Click(object sender, EventArgs e) {
      if (!string.IsNullOrWhiteSpace(AppTextBox.Text)) {
        AddAppToList(AppTextBox.Text);
      }
    }

    private void RemoveAppButton_Click(object sender, EventArgs e) {
      if (AppListBox.SelectedIndex >= 0) {
        AppListBox.Items.RemoveAt(AppListBox.SelectedIndex);
      }
    }
  }
}
