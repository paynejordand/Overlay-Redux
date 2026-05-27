using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        private WSServer _wss;
        private CancellationTokenSource? _cts;
        private MedsWindow? _medsWindow;
        private RespawnWindow? _respawnWindow;
        public MainWindow()
        {
            InitializeComponent();
            _wss = new(respawnCallback: HandleRespawn);
            _wss.MedsUpdated += HandleMedsUpdated;
            _wss.StatusUpdated += HandleStatusUpdated;
            _wss.MatchSetup += HandleMatchSetup;
            _wss.MatchEnded += HandleMatchEnded;
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsPath = System.IO.Path.Combine(appData, "OverlayRedux", "settings.json");
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
        private void HandleMatchSetup()
        {
            Dispatcher.Invoke(() =>
            {
                _medsWindow?.Close();
                _medsWindow = new MedsWindow();
                _medsWindow.Show();
            });
        }

        private void HandleMatchEnded()
        {
            Dispatcher.Invoke(() =>
            {
                _medsWindow?.Close();
                _medsWindow = null;
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

        // --- Respawn Window ---
        private void BtnRespawn_Click(object sender, RoutedEventArgs e)
        {
            EnsureRespawnWindow();
            _respawnWindow!.AddBanner(team: "RAH", players: ["Stink", "Monty"], duration: 0);
        }

        private void HandleRespawn(string team, List<string> players)
        {
            Dispatcher.Invoke(() =>
            {
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
            _respawnWindow?.Close();
            base.OnClosing(e);
        }
    }
}