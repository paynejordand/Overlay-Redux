using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace Overlay_Redux
{
    public partial class SettingsWindow : Window
    {
        private bool _isLoading = true;
        private bool _saved = false;
        public SettingsViewModel ViewModel { get; } = new();
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.NucleusHash = App.Settings.NucleusHash;

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow!._wss.PlayersGot += HandlePlayersGot;

            LoadSettings();
        }

        private void LoadSettings()
        {
            _isLoading = true;
            var s = App.Settings;

            ViewModel.MedsWindowActive = s.MedsWindowActive;
            ViewModel.NadesWindowActive = s.NadesWindowActive;
            ViewModel.RespawnWindowActive = s.RespawnWindowActive;

            PickerMedsBackground.SelectedColor = (Color)ColorConverter.ConvertFromString(s.MedsBackground);
            PickerMedsBorder.SelectedColor = (Color)ColorConverter.ConvertFromString(s.MedsBorderBrush);
            PickerMedsText.SelectedColor = (Color)ColorConverter.ConvertFromString(s.MedsTextForeground);
            PickerNadesBackground.SelectedColor = (Color)ColorConverter.ConvertFromString(s.NadesBackground);
            PickerNadesBorder.SelectedColor = (Color)ColorConverter.ConvertFromString(s.NadesBorderBrush);
            PickerNadesText.SelectedColor = (Color)ColorConverter.ConvertFromString(s.NadesTextForeground);
            PickerRespawnBackground.SelectedColor = (Color)ColorConverter.ConvertFromString(s.RespawnBackground);
            PickerRespawnText.SelectedColor = (Color)ColorConverter.ConvertFromString(s.RespawnTextForeground);

            _isLoading = false;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var s = App.Settings;

            s.MedsWindowActive = ViewModel.MedsWindowActive;
            s.NadesWindowActive = ViewModel.NadesWindowActive;
            s.RespawnWindowActive = ViewModel.RespawnWindowActive;

            s.MedsBackground = ToHex(PickerMedsBackground.SelectedColor);
            s.MedsBorderBrush = ToHex(PickerMedsBorder.SelectedColor);
            s.MedsTextForeground = ToHex(PickerMedsText.SelectedColor);
            s.NadesBackground = ToHex(PickerNadesBackground.SelectedColor);
            s.NadesBorderBrush = ToHex(PickerNadesBorder.SelectedColor);
            s.NadesTextForeground = ToHex(PickerNadesText.SelectedColor);
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

        private void PickerNades_ColorChanged(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;
            var temp = new Settings
            {
                NadesBackground = ToHex(PickerNadesBackground.SelectedColor),
                NadesBorderBrush = ToHex(PickerNadesBorder.SelectedColor),
                NadesTextForeground = ToHex(PickerNadesText.SelectedColor),
            };
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?._nadesWindow?.ApplySettings(temp);
        }

        private void PickerRespawn_ColorChanged(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;

            var temp = new Settings
            {
                RespawnBackground = ToHex(PickerRespawnBackground.SelectedColor),
                RespawnTextForeground = ToHex(PickerRespawnText.SelectedColor),
            };
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?._respawnWindow?.ApplySettings(temp);
        }

        private void HandlePlayersGot(JsonElement players)
        {
            Dispatcher.Invoke(() =>
            {
                if (players.GetArrayLength() != 1)
                {
                    ViewModel.VerificationFailureReason = "must be alone in a custom match lobby";
                    ViewModel.VerificationStatus = SettingsViewModel.VerificationState.Failed;
                    ViewModel.CandidateName = null;
                    ViewModel.CandidateHash = null;
                    return;
                }

                ViewModel.CandidateName = players[0].GetProperty("name").GetString();
                ViewModel.CandidateHash = players[0].GetProperty("nucleusHash").GetString();
                ViewModel.VerificationStatus = SettingsViewModel.VerificationState.Pending;
            });
        }

        private void BtnClearHash_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.NucleusHash = null;
            ViewModel.NucleusHash = null;
            App.SettingsService.Save(App.Settings);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow!._wss.NucleusHash = App.Settings.NucleusHash;
            mainWindow!.ViewModel.NucleusHash = App.Settings.NucleusHash;
        }

        private async void BtnInitiate_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.VerificationStatus = SettingsViewModel.VerificationState.Waiting;
            ViewModel.CandidateName = null;
            ViewModel.CandidateHash = null;

            var mainWindow = Application.Current.MainWindow as MainWindow;
            await mainWindow!._wss.SendGetPlayers();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.NucleusHash = ViewModel.CandidateHash;
            ViewModel.NucleusHash = ViewModel.CandidateHash;
            ViewModel.CandidateName = null;
            ViewModel.CandidateHash = null;
            ViewModel.VerificationStatus = SettingsViewModel.VerificationState.Idle;
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow!._wss.NucleusHash = App.Settings.NucleusHash;
            mainWindow!.ViewModel.NucleusHash = App.Settings.NucleusHash;
            App.SettingsService.Save(App.Settings);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_saved)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?._medsWindow?.ApplySettings(App.Settings);
                mainWindow?._nadesWindow?.ApplySettings(App.Settings);
                mainWindow?._respawnWindow?.ApplySettings(App.Settings);
            }
            base.OnClosing(e);
        }

        private static string ToHex(Color color) =>
            $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}