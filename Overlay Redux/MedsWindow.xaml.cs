using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Overlay_Redux
{
    public partial class MedsWindow : Window
    {
        public MedsViewModel ViewModel { get; } = new();

        public MedsWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ApplySettings(App.Settings);
        }

        public void ApplySettings(Settings settings)
        {
            var converter = new BrushConverter();

            Background = (Brush)converter.ConvertFromString(settings.MedsBackground)!;

            var borderStyle = new Style(typeof(Border));
            borderStyle.Setters.Add(new Setter(Border.BorderBrushProperty, (Brush)converter.ConvertFromString(settings.MedsBorderBrush)!));
            borderStyle.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(1.25)));
            Resources["MedCountBorder"] = borderStyle;

            var textStyle = new Style(typeof(TextBlock));
            textStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, (Brush)converter.ConvertFromString(settings.MedsTextForeground)!));
            textStyle.Setters.Add(new Setter(TextBlock.FontSizeProperty, 24.0));
            textStyle.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
            textStyle.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(2)));
            Resources["MedCountText"] = textStyle;
        }
    }
}