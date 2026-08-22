using NuLigaViewer.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace NuLigaViewer.ViewModels
{
    public class LeaguesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BadenRegion> Regions { get; } = new();

        public LeaguesViewModel()
        {
            _settingsCommand = new RelayCommand(GoToSettings, () => true);
        }

        private string _year = string.Empty;
        public string Year
        {
            get => _year;
            set
            {
                if (_year != value)
                {
                    _year = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public string Name => $"Verbandsrunde {Year}";

        private readonly RelayCommand _settingsCommand;
        public ICommand SettingsCommand => _settingsCommand;

        public void LoadRegions(string year, IEnumerable<BadenRegion> regions)
        {
            Year = year;
            Regions.Clear();
            foreach (var region in regions)
            {
                Regions.Add(region);
            }
        }

        public async static Task GoToSettings()
        {
            await Shell.Current.GoToAsync($"settings");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}