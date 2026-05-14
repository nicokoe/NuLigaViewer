#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace NuLigaViewer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            if (Current != null)
            {
                Current.Resources["AppFontFamily"] = Preferences.Default.Get("fontname", "OpenSansRegular");
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

#if WINDOWS
        window.Created += (s, e) =>
        {
            if (s != null)
            {
            var mauiWin = (Microsoft.Maui.Controls.Window)s;
            var nativeWin = mauiWin.Handler.PlatformView as Microsoft.UI.Xaml.Window;

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWin);
            var winId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(winId);

            // Set needed size in physical pixels. 
            appWindow.Resize(new SizeInt32(500, 768));
            }
        };
#endif

            return window;
        }
    }
}