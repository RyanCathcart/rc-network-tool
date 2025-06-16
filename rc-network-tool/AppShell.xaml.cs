using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;

namespace rc_network_tool;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Loaded += (_, _) => {
#if WINDOWS
            var shellView = Current?.Handler?.PlatformView as ShellView;
            var navigationView = shellView?.Content as MauiNavigationView;

            var contentGrid = navigationView?.GetType()
                .GetProperty("ContentGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(navigationView) as Microsoft.UI.Xaml.Controls.Grid;

            contentGrid!.Background.Opacity = 0;
#endif
        };
    }
}
