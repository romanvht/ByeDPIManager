namespace bdmanager {
  public class SystemProxyBackup {
    public bool ProxyEnableExists { get; set; }
    public int ProxyEnable { get; set; }

    public bool ProxyServerExists { get; set; }
    public string ProxyServer { get; set; }

    public bool ProxyOverrideExists { get; set; }
    public string ProxyOverride { get; set; }

    public bool AutoConfigUrlExists { get; set; }
    public string AutoConfigUrl { get; set; }

    public string AppliedProxyServer { get; set; }
    public string AppliedProxyOverride { get; set; }
  }
}
