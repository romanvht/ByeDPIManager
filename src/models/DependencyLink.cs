namespace bdmanager {
  public class DependencyLink {
    public string Name { get; }
    public string DescriptionKey { get; }
    public string Url { get; }

    public DependencyLink(string name, string descriptionKey, string url) {
      Name = name;
      DescriptionKey = descriptionKey;
      Url = url;
    }
  }
}
