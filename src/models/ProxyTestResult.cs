using System;

namespace bdmanager {
  public sealed class ProxyTestResult {
    public string Strategy { get; }
    public int Success { get; }
    public int Total { get; }
    public string Result => Success + "/" + Total;
    public double SuccessRate => Total > 0 ? (double)Success / Total : 0;

    public ProxyTestResult(string strategy, int success, int total) {
      Strategy = strategy;
      Success = success;
      Total = total;
    }

    public static ProxyTestResult FromText(string strategy, string result) {
      string[] parts = (result ?? string.Empty).Split('/');
      int success = 0;
      int total = 0;
      if (parts.Length == 2) {
        int.TryParse(parts[0], out success);
        int.TryParse(parts[1], out total);
      }
      return new ProxyTestResult(strategy, success, total);
    }
  }

  public sealed class ProxyTestProgress {
    public int Completed { get; }
    public int Total { get; }

    public ProxyTestProgress(int completed, int total) {
      Completed = completed;
      Total = total;
    }
  }
}
