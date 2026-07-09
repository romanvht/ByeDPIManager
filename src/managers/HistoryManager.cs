using System;
using System.Collections.Generic;
using System.Linq;

namespace bdmanager {
  public static class HistoryManager {
    private const int MaxItems = 50;

    public static List<HistoryItem> Ensure(List<HistoryItem> history) {
      if (history == null) {
        return new List<HistoryItem>();
      }

      foreach (var item in history) {
        if (item.LastUsedAt == default(DateTime)) {
          item.LastUsedAt = DateTime.UtcNow;
        }

        item.Name = item.Name?.Trim();
      }

      return history;
    }

    public static List<HistoryItem> GetItems(List<HistoryItem> history) {
      history = Ensure(history);

      return history
        .Where(item => !string.IsNullOrWhiteSpace(item.Arguments))
        .OrderByDescending(item => item.IsPinned)
        .ThenByDescending(item => item.LastUsedAt)
        .ToList();
    }

    public static HistoryItem AddOrUpdate(List<HistoryItem> history, string arguments, string name = null) {
      string normalizedArguments = NormalizeArguments(arguments);
      if (string.IsNullOrWhiteSpace(normalizedArguments)) {
        return null;
      }

      history = Ensure(history);

      var existing = history.FirstOrDefault(item =>
        NormalizeArguments(item.Arguments) == normalizedArguments
      );

      if (existing != null) {
        existing.Arguments = normalizedArguments;
        existing.LastUsedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(name)) {
          existing.Name = name.Trim();
        }

        return existing;
      }

      var newItem = new HistoryItem {
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim(),
        Arguments = normalizedArguments,
        IsPinned = false,
        LastUsedAt = DateTime.UtcNow
      };

      history.Add(newItem);
      Trim(history);

      return newItem;
    }

    public static void Remove(List<HistoryItem> history, HistoryItem item) {
      if (history == null || item == null) return;

      string normalizedArguments = NormalizeArguments(item.Arguments);
      history.RemoveAll(historyItem =>
        NormalizeArguments(historyItem.Arguments) == normalizedArguments
      );
    }

    private static string NormalizeArguments(string arguments) {
      return (arguments ?? string.Empty).Trim();
    }

    private static void Trim(List<HistoryItem> history) {
      if (history.Count <= MaxItems) {
        return;
      }

      var itemsToRemove = GetItems(history)
        .Skip(MaxItems)
        .ToList();

      foreach (var item in itemsToRemove) {
        history.Remove(item);
      }
    }
  }
}
