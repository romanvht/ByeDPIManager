using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace bdmanager {
  public static class FormatUtils {
    public static string[] ReadLines(string filePath) {
      if (!File.Exists(filePath)) {
        throw new FileNotFoundException($"File not found: {filePath}");
      }

      return File.ReadAllLines(filePath)
        .Select(line => RemoveComments(line))
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .ToArray();
    }

    public static string RemoveComments(string line) {
      if (string.IsNullOrEmpty(line)) {
        return string.Empty;
      }

      int hashIndex = line.IndexOf('#');
      int slashIndex = line.IndexOf("//");

      int commentIndex = -1;
      if (hashIndex >= 0 && slashIndex >= 0) {
        commentIndex = Math.Min(hashIndex, slashIndex);
      } else if (hashIndex >= 0) {
        commentIndex = hashIndex;
      } else if (slashIndex >= 0) {
        commentIndex = slashIndex;
      }

      if (commentIndex >= 0) {
        line = line.Substring(0, commentIndex);
      }

      return line.Trim();
    }
  }
}
