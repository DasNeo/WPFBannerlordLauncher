using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

namespace WPFBannerlordLauncher.Controls
{
    /// <summary>
    /// Interaction logic for Module.xaml
    /// </summary>
    public partial class Module : UserControl, INotifyPropertyChanged
    {
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

        public string FormattedErrors
        {
            get
            {
                return String.Join("\r", Errors);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Module()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        private void PackIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).IsDialogOpen = true;
            ((MainWindow)App.Current.MainWindow).DialogContent = new DialogShowError()
            {
                FormattedErrors = FormattedErrors,
                Title = $"Error with mod {Title} - {Version}"
            };
        }
    }
}
