using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using WPFBannerlordLauncher.Controls;
using WPFBannerlordLauncher.Models;
using Module = WPFBannerlordLauncher.Controls.Module;
using XmlNode = System.Xml.XmlNode;

namespace WPFBannerlordLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<ModuleModel> Modules { get; set; } = new List<ModuleModel>();

        public MainWindow()
        {
            InitializeComponent();
            GetModules("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Mount & Blade II Bannerlord\\Modules");
        }

        // assume path is inside /modules/
        private void GetModules(string path)
        {
            if (!Directory.Exists(path))
                return;
            List<Module> mods = new List<Module>();
            Task.Factory.StartNew(() =>
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
                        mods.Add(new Module()
                        {
                            Title = model.Name,
                            Version = model.Version,
                        });
                    });
                }
            }).ContinueWith((a) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var mod in mods)
                        stkPanel.Children.Add(mod);
                });
            });
            
        }
        
        // No idea if that works. :D
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
                    } catch(Exception)
                    {
                        ;
                    }
                }

                if(!useNode)
                    list.Add(mod);
                
            }
            if (useNode)
                list.Add(mod);
            return list;
        }
    }
}
