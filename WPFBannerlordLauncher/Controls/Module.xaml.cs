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

        public bool HasErrors => !string.IsNullOrWhiteSpace(errors);

        private string errors { get; set; }
        public string Errors
        {
            get => errors;
            set
            {
                errors = value;
                OnPropertyChanged();
                OnPropertyChanged("HasErrors");
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
    }
}
