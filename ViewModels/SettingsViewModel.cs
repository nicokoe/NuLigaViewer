using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NuLigaViewer.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly Settings _settings;
        public SettingsViewModel(Settings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string TextSize
        {
            get => _settings.TextSize.ToString();
            set
            {
                if (Enum.TryParse<TextSize>(value, out var newTextSize) && _settings.TextSize != newTextSize)
                {
                    _settings.TextSize = newTextSize;
                    Preferences.Default.Set("TextSize", (int)newTextSize);
                    OnPropertyChanged(nameof(TextSize));
                }
            }
        }

        public IEnumerable<string> AvailableTextSizes => Enum.GetNames(typeof(TextSize));

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}