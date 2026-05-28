using System.Windows;
using System.Windows.Media;

namespace Overlay_Redux
{
    public partial class MedsWindow : Window
    {
        public MedsViewModel ViewModel { get; } = new();

        public MedsWindow()
        {
            DataContext = ViewModel;
            InitializeComponent();
            ApplySettings(App.Settings);
        }

        public void ApplySettings(Settings settings)
        {
            ViewModel.WindowBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.MedsBackground));
            ViewModel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.MedsBorderBrush));
            ViewModel.TextForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.MedsTextForeground));
        }
    }
}