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
    /// Interaction logic for Badge.xaml
    /// </summary>
    public partial class Badge : UserControl, INotifyPropertyChanged
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Badge), new PropertyMetadata(string.Empty));

        public Brush BadgeBackground
        {
            get { return (Brush)GetValue(BadgeBackgroundProperty); }
            set { SetValue(BadgeBackgroundProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Background.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BadgeBackgroundProperty =
            DependencyProperty.Register("BadgeBackground", typeof(Brush), typeof(Badge), new PropertyMetadata(new SolidColorBrush(Colors.Red)));



        public Brush BadgeForeground
        {
            get { return (Brush)GetValue(BadgeForegroundProperty); }
            set { SetValue(BadgeForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BadgeForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BadgeForegroundProperty =
            DependencyProperty.Register("BadgeForeground", typeof(Brush), typeof(Badge), new PropertyMetadata(new SolidColorBrush(Colors.White)));



        /*
        private Brush background;
        public Brush BadgeBackground
        {
            get => background;
            set
            {
                background = value;
                OnPropertyChanged();
            }
        }*/

        private Brush foreground;
        public new Brush Foreground
        {
            get => foreground;
            set
            {
                foreground = value;
                OnPropertyChanged();
            }
        }

        public Badge()
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
