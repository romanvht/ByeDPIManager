using System;
using System.IO;
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
        string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
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

        var files = Directory.GetFiles(_logsDir, "bdmanager_*.log");
        var currentDate = DateTime.Now.Date;

        foreach (var file in files) {
          try {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (fileName.StartsWith("bdmanager_") && fileName.Length > 11) {
              var dateString = fileName.Substring(11);
              if (DateTime.TryParse(dateString, out var fileDate)) {
                if ((currentDate - fileDate.Date).TotalDays > 7) {
                  File.Delete(file);
                  Console.WriteLine($"Удален старый лог: {file}");
                }
              }
            }
          }
          catch {

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
        if (string.IsNullOrEmpty(_logFilePath)) return;

        string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
        if (!_logFilePath.Contains(dateStr)) {
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
