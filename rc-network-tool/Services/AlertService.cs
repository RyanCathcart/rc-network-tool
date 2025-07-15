using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace rc_network_tool.Services;

public class AlertService : IAlertService
{
    public Task ShowAlertAsync(string? title, string message, string closeText = "Close")
    {
        return Shell.Current.CurrentPage.DisplayAlert(title, message, closeText);
    }

    public Task ShowToastAsync(string message, ToastDuration duration = ToastDuration.Short, CancellationToken cancellationToken = default)
    {
        var toast = Toast.Make(message, duration);

        return toast.Show(cancellationToken);
    }
}
