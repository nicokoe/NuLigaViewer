using NuLigaViewer.Data;
using System.ComponentModel;

namespace NuLigaViewer.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private static Dictionary<Font, string> FontMapping { get; } = new Dictionary<Font, string>
        {
            { Font.OpenSansRegular, "100 %" },
            { Font.BarlowRegular, "95 %" },
            { Font.SemiCondensed, "88 %" },
            { Font.Condensed, "78 %" },
            { Font.ExtraCondensed, "65 %" }
        };

        private readonly Settings _settings;

        public SettingsViewModel(Settings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public IEnumerable<string> FontWidths => FontMapping.Values.ToList();
        public IEnumerable<string> Years => new List<string> { "2025/26", "2026/27" };

        public string FontWidth
        {
            get => FontMapping[_settings.Font];
            set
            {
                var font = FontMapping.FirstOrDefault(x => x.Value == value).Key;
                if (_settings.Font == font)
                {
                    return;
                }

                _settings.Font = font;

                try
                {
                    Preferences.Default.Set("fontname", font.ToString());
                    if (Application.Current != null)
                    {
                        Application.Current.Resources["AppFontFamily"] = font.ToString();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }

                OnPropertyChanged(nameof(FontWidth));
            }
        }

        public string Year
        {
            get => _settings.Year;
            set
            {
                if (_settings.Year == value)
                {
                    return;
                }
                _settings.Year = value;

                try
                {
                    Preferences.Default.Set("year", value);
                    var regions = NuLigaParser.ParseLeagues(value, Category.Open);
                    NavigationState.SelectedRegionsViewModel.LoadRegions(value, regions);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }

                OnPropertyChanged(nameof(Year));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}