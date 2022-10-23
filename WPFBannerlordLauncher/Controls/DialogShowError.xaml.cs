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
    /// Interaction logic for DialogShowError.xaml
    /// </summary>
    public partial class DialogShowError : UserControl, INotifyPropertyChanged
    {
        private string formattedErrors { get; set; } = "";
        public string FormattedErrors
        {
            get => formattedErrors;
            set
            {
                formattedErrors = value;
                OnPropertyChanged();
            }
        }
        private string title { get; set; } = "";
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        public DialogShowError()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
