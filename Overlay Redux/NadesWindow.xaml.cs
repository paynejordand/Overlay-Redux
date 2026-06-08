using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Overlay_Redux
{
    /// <summary>
    /// Interaction logic for NadesWindow.xaml
    /// </summary>
    public partial class NadesWindow : Window
    {
        public NadesViewModel ViewModel { get; } = new();
        public NadesWindow()
        {
            DataContext = ViewModel;
            InitializeComponent();
            ApplySettings(App.Settings);
        }
        public void ApplySettings(Settings settings)
        {
            ViewModel.WindowBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.NadesBackground));
            ViewModel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.NadesBorderBrush));
            ViewModel.TextForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.NadesTextForeground));
        }
    }
}
