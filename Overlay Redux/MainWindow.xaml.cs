using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Overlay_Redux
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; } = new();

        internal WSServer _wss;
        private CancellationTokenSource? _cts;
        internal MedsWindow? _medsWindow;
        internal NadesWindow? _nadesWindow;
        internal RespawnWindow? _respawnWindow;
        internal SettingsWindow? _settingsWindow;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.NucleusHash = App.Settings.NucleusHash;
            _wss = new(respawnCallback: HandleRespawn)
            {
                NucleusHash = App.Settings.NucleusHash
            };
            _wss.InventoryUpdated += HandleInventoryUpdated;
            _wss.StatusUpdated += HandleStatusUpdated;
            _wss.MatchSetup += HandleMatchSetup;
            _wss.MatchEnded += HandleMatchEnded;
        }

        // --- Settings ---
        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            _settingsWindow = new SettingsWindow();
            _settingsWindow.ShowDialog();
        }

        // --- Server ---
        private void BtnStartWSS_Click(object sender, RoutedEventArgs e)
        {
            BtnStartWSS.IsEnabled = false;
            _cts = new CancellationTokenSource();
            Task.Run(() => _wss.Start(), _cts.Token);
        }

        private void HandleStatusUpdated(string status)
        {
            Dispatcher.Invoke(() => TxtServerStatus.Text = $"Server: {status}");
        }

        // --- Meds Window ---
        private void BtnMeds_Click(object sender, RoutedEventArgs e)
        {
            if (_medsWindow == null || !_medsWindow.IsVisible)
            {
                _medsWindow = new MedsWindow();
                _medsWindow.Show();
            }

        }

        // --- Nades Window ---
        private void BtnNades_Click(object sender, RoutedEventArgs e)
        {
            if (_nadesWindow == null || !_nadesWindow.IsVisible)
            {
                _nadesWindow = new NadesWindow();
                _nadesWindow.Show();
            }
        }

        private void HandleMatchSetup()
        {
            Dispatcher.Invoke(() =>
            {
                if (App.Settings.MedsWindowActive)
                {
                    _medsWindow?.Close();
                    _medsWindow = new MedsWindow();
                    _medsWindow.Show();
                }
                if (App.Settings.NadesWindowActive)
                {
                    _nadesWindow?.Close();
                    _nadesWindow = new NadesWindow();
                    _nadesWindow.Show();
                }
            });
        }

        private void HandleMatchEnded()
        {
            Dispatcher.Invoke(() =>
            {
                _medsWindow?.Close();
                _nadesWindow?.Close();
                _medsWindow = null;
                _nadesWindow = null;
            });
        }
        private void HandleMedsUpdated(Dictionary<string, int> meds)
        {
            Dispatcher.Invoke(() =>
            {
                if (_medsWindow == null || !_medsWindow.IsVisible) return;
                _medsWindow.ViewModel.Syringes = meds["syringes"];
                _medsWindow.ViewModel.MedKits = meds["medkits"];
                _medsWindow.ViewModel.PhoenixKits = meds["phoenixKits"];
                _medsWindow.ViewModel.ShieldCells = meds["shieldCells"];
                _medsWindow.ViewModel.ShieldBatts = meds["shieldBatteries"];
                _medsWindow.ViewModel.UltAccels = meds["ultimateAccelerants"];
            });
        }
        private void HandleInventoryUpdated(Dictionary<string, (int Count, string Category)> items)
        {
            Dispatcher.Invoke(() =>
            {
                if (_medsWindow != null && _medsWindow.IsVisible)
                {
                    _medsWindow.ViewModel.Syringes = items["syringes"].Count;
                    _medsWindow.ViewModel.MedKits = items["medkits"].Count;
                    _medsWindow.ViewModel.PhoenixKits = items["phoenixKits"].Count;
                    _medsWindow.ViewModel.ShieldCells = items["shieldCells"].Count;
                    _medsWindow.ViewModel.ShieldBatts = items["shieldBatteries"].Count;
                    _medsWindow.ViewModel.UltAccels = items["ultimateAccelerants"].Count;
                }

                if (_nadesWindow != null && _nadesWindow.IsVisible)
                {
                    _nadesWindow.ViewModel.Frags = items["frags"].Count;
                    _nadesWindow.ViewModel.Thermites = items["thermites"].Count;
                    _nadesWindow.ViewModel.Arcs = items["arcStars"].Count;
                }
            });
        }

        // --- Respawn Window ---
        private void BtnRespawn_Click(object sender, RoutedEventArgs e)
        {
            EnsureRespawnWindow();
            _respawnWindow!.AddBanner(team: "Lost Lake Boys", players: ["Stinkerson", "Hannibal of Carthage"], duration: 0);
        }

        private void HandleRespawn(string team, List<string> players)
        {
            Dispatcher.Invoke(() =>
            {
                if (!App.Settings.RespawnWindowActive) return;
                EnsureRespawnWindow();
                _respawnWindow!.AddBanner(team, players);
            });
        }

        private void EnsureRespawnWindow()
        {
            if (_respawnWindow == null || !_respawnWindow.IsVisible)
            {
                _respawnWindow = new RespawnWindow();
                _respawnWindow.Show();
            }
        }

        // --- Cleanup ---
        protected override void OnClosing(CancelEventArgs e)
        {
            _cts?.Cancel();
            _wss?.Stop();
            _medsWindow?.Close();
            _nadesWindow?.Close();
            _respawnWindow?.Close();
            base.OnClosing(e);
        }
    }
}