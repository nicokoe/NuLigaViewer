using NuLigaViewer.Data;
using System.ComponentModel;

namespace NuLigaViewer.ViewModels
{
    public class ClubPlayerViewModel : INotifyPropertyChanged
    {
        private readonly ClubPlayer _player;

        public ClubPlayerViewModel(ClubPlayer player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public int? DewisPkz => _player.DewisPkz;
        public int Rang => _player.Rang;
        public string? Name => _player.Name;
        public int? DWZ => _player.DWZ;
        public int? Number => _player.Number;
        public string? Status => _player.Status;
        public string? PlayerUrl => _player.PlayerUrl;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}