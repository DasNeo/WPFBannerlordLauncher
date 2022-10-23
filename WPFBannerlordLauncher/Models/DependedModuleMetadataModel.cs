using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFBannerlordLauncher.Models
{
    public class DependedModuleMetadataModel
    {
        public string id { get; set; }
        public string order { get; set; }
        public string optional { get; set; }
        public string version { get; set; }
    }
}
