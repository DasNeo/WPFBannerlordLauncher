using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WPFBannerlordLauncher.Models
{
    [XmlRoot("SubModule")]
    public class SubModuleModel
    {
        public string Name { get; set; }
        public string DLLName { get; set; }
        public string SubModuleClassType { get; set; }
        public List<AssemblyModel> Assemblies { get; set; } = new List<AssemblyModel>();
        public List<TagModel> Tags { get; set; } = new List<TagModel>();
    }
}
