using NuLigaViewer.Data;
using System.ComponentModel;

namespace NuLigaViewer.ViewModels
{
    public class GameDayViewModel : INotifyPropertyChanged
    {
        private readonly TeamPairing _teamPairing;

        public GameDayViewModel(TeamPairing teamPairing)
        {
            _teamPairing = teamPairing ?? throw new ArgumentNullException(nameof(teamPairing));
        }
        public int Runde => _teamPairing.Runde;
        public string? HeimMannschaft => Shortener.Instance.ShortenClubName(_teamPairing.HeimMannschaft);
        public double HeimMannschaftDWZ => _teamPairing.HeimMannschaftDWZ;
        public string? GastMannschaft => Shortener.Instance.ShortenClubName(_teamPairing.GastMannschaft);
        public double GastMannschaftDWZ => _teamPairing.GastMannschaftDWZ;
        public string? BrettPunkte => _teamPairing.BrettPunkte;

        public void Refresh()
        {
            OnPropertyChanged(nameof(Runde));
            OnPropertyChanged(nameof(HeimMannschaft));
            OnPropertyChanged(nameof(HeimMannschaftDWZ));
            OnPropertyChanged(nameof(GastMannschaft));
            OnPropertyChanged(nameof(GastMannschaftDWZ));
            OnPropertyChanged(nameof(BrettPunkte));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}