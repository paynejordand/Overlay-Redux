using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Overlay_Redux
{
    public class NadesViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private int _frags;
        public int Frags
        {
            get => _frags;
            set { _frags = value; OnPropertyChanged(nameof(Frags)); }
        }

        private int _thermites;
        public int Thermites
        {
            get => _thermites;
            set { _thermites = value; OnPropertyChanged(nameof(Thermites)); }
        }

        private int _arcs;
        public int Arcs
        {
            get => _arcs;
            set { _arcs = value; OnPropertyChanged(nameof(Arcs)); }
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
    }
}
