using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WPFBannerlordLauncher.Models
{
    public class ModuleModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string URL { get; set; }
        public string Version { get; set; }
        public string Official { get; set; }
        public List<DependedModuleModel> DependedModules { get; set; } = new List<DependedModuleModel>();
        public List<ModulesToLoadAfterThisModel> ModulesToLoadAfterThis { get; set; } = new List<ModulesToLoadAfterThisModel>();
        public List<DependedModuleMetadataModel> DependedModuleMetadatas { get; set; } = new List<DependedModuleMetadataModel>();
        public List<SubModuleModel> SubModules { get; set; } = new List<SubModuleModel>();
        public List<XmlNode> Xmls { get; set; } = new List<XmlNode>();
    }
}
