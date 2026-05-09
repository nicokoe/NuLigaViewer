using NuLigaViewer.Data;
using System.ComponentModel;
using System.Windows.Input;

namespace NuLigaViewer.ViewModels
{
    public class TeamPairingViewModel : INotifyPropertyChanged
    {
        private readonly TeamPairing _teamPairing;

        public TeamPairingViewModel(TeamPairing teamPairing)
        {
            _backButtonCommand = new RelayCommand(AppShell.GoBackInStack, () => true);

            _teamPairing = teamPairing ?? throw new ArgumentNullException(nameof(teamPairing));
        }

        private readonly RelayCommand _backButtonCommand;
        public ICommand BackButtonCommand => _backButtonCommand;

        public int Runde => _teamPairing.Runde;
        public DateTime Datum => _teamPairing.Datum;
        public string Title => _teamPairing.Title;
        public string? GastMannschaft => Shortener.Instance.ShortenClubName(_teamPairing.GastMannschaft); 
        public double GastMannschaftDWZ => _teamPairing.GastMannschaftDWZ;
        public string? HeimMannschaft => Shortener.Instance.ShortenClubName(_teamPairing.HeimMannschaft);
        public double HeimMannschaftDWZ => _teamPairing.HeimMannschaftDWZ;
        public string? BrettPunkte => _teamPairing.BrettPunkte;
        public GameReport? Report => _teamPairing.Report;

        public bool ContainsTeamPairing(TeamPairing teamPairing) => teamPairing == _teamPairing;

        public void Refresh()
        {
            OnPropertyChanged(nameof(Runde));
            OnPropertyChanged(nameof(Datum));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(GastMannschaft));
            OnPropertyChanged(nameof(GastMannschaftDWZ));
            OnPropertyChanged(nameof(HeimMannschaft));
            OnPropertyChanged(nameof(HeimMannschaftDWZ));
            OnPropertyChanged(nameof(BrettPunkte));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}