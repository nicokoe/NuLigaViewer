using NuLigaViewer.ViewModels;

namespace NuLigaViewer.Pages
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            var textSize = (TextSize)Preferences.Default.Get("TextSize", 0);
            BindingContext = new SettingsViewModel(new Settings { TextSize = textSize });
        }
    }
}