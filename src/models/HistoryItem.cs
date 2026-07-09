using System;

namespace bdmanager {
  public class HistoryItem {
    public string Name { get; set; }
    public string Arguments { get; set; }
    public bool IsPinned { get; set; }
    public DateTime LastUsedAt { get; set; }

    public override string ToString() {
      if (string.IsNullOrWhiteSpace(Name)) {
        return Arguments;
      }

      return $"{Name}: {Arguments}";
    }
  }
}
