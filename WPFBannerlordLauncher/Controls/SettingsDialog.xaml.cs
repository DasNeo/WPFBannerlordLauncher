using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : UserControl, INotifyPropertyChanged
    {
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Path.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(SettingsDialog), new PropertyMetadata(""));

        public bool Compact
        {
            get { return (bool)GetValue(CompactProperty); }
            set 
            { 
                SetValue(CompactProperty, value); 
            }
        }

        // Using a DependencyProperty as the backing store for Compact.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CompactProperty =
            DependencyProperty.Register("Compact", typeof(bool), typeof(SettingsDialog), new PropertyMetadata(false));

        public event EventHandler? RearrangeButtonPressed;
        public event EventHandler? CompactChanged;
        public event EventHandler? Closed;
        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsDialog()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RearrangeButtonPressed?.Invoke(this, EventArgs.Empty);
        }

        private void ToggleButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            CompactChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Closed?.Invoke(sender, EventArgs.Empty);
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
