using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            var font = Application.Current?.Resources["AppFontFamily"] as string;
            var year = Preferences.Default.Get("year", "2026/27");
            var settings = new Settings
            {
                Font = Enum.TryParse<Font>(font, out var parsedFont) ? parsedFont : Font.OpenSansRegular,
                Year = year
            };
            BindingContext = new SettingsViewModel(settings);
        }
    }
}