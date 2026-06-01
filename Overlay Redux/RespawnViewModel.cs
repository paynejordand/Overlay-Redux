using System.ComponentModel;
using System.Windows.Media;

namespace Overlay_Redux
{
    public class RespawnViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private Brush? _background;
        public Brush? Background
        {
            get => _background;
            set { _background = value; OnPropertyChanged(nameof(Background)); }
        }

        private Brush? _foreground;
        public Brush? Foreground
        {
            get => _foreground;
            set { _foreground = value; OnPropertyChanged(nameof(Foreground)); }
        }
    }
}