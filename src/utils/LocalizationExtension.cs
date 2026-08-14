using System;
using System.Windows.Markup;

namespace bdmanager {
  [MarkupExtensionReturnType(typeof(string))]
  public sealed class LocExtension : MarkupExtension {
    public string Key { get; set; }

    public LocExtension() {
    }

    public LocExtension(string key) {
      Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return Program.localization?.GetString(Key) ?? Key ?? string.Empty;
    }
  }
}
