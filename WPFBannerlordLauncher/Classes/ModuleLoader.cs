using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using WPFBannerlordLauncher.Controls;
using WPFBannerlordLauncher.Models;
using Module = WPFBannerlordLauncher.Controls.Module;
using XmlNode = System.Xml.XmlNode;

namespace WPFBannerlordLauncher.Classes
{
    public class ModuleLoader
    {
        public static List<ModuleModel> Modules { get; set; } = new List<ModuleModel>();
        public static List<Playlist> Playlists { get; set; } = new List<Playlist>();

        internal static string playlistJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "playlists.json");

        public static async Task LoadPlaylists()
        {
            await new TaskFactory().StartNew(() =>
            {
                if (!File.Exists(playlistJsonPath))
                {
                    var json = JsonConvert.SerializeObject(Playlists);
                    File.WriteAllText(playlistJsonPath, json);
                }
                else
                {
                    Playlists = JsonConvert.DeserializeObject<List<Playlist>>(File.ReadAllText(playlistJsonPath)) ?? new List<Playlist>();
                }
            });
        }

        public static async Task SavePlaylists()
        {
            await new TaskFactory().StartNew(() =>
            {
                var json = JsonConvert.SerializeObject(Playlists);
                File.WriteAllText(playlistJsonPath, json);
            });
        }

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
                    using (XmlReader reader = XmlReader.Create(subModule, new XmlReaderSettings()
                    {
                        IgnoreComments = true,
                    }))
                    {

                        xml.Load(reader);

                        var id = xml.GetElementsByTagName("Id").Item(0);
                        var name = xml.GetElementsByTagName("Name").Item(0);
                        var isOfficial = xml.GetElementsByTagName("ModuleType").Item(0);
                        var version = xml.GetElementsByTagName("Version").Item(0);
                        var dependencies = xml.GetElementsByTagName("DependedModule");
                        var loadAfter = xml.GetElementsByTagName("ModulesToLoadAfterThis");
                        var subModules = xml.GetElementsByTagName("SubModule");
                        var metadata = xml.GetElementsByTagName("DependedModuleMetadata");
                        var xmls = xml.GetElementsByTagName("XmlNode");

                        if (loadAfter.Count > 0)
                        {
                            loadAfter = loadAfter.Item(0)?.ChildNodes;
                        }
                        ModuleModel model = new ModuleModel()
                        {
                            Id = id?.Attributes?[0].Value ?? "",
                            Name = name?.Attributes?[0].Value ?? "",
                            Version = version?.Attributes?[0].Value ?? "",
                            Official = isOfficial?.Attributes?[0].Value ?? "",
                        };

                        model.DependedModules = GetAttributes<DependedModuleModel>(dependencies);
                        if (loadAfter.Count > 0)
                            model.ModulesToLoadAfterThis = GetAttributes<ModulesToLoadAfterThisModel>(loadAfter);
                        model.DependedModuleMetadatas = GetAttributes<DependedModuleMetadataModel>(metadata);
                        model.SubModules = GetAttributes<SubModuleModel>(subModules);
                        model.Xmls = GetAttributes<WPFBannerlordLauncher.Models.XmlNode>(xmls);

                        Modules.Add(model);
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            var module = new Module()
                            {
                                Title = model.Name,
                                Version = model.Version,
                                Margin = new Thickness(5),
                                Errors = new List<string>()
                                {
                                "Error 1",
                                "Error 2"
                                },
                                ModuleModel = model,
                            };
                            foreach (var depMod in model.DependedModules)
                                module.Dependencies.Add(depMod.Id);

                            modules.Add(module);
                        });
                    }
                }
            });
            return modules;
        }

        private List<T> GetAttributes<T>(XmlNodeList dependencies)
        {
            List<T> list = new List<T>();

            bool useNode = false;
            XmlNodeList internalDependencies = dependencies;
            if (dependencies.Count == 1 && dependencies[0].ChildNodes.Count > 0)
            {
                //useNode = true;
                internalDependencies = dependencies[0].ChildNodes;
            }
            T mod = (T)Activator.CreateInstance(typeof(T));

            foreach (XmlNode node in internalDependencies)
            {
                if (node.Name == "#comment")
                    continue;
                if (!useNode)
                    mod = (T)Activator.CreateInstance(typeof(T));

                foreach (XmlAttribute attr in node.Attributes)
                {
                    PropertyInfo? prop = typeof(T).GetProperty(node.Name);
                    if (prop is null)
                    {
                        prop = typeof(T).GetProperty(attr.Name);
                        if(prop is null)
                            continue;
                    }
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
            {
                list.Add(mod);
            }
            return list;
        }
    }
}
