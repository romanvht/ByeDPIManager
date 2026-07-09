using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace bdmanager.Views.Tabs {
  public class ByeDpiTab : TabPage {
    private AppSettings _settings;

    public TextBox ByeDpiPathTextBox { get; private set; }
    public HotkeyTextBox HotkeyTextBox { get; private set; }
    public TextBox ByeDpiArgsTextBox { get; private set; }
    public DataGridView HistoryGridView { get; private set; }

    public ByeDpiTab(AppSettings settings) {
      _settings = settings;
      InitializeTab();
    }

    private void InitializeTab() {
      Text = Program.localization.GetString("settings_form.byedpi.tab");
      Name = "byeDpiTabPage";
      BackColor = SystemColors.Control;
      Padding = new Padding(10);

      TableLayoutPanel rootLayout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 2,
        ColumnStyles = {
          new ColumnStyle(SizeType.Percent, 100F)
        },
        RowStyles = {
          new RowStyle(SizeType.Percent, 45F),
          new RowStyle(SizeType.Percent, 55F)
        },
        Name = "byeDpiRootLayout"
      };
      Controls.Add(rootLayout);

      GroupBox byeDpiGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.byedpi.group"),
        Name = "byeDpiGroupBox",
        Dock = DockStyle.Fill,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10, 5, 10, 10),
        Margin = new Padding(0, 0, 0, 5)
      };
      rootLayout.Controls.Add(byeDpiGroupBox, 0, 0);

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

      // History
      GroupBox historyGroupBox = new GroupBox {
        Text = Program.localization.GetString("settings_form.byedpi.history_group"),
        Name = "historyGroupBox",
        Dock = DockStyle.Fill,
        ForeColor = SystemColors.ControlText,
        BackColor = SystemColors.Control,
        Padding = new Padding(10, 5, 10, 10),
        Margin = new Padding(0, 5, 0, 0)
      };
      rootLayout.Controls.Add(historyGroupBox, 0, 1);

      HistoryGridView = new DataGridView {
        Dock = DockStyle.Fill,
        Name = "historyGridView",
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        ReadOnly = true,
        RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
        BackgroundColor = SystemColors.Window,
        BorderStyle = BorderStyle.Fixed3D
      };
      HistoryGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
      HistoryGridView.DefaultCellStyle.Padding = new Padding(5);
      HistoryGridView.CellDoubleClick += HistoryGridView_CellDoubleClick;
      HistoryGridView.MouseDown += HistoryGridView_MouseDown;
      historyGroupBox.Controls.Add(HistoryGridView);

      HistoryGridView.Columns.Add(new DataGridViewTextBoxColumn {
        Name = "Marker",
        HeaderText = "",
        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
        Width = 35,
        DefaultCellStyle = new DataGridViewCellStyle {
          Alignment = DataGridViewContentAlignment.MiddleCenter,
          Font = new Font("Segoe UI Symbol", 9F)
        }
      });
      HistoryGridView.Columns.Add(new DataGridViewTextBoxColumn {
        Name = "Strategy",
        HeaderText = Program.localization.GetString("settings_form.byedpi.history_strategy_column"),
        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True }
      });

      ContextMenuStrip historyContextMenu = new ContextMenuStrip();
      historyContextMenu.Items.Add(CreateHistoryMenuItem("settings_form.byedpi.history_apply", ApplySelectedHistoryItem_Click));
      historyContextMenu.Items.Add(CreateHistoryMenuItem("settings_form.byedpi.history_pin", TogglePinSelectedHistoryItem_Click));
      historyContextMenu.Items.Add(CreateHistoryMenuItem("settings_form.byedpi.history_rename", RenameSelectedHistoryItem_Click));
      historyContextMenu.Items.Add(CreateHistoryMenuItem("settings_form.byedpi.history_delete", DeleteSelectedHistoryItem_Click));
      HistoryGridView.ContextMenuStrip = historyContextMenu;
    }

    private string GetHistoryDisplayText(HistoryItem item) {
      if (string.IsNullOrWhiteSpace(item.Name)) {
        return item.Arguments;
      }

      return $"{item.Name}:{Environment.NewLine}{item.Arguments}";
    }

    private string GetHistoryMarker(HistoryItem item, string currentArguments) {
      if (string.Equals(item.Arguments.Trim(), currentArguments, StringComparison.Ordinal)) {
        return "\u2713";
      }

      if (item.IsPinned) {
        return "\u2605";
      }

      return "";
    }

    private ToolStripMenuItem CreateHistoryMenuItem(string localizationKey, EventHandler handler) {
      ToolStripMenuItem item = new ToolStripMenuItem {
        Text = Program.localization.GetString(localizationKey)
      };
      item.Click += handler;

      return item;
    }

    public void LoadSettings() {
      ByeDpiPathTextBox.Text = _settings.ByeDpiPath;
      HotkeyTextBox.Text = _settings.Hotkey;
      ByeDpiArgsTextBox.Text = _settings.ByeDpiArguments;
      _settings.ByeDpiHistory = HistoryManager.Ensure(_settings.ByeDpiHistory);
      HistoryManager.AddOrUpdate(_settings.ByeDpiHistory, _settings.ByeDpiArguments);
      RefreshStrategyControls();
    }

    public void SaveSettings() {
      _settings.ByeDpiPath = ByeDpiPathTextBox.Text;
      _settings.Hotkey = HotkeyTextBox.Text;
      _settings.ByeDpiArguments = ByeDpiArgsTextBox.Text;
      _settings.ByeDpiHistory = HistoryManager.Ensure(_settings.ByeDpiHistory);
      HistoryManager.AddOrUpdate(_settings.ByeDpiHistory, _settings.ByeDpiArguments);
      RefreshStrategyControls();
    }

    public void UpdateArguments(string arguments) {
      if (ByeDpiArgsTextBox != null && !ByeDpiArgsTextBox.IsDisposed) {
        if (ByeDpiArgsTextBox.InvokeRequired) {
          try {
            ByeDpiArgsTextBox.Invoke(new Action(() => {
              ByeDpiArgsTextBox.Text = arguments;
              RefreshStrategyControls();
            }));
          }
          catch { }
        }
        else {
          ByeDpiArgsTextBox.Text = arguments;
          RefreshStrategyControls();
        }
      }
    }

    public void RefreshStrategyControls() {
      if (HistoryGridView == null) return;

      var history = HistoryManager.GetItems(_settings.ByeDpiHistory);
      string currentArguments = (ByeDpiArgsTextBox?.Text ?? string.Empty).Trim();

      HistoryGridView.Rows.Clear();

      foreach (var item in history) {
        int rowIndex = HistoryGridView.Rows.Add(
          GetHistoryMarker(item, currentArguments),
          GetHistoryDisplayText(item)
        );
        HistoryGridView.Rows[rowIndex].Tag = item;

        if (string.Equals(item.Arguments.Trim(), currentArguments, StringComparison.Ordinal)) {
          HistoryGridView.Rows[rowIndex].Selected = true;
        }
      }
    }

    private void HistoryGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
      if (e.RowIndex >= 0) {
        ApplySelectedHistoryItem_Click(sender, e);
      }
    }

    private void HistoryGridView_MouseDown(object sender, MouseEventArgs e) {
      if (e.Button != MouseButtons.Right) return;

      var hit = HistoryGridView.HitTest(e.X, e.Y);
      if (hit.RowIndex >= 0) {
        HistoryGridView.ClearSelection();
        HistoryGridView.Rows[hit.RowIndex].Selected = true;
      }
    }

    private HistoryItem GetSelectedHistoryItem() {
      if (HistoryGridView.SelectedRows.Count == 0) return null;

      return HistoryGridView.SelectedRows[0].Tag as HistoryItem;
    }

    private void ApplySelectedHistoryItem_Click(object sender, EventArgs e) {
      var item = GetSelectedHistoryItem();
      if (item == null) return;

      ByeDpiArgsTextBox.Text = item.Arguments;
      _settings.ByeDpiArguments = item.Arguments;
      HistoryManager.AddOrUpdate(_settings.ByeDpiHistory, item.Arguments, item.Name);
      _settings.Save();
      RefreshStrategyControls();
    }

    private void TogglePinSelectedHistoryItem_Click(object sender, EventArgs e) {
      var item = GetSelectedHistoryItem();
      if (item == null) return;

      item.IsPinned = !item.IsPinned;
      _settings.Save();
      RefreshStrategyControls();
    }

    private void RenameSelectedHistoryItem_Click(object sender, EventArgs e) {
      var item = GetSelectedHistoryItem();
      if (item == null) return;

      string name = PromptForName(item.Name);
      if (string.IsNullOrWhiteSpace(name)) return;

      item.Name = name.Trim();
      _settings.Save();
      RefreshStrategyControls();
    }

    private void DeleteSelectedHistoryItem_Click(object sender, EventArgs e) {
      var item = GetSelectedHistoryItem();
      if (item == null) return;

      HistoryManager.Remove(_settings.ByeDpiHistory, item);
      _settings.Save();
      RefreshStrategyControls();
    }

    private string PromptForName(string currentName) {
      using (Form prompt = new Form()) {
        prompt.Text = Program.localization.GetString("settings_form.byedpi.rename_title");
        prompt.StartPosition = FormStartPosition.CenterParent;
        prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
        prompt.MinimizeBox = false;
        prompt.MaximizeBox = false;
        prompt.ClientSize = new Size(420, 110);
        prompt.Padding = new Padding(10);

        TextBox nameTextBox = new TextBox {
          Text = currentName,
          Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
          Location = new Point(10, 10),
          Width = 400
        };
        prompt.Controls.Add(nameTextBox);

        FlowLayoutPanel buttonsPanel = new FlowLayoutPanel {
          FlowDirection = FlowDirection.RightToLeft,
          Dock = DockStyle.Bottom,
          AutoSize = true
        };
        prompt.Controls.Add(buttonsPanel);

        Button okButton = new Button {
          Text = Program.localization.GetString("settings_form.buttons.ok"),
          DialogResult = DialogResult.OK,
          AutoSize = true
        };
        buttonsPanel.Controls.Add(okButton);

        Button cancelButton = new Button {
          Text = Program.localization.GetString("settings_form.buttons.cancel"),
          DialogResult = DialogResult.Cancel,
          AutoSize = true
        };
        buttonsPanel.Controls.Add(cancelButton);

        prompt.AcceptButton = okButton;
        prompt.CancelButton = cancelButton;

        return prompt.ShowDialog(this) == DialogResult.OK ? nameTextBox.Text : null;
      }
    }

    private bool BrowseForExe(TextBox targetTextBox, string title) {
      using (OpenFileDialog dialog = new OpenFileDialog()) {
        dialog.Filter = "*.exe|*.exe|*.*|*.*";
        dialog.Title = title;

        if (dialog.ShowDialog() == DialogResult.OK) {
          if (IsCurrentApplication(dialog.FileName)) {
            MessageBox.Show(
              Program.localization.GetString("settings_form.path_self_error"),
              Program.localization.GetString("settings_form.title"),
              MessageBoxButtons.OK,
              MessageBoxIcon.Error
            );
            return false;
          }

          targetTextBox.Text = dialog.FileName;
          return true;
        }
      }

      return false;
    }

    private bool IsCurrentApplication(string path) {
      try {
        return string.Equals(
          Path.GetFullPath(path),
          Path.GetFullPath(Application.ExecutablePath),
          StringComparison.OrdinalIgnoreCase
        );
      }
      catch {
        return false;
      }
    }
  }
}
