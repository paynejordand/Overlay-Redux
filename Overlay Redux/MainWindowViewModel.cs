using System.ComponentModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private string? _nucleusHash;
    public string? NucleusHash
    {
        get => _nucleusHash;
        set
        {
            _nucleusHash = value;
            OnPropertyChanged(nameof(NucleusHash));
            OnPropertyChanged(nameof(VerifiedStatus));
        }
    }

    public string VerifiedStatus => string.IsNullOrEmpty(NucleusHash) ? "Unverified" : "Verified";
}