using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using rc_network_tool.Services;
using rc_network_tool.ViewModels;
using rc_network_tool.Views;



#if WINDOWS10_0_17763_0_OR_GREATER
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
#endif

namespace rc_network_tool;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddTransient<INetworkAdapterService, NetworkAdapterService>();
        builder.Services.AddSingleton<IMacOuiRegistryService, MacOuiRegistryService>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        builder.ConfigureLifecycleEvents(events =>
        {
#if WINDOWS10_0_17763_0_OR_GREATER
            events.AddWindows(wndLifeCycleBuilder =>
            {
                wndLifeCycleBuilder.OnWindowCreated(window =>
                {
                    if (MicaController.IsSupported())
                        window.SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
                    else if (DesktopAcrylicController.IsSupported())
                        window.SystemBackdrop = new DesktopAcrylicBackdrop();
                });
            });
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
