using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace bdmanager
{
    public partial class SettingsForm : Form
    {
        private AppSettings _settings;
        private ListBox _appListBox;
        private TextBox _appTextBox;

        public SettingsForm(AppSettings settings)
        {
            _settings = settings;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.Text = "Настройки";
            this.Size = new Size(510, 490);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Load += SettingsForm_Load;
            this.BackColor = SystemColors.Control;
            this.ForeColor = SystemColors.ControlText;
            
            TabControl tabControl = new TabControl
            {
                Location = new Point(15, 15),
                Size = new Size(460, 380),
                Name = "tabControl"
            };
            this.Controls.Add(tabControl);
            
            TabPage byeDpiTabPage = new TabPage
            {
                Text = "ByeDPI",
                Name = "byeDpiTabPage",
                BackColor = SystemColors.Control
            };
            tabControl.TabPages.Add(byeDpiTabPage);
            
            GroupBox byeDpiGroupBox = new GroupBox
            {
                Text = "Настройки ByeDPI",
                Location = new Point(10, 10),
                Size = new Size(430, 330),
                ForeColor = SystemColors.ControlText,
                BackColor = SystemColors.Control,
                Name = "byeDpiGroupBox"
            };
            byeDpiTabPage.Controls.Add(byeDpiGroupBox);
            
            Label byeDpiPathLabel = new Label
            {
                Text = "Путь к ByeDPI:",
                Location = new Point(15, 25),
                Size = new Size(100, 20),
                TextAlign = ContentAlignment.MiddleLeft,
                Name = "byeDpiPathLabel"
            };
            byeDpiGroupBox.Controls.Add(byeDpiPathLabel);
            
            TextBox byeDpiPathTextBox = new TextBox
            {
                Location = new Point(120, 25),
                Size = new Size(250, 50),
                Name = "byeDpiPathTextBox"
            };
            byeDpiGroupBox.Controls.Add(byeDpiPathTextBox);
            
            Button byeDpiBrowseButton = new Button
            {
                Text = "...",
                Location = new Point(380, 24),
                Size = new Size(30, 23),
                Name = "byeDpiBrowseButton"
            };
            byeDpiBrowseButton.Click += ByeDpiBrowseButton_Click;
            byeDpiGroupBox.Controls.Add(byeDpiBrowseButton);
            
            Label byeDpiArgsLabel = new Label
            {
                Text = "Аргументы:",
                Location = new Point(15, 60),
                Size = new Size(100, 20),
                TextAlign = ContentAlignment.MiddleLeft,
                Name = "byeDpiArgsLabel"
            };
            byeDpiGroupBox.Controls.Add(byeDpiArgsLabel);
            
            TextBox byeDpiArgsTextBox = new TextBox
            {
                Location = new Point(120, 60),
                Size = new Size(290, 250),
                Name = "byeDpiArgsTextBox",
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            byeDpiGroupBox.Controls.Add(byeDpiArgsTextBox);
            
            TabPage proxiFyreTabPage = new TabPage
            {
                Text = "ProxiFyre",
                Name = "proxiFyreTabPage",
                BackColor = SystemColors.Control
            };
            tabControl.TabPages.Add(proxiFyreTabPage);
            
            GroupBox proxiFyreGroupBox = new GroupBox
            {
                Text = "Настройки ProxiFyre",
                Location = new Point(10, 10),
                Size = new Size(430, 100),
                ForeColor = SystemColors.ControlText,
                BackColor = SystemColors.Control,
                Name = "proxiFyreGroupBox"
            };
            proxiFyreTabPage.Controls.Add(proxiFyreGroupBox);
            
            Label proxiFyrePathLabel = new Label
            {
                Text = "Путь к ProxiFyre:",
                Location = new Point(15, 25),
                Size = new Size(100, 20),
                TextAlign = ContentAlignment.MiddleLeft,
                Name = "proxiFyrePathLabel"
            };
            proxiFyreGroupBox.Controls.Add(proxiFyrePathLabel);
            
            TextBox proxiFyrePathTextBox = new TextBox
            {
                Location = new Point(120, 25),
                Size = new Size(250, 23),
                Name = "proxiFyrePathTextBox"
            };
            proxiFyreGroupBox.Controls.Add(proxiFyrePathTextBox);
            
            Button proxiFyreBrowseButton = new Button
            {
                Text = "...",
                Location = new Point(380, 24),
                Size = new Size(30, 23),
                Name = "proxiFyreBrowseButton"
            };
            proxiFyreBrowseButton.Click += ProxiFyreBrowseButton_Click;
            proxiFyreGroupBox.Controls.Add(proxiFyreBrowseButton);
            
            Label proxiFyrePortLabel = new Label
            {
                Text = "Порт:",
                Location = new Point(15, 60),
                Size = new Size(100, 20),
                TextAlign = ContentAlignment.MiddleLeft,
                Name = "proxiFyrePortLabel"
            };
            proxiFyreGroupBox.Controls.Add(proxiFyrePortLabel);
            
            TextBox proxiFyrePortTextBox = new TextBox
            {
                Location = new Point(120, 60),
                Size = new Size(80, 23),
                Name = "proxiFyrePortTextBox"
            };
            proxiFyreGroupBox.Controls.Add(proxiFyrePortTextBox);
            
            GroupBox appsGroupBox = new GroupBox
            {
                Text = "Приложения, которые пойдут через прокси",
                Location = new Point(10, 120),
                Size = new Size(430, 225),
                ForeColor = SystemColors.ControlText,
                BackColor = SystemColors.Control,
                Name = "appsGroupBox"
            };
            proxiFyreTabPage.Controls.Add(appsGroupBox);
            
            ListBox appListBox = new ListBox
            {
                Location = new Point(15, 25),
                Size = new Size(400, 130),
                Name = "appListBox"
            };
            appsGroupBox.Controls.Add(appListBox);
            _appListBox = appListBox;
            
            Label appNameLabel = new Label
            {
                Text = "Имя или путь к приложению:",
                Location = new Point(15, 155),
                Size = new Size(160, 20),
                TextAlign = ContentAlignment.MiddleLeft,
                Name = "appNameLabel"
            };
            appsGroupBox.Controls.Add(appNameLabel);
            
            TextBox appTextBox = new TextBox
            {
                Location = new Point(175, 155),
                Size = new Size(200, 23),
                Name = "appTextBox"
            };
            appsGroupBox.Controls.Add(appTextBox);
            _appTextBox = appTextBox;
            
            Button appBrowseButton = new Button
            {
                Text = "...",
                Location = new Point(385, 154),
                Size = new Size(30, 23),
                Name = "appBrowseButton"
            };
            appBrowseButton.Click += AppBrowseButton_Click;
            appsGroupBox.Controls.Add(appBrowseButton);
            
            Button addAppButton = new Button
            {
                Text = "Добавить",
                Location = new Point(15, 185),
                Size = new Size(80, 25),
                Name = "addAppButton"
            };
            addAppButton.Click += AddAppButton_Click;
            appsGroupBox.Controls.Add(addAppButton);
            
            Button removeAppButton = new Button
            {
                Text = "Удалить",
                Location = new Point(100, 185),
                Size = new Size(80, 25),
                Name = "removeAppButton"
            };
            removeAppButton.Click += RemoveAppButton_Click;
            appsGroupBox.Controls.Add(removeAppButton);
            
            Button okButton = new Button
            {
                Text = "ОК",
                DialogResult = DialogResult.OK,
                Location = new Point(310, 410),
                Size = new Size(80, 30),
                Name = "okButton"
            };
            okButton.Click += OkButton_Click;
            this.Controls.Add(okButton);
            
            Button cancelButton = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new Point(400, 410),
                Size = new Size(80, 30),
                Name = "cancelButton"
            };
            this.Controls.Add(cancelButton);
            
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
            
            this.ResumeLayout(false);
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            TabControl tabControl = this.Controls["tabControl"] as TabControl;

            TabPage byeDpiTabPage = tabControl.TabPages["byeDpiTabPage"];
            GroupBox byeDpiGroupBox = byeDpiTabPage.Controls["byeDpiGroupBox"] as GroupBox;

            TextBox byeDpiPathTextBox = byeDpiGroupBox.Controls["byeDpiPathTextBox"] as TextBox;
            byeDpiPathTextBox.Text = _settings.ByeDpiPath;

            TextBox byeDpiArgsTextBox = byeDpiGroupBox.Controls["byeDpiArgsTextBox"] as TextBox;
            byeDpiArgsTextBox.Text = _settings.ByeDpiArguments;

            TabPage proxiFyreTabPage = tabControl.TabPages["proxiFyreTabPage"];
            GroupBox proxiFyreGroupBox = proxiFyreTabPage.Controls["proxiFyreGroupBox"] as GroupBox;

            TextBox proxiFyrePathTextBox = proxiFyreGroupBox.Controls["proxiFyrePathTextBox"] as TextBox;
            proxiFyrePathTextBox.Text = _settings.ProxiFyrePath;

            TextBox proxiFyrePortTextBox = proxiFyreGroupBox.Controls["proxiFyrePortTextBox"] as TextBox;
            proxiFyrePortTextBox.Text = _settings.ProxiFyrePort.ToString();

            _appListBox.Items.Clear();
            if (_settings.ProxifiedApps != null)
            {
                foreach (string app in _settings.ProxifiedApps)
                {
                    _appListBox.Items.Add(app);
                }
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            TabControl tabControl = this.Controls["tabControl"] as TabControl;

            TabPage byeDpiTabPage = tabControl.TabPages["byeDpiTabPage"];
            GroupBox byeDpiGroupBox = byeDpiTabPage.Controls["byeDpiGroupBox"] as GroupBox;

            TextBox byeDpiPathTextBox = byeDpiGroupBox.Controls["byeDpiPathTextBox"] as TextBox;
            _settings.ByeDpiPath = byeDpiPathTextBox.Text;

            TextBox byeDpiArgsTextBox = byeDpiGroupBox.Controls["byeDpiArgsTextBox"] as TextBox;
            _settings.ByeDpiArguments = byeDpiArgsTextBox.Text;

            TabPage proxiFyreTabPage = tabControl.TabPages["proxiFyreTabPage"];
            GroupBox proxiFyreGroupBox = proxiFyreTabPage.Controls["proxiFyreGroupBox"] as GroupBox;

            TextBox proxiFyrePathTextBox = proxiFyreGroupBox.Controls["proxiFyrePathTextBox"] as TextBox;
            _settings.ProxiFyrePath = proxiFyrePathTextBox.Text;
            
            if (!string.IsNullOrEmpty(_settings.ProxiFyrePath))
            {
                string proxiFyreDir = Path.GetDirectoryName(_settings.ProxiFyrePath);
                _settings.ProxiFyreConfigPath = Path.Combine(proxiFyreDir, "app-config.json");
            }

            TextBox proxiFyrePortTextBox = proxiFyreGroupBox.Controls["proxiFyrePortTextBox"] as TextBox;
            if (int.TryParse(proxiFyrePortTextBox.Text, out int port))
            {
                _settings.ProxiFyrePort = port;
            }

            _settings.ProxifiedApps.Clear();
            foreach (string app in _appListBox.Items)
            {
                _settings.ProxifiedApps.Add(app);
            }
        }

        private void ByeDpiBrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*";
                dialog.Title = "Выберите исполняемый файл ByeDPI";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    TabControl tabControl = this.Controls["tabControl"] as TabControl;
                    if (tabControl != null)
                    {
                        TabPage byeDpiTabPage = tabControl.TabPages["byeDpiTabPage"];
                        if (byeDpiTabPage != null)
                        {
                            GroupBox byeDpiGroupBox = byeDpiTabPage.Controls["byeDpiGroupBox"] as GroupBox;
                            if (byeDpiGroupBox != null)
                            {
                                TextBox byeDpiPathTextBox = byeDpiGroupBox.Controls["byeDpiPathTextBox"] as TextBox;
                                if (byeDpiPathTextBox != null)
                                {
                                    byeDpiPathTextBox.Text = dialog.FileName;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void ProxiFyreBrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*";
                dialog.Title = "Выберите исполняемый файл ProxiFyre";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    TabControl tabControl = this.Controls["tabControl"] as TabControl;
                    if (tabControl != null)
                    {
                        TabPage proxiFyreTabPage = tabControl.TabPages["proxiFyreTabPage"];
                        if (proxiFyreTabPage != null)
                        {
                            GroupBox proxiFyreGroupBox = proxiFyreTabPage.Controls["proxiFyreGroupBox"] as GroupBox;
                            if (proxiFyreGroupBox != null)
                            {
                                TextBox proxiFyrePathTextBox = proxiFyreGroupBox.Controls["proxiFyrePathTextBox"] as TextBox;
                                if (proxiFyrePathTextBox != null)
                                {
                                    proxiFyrePathTextBox.Text = dialog.FileName;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void AppBrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*";
                dialog.Title = "Выберите исполняемый файл приложения для прокси";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string fullPath = dialog.FileName;
                    
                    if (!_appListBox.Items.Contains(fullPath))
                    {
                        _appListBox.Items.Add(fullPath);
                        _appTextBox.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Это приложение уже добавлено в список.", "Предупреждение", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void AddAppButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_appTextBox.Text))
            {
                string appName = _appTextBox.Text.Trim();
                
                if (!_appListBox.Items.Contains(appName))
                {
                    _appListBox.Items.Add(appName);
                    _appTextBox.Clear();
                }
                else
                {
                    MessageBox.Show("Это приложение уже добавлено в список.", "Предупреждение", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void RemoveAppButton_Click(object sender, EventArgs e)
        {
            if (_appListBox.SelectedIndex >= 0)
            {
                _appListBox.Items.RemoveAt(_appListBox.SelectedIndex);
            }
        }
    }
} 