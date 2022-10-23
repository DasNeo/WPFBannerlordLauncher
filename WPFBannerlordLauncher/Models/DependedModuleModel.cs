using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFBannerlordLauncher.Models
{
    public class DependedModuleModel
    {
        public string Id { get; set; }
        public string DependentVersion { get; set; }
        public string Optional { get; set; }
    }
}
