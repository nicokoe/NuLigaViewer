using NuLigaViewer.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace NuLigaViewer.ViewModels
{
    public class LeaguesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BadenRegion> Regions { get; }

        public LeaguesViewModel(IEnumerable<BadenRegion> regions)
        {
            _settingsCommand = new RelayCommand(GoToSettings, () => true);
            Regions = new ObservableCollection<BadenRegion>(regions);
        }

        private readonly RelayCommand _settingsCommand;
        public ICommand SettingsCommand => _settingsCommand;

        public async static Task GoToSettings()
        {
            await Shell.Current.GoToAsync($"settings");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}