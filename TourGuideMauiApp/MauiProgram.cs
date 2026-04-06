using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TourGuideMauiApp.Services;
using ZXing.Net.Maui.Controls;
using Mapsui.UI.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace TourGuideMauiApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
               
            });

        builder.Services.AddSingleton<DatabaseService>();

        builder.Services.AddTransient<Views.MainPage>();
        builder.Services.AddTransient<Views.MapPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}