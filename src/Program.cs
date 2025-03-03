using System;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace bdmanager
{
    static class Program
    {
        private static Mutex _mutex = null;
        private static MainForm _mainForm = null;

        public static string AppName => System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        [STAThread]
        static void Main()
        {
            bool createdNew;
            
            _mutex = new Mutex(true, AppName, out createdNew);
            
            if (!createdNew)
            {
                MessageBox.Show("Приложение уже запущено.", AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                _mainForm = new MainForm();
                Application.Run(_mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (_mutex != null)
                {
                    _mutex.ReleaseMutex();
                    _mutex.Dispose();
                }
            }
        }
    }
} 