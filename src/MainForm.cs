using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Text;

namespace bdmanager
{
    public partial class MainForm : Form
    {
        private string appName = Program.AppName;
        private AppSettings _settings;
        private ProcessManager _processManager;
        private SettingsForm _settingsForm;
        private NotifyIcon _notifyIcon;
        private Logger _logger;

        public MainForm()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.Text = appName;
            this.Size = new Size(480, 380);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosing += MainForm_FormClosing;
            this.Load += MainForm_Load;
            this.Icon = GetIconFromResources();
            
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            
            RoundButton toggleButton = new RoundButton
            {
                Text = "Подключить",
                Size = new Size(160, 80),
                Location = new Point((this.ClientSize.Width - 160) / 2, 40),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BorderRadius = 30,
                BorderSize = 1,
                BorderColor = Color.FromArgb(100, 100, 100)
            };
            toggleButton.Click += ToggleButton_Click;
            this.Controls.Add(toggleButton);
            
            RoundButton settingsButton = new RoundButton
            {
                Text = "Настройки",
                Size = new Size(120, 40),
                Location = new Point((this.ClientSize.Width - 120) / 2, 140),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                BorderRadius = 20,
                BorderSize = 1,
                BorderColor = Color.FromArgb(100, 100, 100)
            };
            settingsButton.Click += SettingsButton_Click;
            this.Controls.Add(settingsButton);
            
            Panel logPanel = new Panel
            {
                Size = new Size(this.ClientSize.Width - 20, 120),
                Location = new Point(10, this.ClientSize.Height - 130),
                BackColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(logPanel);
            
            RichTextBox logBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(25, 25, 25),
                ForeColor = Color.LimeGreen,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(5)
            };
            logPanel.Controls.Add(logBox);
            
            _notifyIcon = new NotifyIcon
            {
                Icon = GetIconFromResources(),
                Text = appName,
                Visible = true
            };
            
            ContextMenu trayMenu = new ContextMenu();
            MenuItem openMenuItem = new MenuItem("Открыть");
            openMenuItem.Click += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };
            
            MenuItem exitMenuItem = new MenuItem("Выход");
            exitMenuItem.Click += (s, e) => { 
                ShutdownProcesses();
                _notifyIcon.Visible = false;
                Application.Exit(); 
            };
            
            trayMenu.MenuItems.Add(openMenuItem);
            trayMenu.MenuItems.Add(exitMenuItem);
            
            _notifyIcon.ContextMenu = trayMenu;
            _notifyIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };
            
            this.ResumeLayout(false);
        }

        private void InitializeApplication()
        {
            _settings = AppSettings.Load();
            
            _processManager = new ProcessManager(_settings);
            
            string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _logger = new Logger(appDir);
            _logger.LogAdded += (s, message) => AddLogToUi(message);
            
            _processManager.LogMessage += (s, message) => _logger.Log(message);
            _processManager.StatusChanged += (s, isRunning) => UpdateStatus(isRunning);
            
            _settingsForm = new SettingsForm(_settings);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _logger.Log("Приложение запущено");
            _processManager.CleanupOnStartup();
            
            UpdateStatus(_processManager.IsRunning);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
                _notifyIcon.ShowBalloonTip(3000, appName, "Приложение свернуто в трей", ToolTipIcon.Info);
            }
            else
            {
                ShutdownProcesses();
            }
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            ToggleConnection();
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            OpenSettings();
        }

        private void ToggleConnection()
        {
            RoundButton toggleButton = null;
            foreach (Control control in this.Controls)
            {
                if (control is RoundButton button && (button.Text == "Подключить" || button.Text == "Отключить"))
                {
                    toggleButton = button;
                    break;
                }
            }
            
            if (toggleButton == null) return;
            
            toggleButton.Enabled = false;
            
            if (_processManager.IsRunning)
            {
                _processManager.Stop();
            }
            else
            {
                _processManager.Start();
            }
            
            toggleButton.Enabled = true;
        }

        private void OpenSettings()
        {
            _settingsForm = new SettingsForm(_settings);
            
            if (_settingsForm.ShowDialog() == DialogResult.OK)
            {
                _settings.Save();
                AddLog("Настройки сохранены");
            }
        }

        private void AddLog(string message)
        {
            _logger.Log(message);
        }
        
        private void AddLogToUi(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(UpdateLogInUi), message);
                return;
            }
            
            UpdateLogInUi(message);
        }
        
        private void UpdateLogInUi(string message)
        {
            Panel logPanel = null;
            foreach (Control control in this.Controls)
            {
                if (control is Panel panel && panel.Controls.Count > 0 && panel.Controls[0] is RichTextBox)
                {
                    logPanel = panel;
                    break;
                }
            }
            
            if (logPanel == null || logPanel.Controls.Count == 0) return;
            
            RichTextBox logBox = (RichTextBox)logPanel.Controls[0];
            
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logLine = $"[{timestamp}] {message}\n";
            
            string[] lines = logBox.Text.Split('\n');
            if (lines.Length > 500)
            {
                bool wasAtBottom = logBox.SelectionStart >= logBox.Text.Length - 5;
                
                StringBuilder newText = new StringBuilder();
                for (int i = lines.Length - 500; i < lines.Length; i++)
                {
                    if (i >= 0 && i < lines.Length && !string.IsNullOrEmpty(lines[i]))
                    {
                        newText.AppendLine(lines[i]);
                    }
                }
                
                logBox.Text = newText.ToString();
                
                if (wasAtBottom)
                {
                    logBox.SelectionStart = logBox.Text.Length;
                    logBox.ScrollToCaret();
                }
            }
            
            logBox.AppendText(logLine);
            logBox.ScrollToCaret();
        }

        private void UpdateStatus(bool isRunning)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateStatus(isRunning)));
                return;
            }
            
            RoundButton toggleButton = null;
            foreach (Control control in this.Controls)
            {
                if (control is RoundButton button && (button.Text == "Подключить" || button.Text == "Отключить"))
                {
                    toggleButton = button;
                    break;
                }
            }
            
            if (toggleButton == null) return;
            
            toggleButton.Text = isRunning ? "Отключить" : "Подключить";
            
            if (isRunning)
            {
                toggleButton.BackColor = Color.FromArgb(200, 50, 50);
                toggleButton.HoverColor = Color.FromArgb(220, 60, 60);
                toggleButton.PressedColor = Color.FromArgb(180, 40, 40);
                toggleButton.BorderColor = Color.FromArgb(240, 70, 70);
            }
            else
            {
                toggleButton.BackColor = Color.FromArgb(40, 120, 50);
                toggleButton.HoverColor = Color.FromArgb(50, 140, 60);
                toggleButton.PressedColor = Color.FromArgb(30, 100, 40);
                toggleButton.BorderColor = Color.FromArgb(60, 160, 70);
            }
            
            toggleButton.ForeColor = Color.White;
            
            _notifyIcon.Text = $"bdmanager: {(isRunning ? "Подключено" : "Отключено")}";
        }

        private Icon GetIconFromResources()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "bdmanager.icon.ico";
            
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    return new Icon(stream);
                }
            }
            
            return SystemIcons.Application;
        }

        private void ShutdownProcesses()
        {
            if (_processManager != null && _processManager.IsRunning)
            {
                AddLog("Останавливаем процессы");
                _processManager.Stop();
            }
        }
    }
} 