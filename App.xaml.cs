using Microsoft.Extensions.DependencyInjection;

namespace NuLigaViewer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //   TODO T1
            if (Application.Current != null)
                Application.Current.Resources["AppFontFamily"] = "SemiCondensed";
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}