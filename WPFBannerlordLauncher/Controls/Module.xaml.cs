using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFBannerlordLauncher.Classes;
using WPFBannerlordLauncher.Models;

namespace WPFBannerlordLauncher.Controls
{
    /// <summary>
    /// Interaction logic for Module.xaml
    /// </summary>
    public partial class Module : UserControl, INotifyPropertyChanged
    {
        public bool IsIniting { get; set; } = true;

        private string title { get; set; }
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        private string version { get; set; }
        public string Version
        {
            get => version;
            set
            {
                version = value;
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
                OnPropertyChanged();
            }
        }

        private bool isChecked { get; set; } = false;
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                OnPropertyChanged();
            }
        }

        private bool isFocused { get; set; } = false;
        public new bool IsFocused 
        {
            get => isFocused;
            set
            {
                isFocused = value;
                OnPropertyChanged();
                if(!value)
                {
                    CardBackground = defaultBackground;
                } else
                {
                    CardBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777"));
                }
            }
        }

        public ModuleModel ModuleModel { get; set; }

        private SolidColorBrush cardBackground { get; set; }
        public SolidColorBrush CardBackground
        {
            get => cardBackground;
            set
            {
                cardBackground = value;
                OnPropertyChanged();
            }
        }

        private SolidColorBrush defaultBackground;

        public bool HasErrors => errors.Count > 0;
        
        private List<string> errors { get; set; } = new List<string>();
        public List<string> Errors
        {
            get => errors;
            set
            {
                errors = value;
                OnPropertyChanged();
                OnPropertyChanged("HasErrors");
                OnPropertyChanged("FormattedErrors");
            }
        }

        public bool HasWarnings => warnings?.Count > 0;

        private ObservableCollection<string> warnings { get; set; }
        public ObservableCollection<string> Warnings
        {
            get => warnings;
            set
            {
                warnings = value;
                warnings.CollectionChanged += (s, e) =>
                {
                    OnPropertyChanged();
                    OnPropertyChanged("HasWarnings");
                    OnPropertyChanged("FormattedWarnings");
                };
                OnPropertyChanged();
                OnPropertyChanged("HasWarnings");
                OnPropertyChanged("FormattedWarnings");
            }
        }

        private ObservableCollection<string> dependencies { get; set; }
        public ObservableCollection<string> Dependencies
        {
            get => dependencies;
            set
            {
                dependencies = value;
                OnPropertyChanged();
                value.CollectionChanged += (sender, e) =>
                {
                    foreach (string item in e.NewItems ?? new string[0]) {
                        Badges.Add(new Badge()
                        {
                            Title = item,
                            BadgeBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ad2e2e")),
                            Margin = new Thickness(0,0,0,10),
                        });
                    }
                };
            }
        }

        public string FormattedErrors
        {
            get
            {
                return String.Join("\r", Errors);
            }
        }

        public string FormattedWarnings
        {
            get
            {
                return String.Join("\r", Warnings);
            }
        }

        private ObservableCollection<Badge> badges { get; set; }
        public ObservableCollection<Badge> Badges
        {
            get => badges;
            set
            {
                badges = value;
                badges.CollectionChanged += (a, b) =>
                {
                    OnPropertyChanged();
                };
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Module()
        {
            this.DataContext = this;
            InitializeComponent();
            Badges = new ObservableCollection<Badge>();
            Dependencies = new ObservableCollection<string>();
            CardBackground = (SolidColorBrush)FindResource("MaterialDesignCardBackground");
            defaultBackground = CardBackground;
            Warnings = new ObservableCollection<string>();
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {            
            ((MainWindow)App.Current.MainWindow).ResetFocus(this);

            IsFocused = !IsFocused;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckToggled((CheckBox)sender);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            
            CheckToggled((CheckBox)sender);
        }

        private void CheckToggled(CheckBox cb)
        {
            if (IsIniting)
                return;
            bool isChecked = cb.IsChecked ?? false;
            MainWindow mw = (MainWindow)App.Current.MainWindow;


            if (!String.IsNullOrEmpty(mw.SelectedPlaylist))
            {
                // playlist selected
                var parent = mw.modulesContainer;
                var index = parent.Items.IndexOf(this);
                var playlist = ModuleLoader.Playlists.FirstOrDefault(r => r.Name == mw.SelectedPlaylist);
                var thisMod = playlist?.Modules
                    .FirstOrDefault(r => r.Value.Item1 == this.ModuleModel.Id);
                playlist?.Modules.Remove(thisMod.Value.Key);
                playlist?.Modules.Add(thisMod.Value.Key, new Tuple<string, bool>(this.ModuleModel.Id, isChecked));
            }
        }

        private void PackIcon2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).IsDialogOpen = true;
            ((MainWindow)App.Current.MainWindow).DialogContent = new DialogShowError()
            {
                FormattedErrors = FormattedWarnings,
                Title = $"Warnings with mod {Title} - {Version}"
            };
            e.Handled = true;
        }

        private void PackIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).IsDialogOpen = true;
            ((MainWindow)App.Current.MainWindow).DialogContent = new DialogShowError()
            {
                FormattedErrors = FormattedErrors,
                Title = $"Error with mod {Title} - {Version}"
            };
            e.Handled = true;
        }
    }
}
