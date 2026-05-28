using System.Windows;
using System.Windows.Media;

namespace Overlay_Redux
{
    public partial class SettingsWindow : Window
    {
        private bool _isLoading = true;
        private bool _saved = false;
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            _isLoading = true;
            var s = App.Settings;

            ChkMedsActive.IsChecked = s.MedsWindowActive;
            ChkRespawnActive.IsChecked = s.RespawnWindowActive;
            TxtNucleusHash.Text = s.NucleusHash;

            PickerMedsBackground.SelectedColor = (Color)ColorConverter.ConvertFromString(s.MedsBackground);
            PickerMedsBorder.SelectedColor = (Color)ColorConverter.ConvertFromString(s.MedsBorderBrush);
            PickerMedsText.SelectedColor = (Color)ColorConverter.ConvertFromString(s.MedsTextForeground);
            PickerRespawnBackground.SelectedColor = (Color)ColorConverter.ConvertFromString(s.RespawnBackground);
            PickerRespawnText.SelectedColor = (Color)ColorConverter.ConvertFromString(s.RespawnTextForeground);

            //UpdateRespawnPreview();
            _isLoading = false;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var s = App.Settings;

            s.MedsWindowActive = ChkMedsActive.IsChecked ?? true;
            s.RespawnWindowActive = ChkRespawnActive.IsChecked ?? true;
            s.NucleusHash = TxtNucleusHash.Text;

            s.MedsBackground = ToHex(PickerMedsBackground.SelectedColor);
            s.MedsBorderBrush = ToHex(PickerMedsBorder.SelectedColor);
            s.MedsTextForeground = ToHex(PickerMedsText.SelectedColor);
            s.RespawnBackground = ToHex(PickerRespawnBackground.SelectedColor);
            s.RespawnTextForeground = ToHex(PickerRespawnText.SelectedColor);

            App.SettingsService.Save(s);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?._medsWindow?.ApplySettings(s);
            mainWindow?._respawnWindow?.ApplySettings(s);
            
            _saved = true;
            Close();
        }
        private void PickerMeds_ColorChanged(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;

            var temp = new Settings
            {
                MedsBackground = ToHex(PickerMedsBackground.SelectedColor),
                MedsBorderBrush = ToHex(PickerMedsBorder.SelectedColor),
                MedsTextForeground = ToHex(PickerMedsText.SelectedColor),
            };

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?._medsWindow?.ApplySettings(temp);
        }

        private void PickerRespawn_ColorChanged(object sender, RoutedEventArgs e)
        {
            TxtRespawnPreview.Background = new SolidColorBrush(PickerRespawnBackground.SelectedColor);
            TxtRespawnPreview.Foreground = new SolidColorBrush(PickerRespawnText.SelectedColor);
        }

        private void BtnClearHash_Click(object sender, RoutedEventArgs e)
        {
            TxtNucleusHash.Text = null;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_saved)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?._medsWindow?.ApplySettings(App.Settings);
            }
            base.OnClosing(e);
        }

        private static string ToHex(Color color) =>
            $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}