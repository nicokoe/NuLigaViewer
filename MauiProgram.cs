using Microsoft.Extensions.Logging;

namespace NuLigaViewer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Barlow-Regular.ttf", "BarlowRegular");
                    fonts.AddFont("BarlowSemiCondensed-Regular.ttf", "SemiCondensed");
                    fonts.AddFont("BarlowCondensed-Regular.ttf", "Condensed");
                    fonts.AddFont("SofiaSansExtraCondensed-Regular.ttf", "ExtraCondensed");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
