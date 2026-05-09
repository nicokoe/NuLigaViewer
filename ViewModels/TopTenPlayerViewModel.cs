using NuLigaViewer.Data;
using System.ComponentModel;

namespace NuLigaViewer.ViewModels
{
    public class TopTenPlayerViewModel : INotifyPropertyChanged
    {
        private readonly Player _player;

        public TopTenPlayerViewModel(Player player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public int Rang { get; set; }
        public string? Name => _player.Name;
        public int? DWZ => _player.DWZ == 1000 ? null : _player.DWZ;
        public string? Verein => Shortener.Instance.ShortenClubName(_player.TeamName);
        public double Punkte => _player.Points;
        public int Spiele => _player.Games;

        public void Refresh()
        {
            OnPropertyChanged(nameof(Rang));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DWZ));
            OnPropertyChanged(nameof(Verein));
            OnPropertyChanged(nameof(Punkte));
            OnPropertyChanged(nameof(Spiele));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}