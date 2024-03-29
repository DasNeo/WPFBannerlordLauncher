using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System;
using WPFBannerlordLauncher.Classes;
using WPFBannerlordLauncher.Models;
using WPFBannerlordLauncher.Controls;
using System.IO;
using System.Threading;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics.Eventing.Reader;

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
                OnPropertyChanged();
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

        private string selectedPlaylist { get; set; }
        public string SelectedPlaylist
        {
            get => selectedPlaylist;
            set
            {
                selectedPlaylist = value;
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

        private ObservableCollection<string> playlists { get; set; }
        public ObservableCollection<string> Playlists
        {
            get => playlists;
            set
            {
                playlists = value;
                playlists.CollectionChanged += (s, e) =>
                {
                    OnPropertyChanged();
                    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        var addedItem = e.NewItems?[0]?.ToString();
                        if (String.IsNullOrEmpty(addedItem))
                            throw new ArgumentException("Collection Playlists was changed with a null value (Action = add)");
                        if (ModuleLoader.Playlists.FirstOrDefault(r => r.Name == addedItem) is null)
                        {
                            var newPlaylist = new Playlist()
                            {
                                Name = addedItem,
                                Modules = new Dictionary<int, Tuple<string, bool>>()
                            };
                            for (int i = 0; i < Modules.Count; i++)
                            {
                                Module mod = Modules[i];
                                newPlaylist.Modules.Add(i, new Tuple<string, bool>(mod.ModuleModel.Id, false));
                            }
                            ModuleLoader.Playlists.Add(newPlaylist);
                        } else
                        {
                            
                        }
                    } else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    {
                        var removedItem = e.OldItems?[0]?.ToString();
                        if (String.IsNullOrEmpty(removedItem))
                            throw new ArgumentException("Collection Playlists was changed with a null value (Action = remove)");

                        var playlistItem = ModuleLoader.Playlists.FirstOrDefault(r => r.Name == removedItem);
                        if (playlistItem != null)
                            ModuleLoader.Playlists.Remove(playlistItem);
                    }
                };
                OnPropertyChanged();
            }
        }

        private bool compact { get; set; } = false;
        public bool Compact
        {
            get => compact;
            set
            {
                compact = value;

                foreach (var mod in ((MainWindow)App.Current.MainWindow).Modules)
                    mod.Compact = value;
            }
        }

        public MainWindow()
        {
            this.DataContext = this;
            Modules = new ObservableCollection<Module>();
            Playlists = new ObservableCollection<string>();
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Properties.Settings.Default.Path))
            {
                await ModuleLoader.LoadPlaylists();
                if (ModuleLoader.Playlists.Count > 0)
                {
                    foreach (var playlist in ModuleLoader.Playlists)
                        playlists.Add(playlist.Name);
                }
                var mods = await modLoader.GetModules(Path.Combine(Properties.Settings.Default.Path, "Modules"));
                await new TaskFactory().StartNew(() =>
                {
                    foreach (var mod in mods)
                    {
                        App.Current.Dispatcher.Invoke(() => Modules.Add(mod));
                    }
                });
                // TODO: enable mods, sort mods
                SortModlist();
                GenerateWarnings();
                EnableMods();
            } else
            {
                SettingsButton_Click(null, null);
            }
        }

        public void ReindexModulesInPlaylist()
        {
            foreach (var playlist in ModuleLoader.Playlists)
            {
                var names = new Dictionary<int, Tuple<string, bool>>();
                foreach (var mod in playlist.Modules)
                {
                    var modView = Modules.FirstOrDefault(r => r.ModuleModel.Id == mod.Value.Item1);
                    var index = Modules.IndexOf(modView);
                    names.Add(index, new Tuple<string, bool>(mod.Value.Item1, mod.Value.Item2));
                }
                playlist.Modules = names.OrderBy(r => r.Key).ToDictionary(r => r.Key, b => b.Value);
            }
        }

        public void EnableMods()
        {
            foreach(var mod in Modules)
            {
                mod.IsIniting = true;
                var playlist = ModuleLoader.Playlists.FirstOrDefault(r => r.Name == SelectedPlaylist);
                if (playlist != null)
                {
                    foreach(var module in playlist.Modules)
                    {
                        if (module.Value.Item1 == mod.ModuleModel.Id)
                        {
                            mod.IsChecked = module.Value.Item2;
                            break;
                        }
                    }
                }
                mod.IsIniting = false;
            }
        }

        public void GenerateWarnings()
        {
            foreach(var mod in Modules)
            {
                var noNatives = HasNoNativeDependencies(mod, out bool hasMetadataOnNative);
                if (noNatives && !hasMetadataOnNative &&
                    mod.ModuleModel.Official != "Official")
                {
                    mod.Warnings.Add("Module has no dependencies and no LoadAfterThis-Tag.\rIf this is intended leave as is.");
                }
            }
        }

        private bool HasNoNativeDependencies(Module mod, out bool hasMetadataOnNative)
        {
            bool hasDependendModuleMetadataOnNativeBeforeThis =
                    mod.ModuleModel.DependedModuleMetadatas.FirstOrDefault(
                        r => r.id == "Native"
                        && r.order == "LoadBeforeThis") != null;

            bool hasDependenModuleMetadataOnNativeAfterThis =
                mod.ModuleModel.DependedModuleMetadatas.FirstOrDefault(
                    r => r.id == "Native"
                    && r.order == "LoadAfterThis") != null;

            bool hasDependencyOnNative =
                mod.ModuleModel.DependedModules.FirstOrDefault(r => r.Id == "Native") != null;

            bool hasModuleToLoadAfterThisOnNative =
                mod.ModuleModel.ModulesToLoadAfterThis.FirstOrDefault(r => r.Id == "Native") != null;

            bool hasNoNativeDependency =
                (!hasDependencyOnNative
                && !hasDependendModuleMetadataOnNativeBeforeThis);

            hasMetadataOnNative = false;
            if (hasDependenModuleMetadataOnNativeAfterThis)
            {
                hasMetadataOnNative = true;
                hasNoNativeDependency = true;
            }

            if (hasModuleToLoadAfterThisOnNative)
            {
                hasMetadataOnNative = true;
                hasNoNativeDependency = true;
            }
            return hasNoNativeDependency;
        }

        public void SortModlist(bool basedOnDependencies = false)
        {
            if (!basedOnDependencies)
            {
                // arrange mods based on the index of playlist
                var playlist = ModuleLoader.Playlists.FirstOrDefault(r => r.Name == SelectedPlaylist);
                if (playlist != null)
                {
                    var internalModuleList = new ObservableCollection<Module>();
                    foreach (var mod in playlist.Modules)
                    {
                        var module = Modules.FirstOrDefault(r => r.ModuleModel.Id == mod.Value.Item1);
                        internalModuleList.Add(module);
                        module.IsIniting = true;
                        module.IsChecked = true;
                        module.IsIniting = false;
                    }
                    Modules = internalModuleList;
                }
            } else 
            { 
                var internalModuleList = new ObservableCollection<Module>();
                int overwrittenIndex = 0;
                foreach (var mod in Modules)
                {
                    var module = mod.ModuleModel;
                    var lastDependency = mod.Dependencies.LastOrDefault();


                    bool hasNoNativeDependency = HasNoNativeDependencies(mod, out bool _);

                    if (hasNoNativeDependency && !internalModuleList.Contains(mod))
                    {
                        internalModuleList.Insert(overwrittenIndex, mod);
                        overwrittenIndex++;
                    }
                    else
                    {
                        foreach (var depmod in mod.Dependencies)
                        {
                            var dep = Modules.FirstOrDefault(r => r.ModuleModel.Id == depmod);
                            if (dep is null)
                                continue;

                            var reversedLoadAfterList = dep.ModuleModel.ModulesToLoadAfterThis;
                            reversedLoadAfterList.Reverse();
                            foreach (var loadAfterThis in reversedLoadAfterList)
                            {
                                var loadAfterMod = Modules.FirstOrDefault(r => r.ModuleModel.Id == loadAfterThis.Id);
                                if (!internalModuleList.Contains(loadAfterMod))
                                {
                                    internalModuleList.Insert(overwrittenIndex, loadAfterMod);
                                }
                            }
                        }
                    }

                    if (!internalModuleList.Contains(mod))
                    {
                        internalModuleList.Add(mod);
                    }
                }

                // Rearrange mods before natives
                var indexOfNative = internalModuleList.IndexOf(internalModuleList.FirstOrDefault(r => r.ModuleModel.Id == "Native"));
                Dictionary<int, Module> moveList = new Dictionary<int, Module>();
                int counter = 0;
                for (int i = 0; i < indexOfNative + 1; i++)
                {
                    Module? mod = internalModuleList[i];
                    foreach (var intmod in mod.ModuleModel.DependedModules)
                    {
                        if (!moveList.ContainsValue(internalModuleList.FirstOrDefault(r => r.ModuleModel.Id == intmod.Id)))
                        {
                            moveList.Add(counter, internalModuleList.FirstOrDefault(r => r.ModuleModel.Id == intmod.Id));
                            counter++;
                        }
                        ;
                    }
                }

                foreach (var move in moveList)
                {
                    internalModuleList.Remove(move.Value);
                    internalModuleList.Insert(move.Key, move.Value);
                }

                Modules = internalModuleList;
            }
            ReindexModulesInPlaylist();
        }

        public void ResetFocus(Module excludedMod)
        {
            foreach (var mod in Modules)
                if (mod != excludedMod)
                    mod.IsFocused = false;
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

            var playlist = ModuleLoader.Playlists.FirstOrDefault(r => r.Name == SelectedPlaylist);
            if (playlist is null)
                return;

            playlist.Modules.Clear();
            foreach(var items in modulesContainer.Items)
            {
                var mod = (Module)items;
                var itemIndex = modulesContainer.Items.IndexOf(items);
                playlist.Modules.Add(itemIndex, new Tuple<string, bool>(mod.ModuleModel.Id, mod.IsChecked));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EditPlaylistName();
            dialog.Saved += (s, e) =>
            {
                Playlists.Add(dialog.Title);
            };
            DialogContent = dialog;
            IsDialogOpen = true;
            //Playlists.Add("test");
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            await ModuleLoader.SavePlaylists();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsDialog()
            {
                Path = Properties.Settings.Default.Path,
                Compact = Compact,
            };
            settings.RearrangeButtonPressed += (s, e) =>
            {
                SortModlist(true);
            };
            settings.CompactChanged += (s, e) =>
            {
                Compact = settings.Compact;
            };
            settings.Closed += (s, e) =>
            {
                Properties.Settings.Default.Path = settings.Path;
            };
            DialogContent = settings;
            IsDialogOpen = true;
        }
    }
}
