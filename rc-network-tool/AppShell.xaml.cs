#if WINDOWS
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
#endif

namespace rc_network_tool;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // The below code is required to ensure that the Acrylic/Mica backdrop effects
        // don't layer atop each other and create unwanted visuals.
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
