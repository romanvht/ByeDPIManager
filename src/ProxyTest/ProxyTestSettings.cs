using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bdmanager.src.ProxyTest {
  public class ProxyTestSettings {
    public bool UseCustomDomains { get; set; } = false;
    public string[] CustomDomains { get; set; } = Array.Empty<string>();
    public bool UseCustomCommands { get; set; } = false;
    public string[] CustomCommands { get; set; } = Array.Empty<string>();
    public int Delay { get; set; } = 0;
    public int RequestsCount { get; set; } = 1;
    public bool FullLog { get; set; } = false;
  }
}
