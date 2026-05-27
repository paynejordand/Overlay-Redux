using System.Windows;
using System.Windows.Media;

namespace Overlay_Redux
{
    public partial class ColorPickerTest : Window
    {
        public ColorPickerTest()
        {
            InitializeComponent();
            PickerPortable.ColorChanged += OnColorChanged;
        }

        private void OnColorChanged(object sender, RoutedEventArgs e)
        {
            var color = PickerPortable.SelectedColor;
            ColorPreview.Background = new SolidColorBrush(color);
            TxtHex.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}