using NuLigaViewer.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace NuLigaViewer.ViewModels
{
    public class BadenRegion : ObservableCollection<League>
    {
        public string Name { get; set; }

        public BadenRegion(string name, ObservableCollection<League> leagues) : base(leagues)
        {
            Name = name;
        }
    }

    public class LeaguesViewModel : INotifyPropertyChanged
    {
        public LeaguesViewModel()
        {
            _settingsCommand = new RelayCommand(GoToSettings, () => true);

            var preferedCategory = Preferences.Default.Get("category", "Verbandsrunde");
            _category = (Category)Enum.Parse(typeof(Category), preferedCategory);
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
        public ObservableCollection<BadenRegion> Regions { get; } = new ObservableCollection<BadenRegion>();
        public IEnumerable<Category> Categories => Enum.GetValues<Category>();

        private Category _category;
        public Category Category
        {
            get => _category;
            set
            {
                if (_category == value)
                {
                    return;
                }
                _category = value;

                try
                {
                    Preferences.Default.Set("category", value.ToString());

                    var regions = NuLigaParser.ParseLeagues(Year, value);
                    LoadLeaguesFromRegions(regions);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }

                OnPropertyChanged(nameof(Category));
            }
        }

        public void LoadLeaguesFromRegions(List<List<League>> regions)
        {
            foreach(var region in Regions)
            {
                region.Clear();
            }
            Regions.Clear();

            for (int i = 0; i < regions.Count(); i++)
            {
                var leagues = regions.ElementAt(i);
                if (leagues.Count > 0)
                {
                    var newRegion = new BadenRegion(leagues.First().Region, new ObservableCollection<League>());
                    Regions.Add(newRegion);
                    foreach (var league in leagues)
                    {
                        newRegion.Add(league);
                    }
                }
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