using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using WPFBannerlordLauncher.Controls;
using WPFBannerlordLauncher.Models;
using Module = WPFBannerlordLauncher.Controls.Module;
using XmlNode = System.Xml.XmlNode;

namespace WPFBannerlordLauncher.Classes
{
    public class ModuleLoader
    {
        public List<ModuleModel> Modules { get; set; } = new List<ModuleModel>();

        // assume path is inside /modules/
        public async Task<List<Module>> GetModules(string path)
        {
            List<Module> modules = new List<Module>();
            if (!Directory.Exists(path))
                return modules;

            await Task.Factory.StartNew(() =>
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    string subModule = "";
                    foreach (var file in Directory.GetFiles(dir))
                        if (file.ToLower().Contains("submodule.xml")) // file is the fullpath. :D
                        {
                            subModule = file;
                            break;
                        }

                    if (String.IsNullOrEmpty(subModule))
                        continue;

                    XmlDocument xml = new XmlDocument();
                    xml.Load(subModule);

                    var id = xml.GetElementsByTagName("Id").Item(0);
                    var name = xml.GetElementsByTagName("Name").Item(0);
                    var version = xml.GetElementsByTagName("Version").Item(0);
                    var dependencies = xml.GetElementsByTagName("DependedModule");
                    var subModules = xml.GetElementsByTagName("SubModule");
                    var metadata = xml.GetElementsByTagName("DependedModuleMetadata");
                    var xmls = xml.GetElementsByTagName("XmlNode");

                    ModuleModel model = new ModuleModel()
                    {
                        Id = id?.Attributes?[0].Value ?? "",
                        Name = name?.Attributes?[0].Value ?? "",
                        Version = version?.Attributes?[0].Value ?? "",
                    };

                    model.DependedModules = GetAttributes<DependedModuleModel>(dependencies);
                    model.DependedModuleMetadatas = GetAttributes<DependedModuleMetadataModel>(metadata);
                    model.SubModules = GetAttributes<SubModuleModel>(subModules);
                    model.Xmls = GetAttributes<WPFBannerlordLauncher.Models.XmlNode>(xmls);

                    Modules.Add(model);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        modules.Add(new Module()
                        {
                            Title = model.Name,
                            Version = model.Version,
                            Margin = new Thickness(5),
                            Errors = new List<string>()
                            {
                                "Error 1",
                                "Error 2"
                            }
                        });
                    });
                }
            });
            return modules;
        }

        private List<T> GetAttributes<T>(XmlNodeList dependencies)
        {
            List<T> list = new List<T>();

            bool useNode = false;
            if (dependencies.Count == 1 && dependencies[0].ChildNodes.Count >= 0)
            {
                useNode = true;
                dependencies = dependencies[0].ChildNodes;
            }
            T mod = (T)Activator.CreateInstance(typeof(T));
            foreach (XmlNode node in dependencies)
            {
                if (node.Name == "#comment")
                    continue;
                if (!useNode)
                    mod = (T)Activator.CreateInstance(typeof(T));

                foreach (XmlAttribute attr in node.Attributes)
                {
                    PropertyInfo? prop = typeof(T).GetProperty((useNode ? node.Name : attr.Name));
                    if (prop is null)
                        continue;
                    try
                    {
                        // prop is XmlNameModel XmlName
                        prop.SetValue(mod, attr.Value);
                    }
                    catch (Exception)
                    {
                        ;
                    }
                }

                if (!useNode)
                    list.Add(mod);

            }
            if (useNode)
                list.Add(mod);
            return list;
        }
    }
}
