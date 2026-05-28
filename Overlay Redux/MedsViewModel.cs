using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Overlay_Redux
{
    public class MedsViewModel : INotifyPropertyChanged
    {
        private int _syringes;
        public int Syringes
        {
            get => _syringes;
            set { _syringes = value; OnPropertyChanged(nameof(Syringes)); }
        }

        private int _medKits;
        public int MedKits
        {
            get => _medKits;
            set { _medKits = value; OnPropertyChanged(nameof(MedKits)); }
        }

        private int _phoenixKits;
        public int PhoenixKits
        {
            get => _phoenixKits;
            set { _phoenixKits = value; OnPropertyChanged(nameof(PhoenixKits)); }
        }

        private int _shieldCells;
        public int ShieldCells
        {
            get => _shieldCells;
            set { _shieldCells = value; OnPropertyChanged(nameof(ShieldCells)); }
        }

        private int _shieldBatts;
        public int ShieldBatts
        {
            get => _shieldBatts;
            set { _shieldBatts = value; OnPropertyChanged(nameof(ShieldBatts)); }
        }

        private int _ultAccels;
        public int UltAccels
        {
            get => _ultAccels;
            set { _ultAccels = value; OnPropertyChanged(nameof(UltAccels)); }
        }

        // Styling
        private Brush _windowBackground;
        public Brush WindowBackground
        {
            get => _windowBackground;
            set { _windowBackground = value; OnPropertyChanged(nameof(WindowBackground)); }
        }

        private Brush _borderBrush;
        public Brush BorderBrush
        {
            get => _borderBrush;
            set { _borderBrush = value; OnPropertyChanged(nameof(BorderBrush)); }
        }

        private Brush _textForeground;
        public Brush TextForeground
        {
            get => _textForeground;
            set { _textForeground = value; OnPropertyChanged(nameof(TextForeground)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
