using System;
using System.IO;
using System.Linq;
using System.Text;

namespace bdmanager {
  public class Logger {
    private readonly string _logsDir;
    private string _logFilePath;

    public event EventHandler<string> LogAdded;

    public Logger() {
      _logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

      if (!Directory.Exists(_logsDir)) {
        Directory.CreateDirectory(_logsDir);
      }

      InitLogFile();
      CleanupOldLogs();
    }

    private void InitLogFile() {
      try {
        string dateStr = DateTime.Now.ToString("dd-MM-yyyy");
        _logFilePath = Path.Combine(_logsDir, $"bdmanager_{dateStr}.log");

        if (!File.Exists(_logFilePath)) {
          File.WriteAllText(_logFilePath, $"=== bdmanager Log started at {DateTime.Now} ===\r\n", Encoding.UTF8);
        }
        else {
          File.AppendAllText(_logFilePath, $"\r\n=== bdmanager Log continued at {DateTime.Now} ===\r\n", Encoding.UTF8);
        }
      }
      catch (Exception ex) {
        Console.WriteLine($"Ошибка при инициализации файла лога: {ex.Message}");
      }
    }

    private void CleanupOldLogs() {
      try {
        if (!Directory.Exists(_logsDir)) return;

        var files = Directory.GetFiles(_logsDir, "bdmanager_*.log").OrderByDescending(File.GetCreationTime).ToList();

        if (files.Count > 10) {
          foreach (var file in files.Skip(10)) {
            try {
              File.Delete(file);
              Console.WriteLine($"Удален старый лог: {file}");
            }
            catch (Exception ex) {
              Console.WriteLine($"Ошибка удаления файла {file}: {ex.Message}");
            }
          }
        }
      }
      catch (Exception ex) {
        Console.WriteLine($"Ошибка при очистке старых логов: {ex.Message}");
      }
    }

    public void Log(string message) {
      WriteToFile(message);
      LogAdded?.Invoke(this, message);
    }

    private void WriteToFile(string message) {
      try {
        if (string.IsNullOrEmpty(_logFilePath)) {
          InitLogFile();
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string logLine = $"[{timestamp}] {message}\r\n";

        File.AppendAllText(_logFilePath, logLine, Encoding.UTF8);
      }
      catch {

      }
    }
  }
}
