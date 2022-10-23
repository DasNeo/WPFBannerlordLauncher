using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
using WPFBannerlordLauncher.Classes;
using WPFBannerlordLauncher.Controls;
using WPFBannerlordLauncher.Models;
using Module = WPFBannerlordLauncher.Controls.Module;

namespace WPFBannerlordLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IDropTarget
    {
        private ModuleLoader modLoader = new ModuleLoader();

        private ObservableCollection<Module> modules { get; set; }
        public ObservableCollection<Module> Modules 
        {
            get => modules; 
            set
            {
                modules = value;
                modules.CollectionChanged += (a, b) =>
                {
                    OnPropertyChanged();
                };
            }
        }

        private bool isDialogOpen { get; set; } = false;
        public bool IsDialogOpen
        {
            get => isDialogOpen;
            set
            {
                isDialogOpen = value;
                OnPropertyChanged();
            }
        }

        private object dialogContent { get; set; }
        public object DialogContent
        {
            get => dialogContent;
            set
            {
                dialogContent = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            this.DataContext = this;
            Modules = new ObservableCollection<Module>();
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var mod in await modLoader.GetModules("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Mount & Blade II Bannerlord\\Modules"))
                Modules.Add(mod);
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = DragDropEffects.Move;
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            Module item = (Module)dropInfo.Data;
            Module target = (Module)dropInfo.TargetItem;

            int index = Modules.IndexOf(target);
            Modules.Remove(item);
            Modules.Insert(index, item);

        }
    }
}
