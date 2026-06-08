using System.ComponentModel;

namespace Overlay_Redux
{
    public class SettingsViewModel : INotifyPropertyChanged
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
                OnPropertyChanged(nameof(HashStatus));
                OnPropertyChanged(nameof(HashIsSet));
                OnPropertyChanged(nameof(VerificationStatusText));
            }
        }

        private bool _medsWindowActive;
        public bool MedsWindowActive
        {
            get => _medsWindowActive;
            set { _medsWindowActive = value; OnPropertyChanged(nameof(MedsWindowActive)); }
        }

        private bool _respawnWindowActive;
        public bool RespawnWindowActive
        {
            get => _respawnWindowActive;
            set { _respawnWindowActive = value; OnPropertyChanged(nameof(RespawnWindowActive)); }
        }

        private bool _nadesWindowActive;
        public bool NadesWindowActive
        {
            get => _nadesWindowActive;
            set { _nadesWindowActive = value; OnPropertyChanged(nameof(NadesWindowActive)); }
        }

        // Derived properties for UI binding
        public string HashStatus => string.IsNullOrEmpty(NucleusHash) ? "Unverified" : "Verified";
        public bool HashIsSet => !string.IsNullOrEmpty(NucleusHash);

        // Verification process state
        public enum VerificationState { Idle, Waiting, Pending, Failed }

        private VerificationState _verificationStatus = VerificationState.Idle;
        public VerificationState VerificationStatus
        {
            get => _verificationStatus;
            set
            {
                _verificationStatus = value;
                OnPropertyChanged(nameof(VerificationStatus));
                OnPropertyChanged(nameof(VerificationStatusText));
                OnPropertyChanged(nameof(CanStartVerification));
            }
        }

        private string? _verificationFailureReason;
        public string? VerificationFailureReason
        {
            get => _verificationFailureReason;
            set
            {
                _verificationFailureReason = value;
                OnPropertyChanged(nameof(VerificationFailureReason));
                OnPropertyChanged(nameof(VerificationStatusText));
            }
        }

        public string VerificationStatusText => VerificationStatus switch
        {
            VerificationState.Idle => string.IsNullOrEmpty(NucleusHash) ? "Not started" : "Confirmed",
            VerificationState.Waiting => "Waiting for response...",
            VerificationState.Pending => "Pending confirmation",
            VerificationState.Failed => $"Failed — {VerificationFailureReason}",
            _ => string.Empty
        };

        public bool CanStartVerification => VerificationStatus != VerificationState.Waiting;

        private string? _candidateName;
        public string? CandidateName
        {
            get => _candidateName;
            set
            {
                _candidateName = value;
                OnPropertyChanged(nameof(CandidateName));
                OnPropertyChanged(nameof(CanConfirm));
            }
        }

        private string? _candidateHash;
        public string? CandidateHash
        {
            get => _candidateHash;
            set 
            { 
                _candidateHash = value; 
                OnPropertyChanged(nameof(CandidateHash));
                OnPropertyChanged(nameof(CanConfirm));
            }
        }

        public bool CanConfirm => !string.IsNullOrEmpty(CandidateName) && !string.IsNullOrEmpty(CandidateHash);
    }
}