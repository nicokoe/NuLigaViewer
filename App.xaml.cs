using Microsoft.Extensions.DependencyInjection;

namespace NuLigaViewer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            if (Application.Current != null)
            {
                Application.Current.Resources["AppFontFamily"] = Preferences.Get("fontname", "OpenSansRegular");
                Shortener.Instance.SetShortenClubName(Preferences.Get("shortenClubName", "0"));
                Shortener.Instance.SetShortenPlayerName(Preferences.Get("shortenPlayerName", "1"));
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}