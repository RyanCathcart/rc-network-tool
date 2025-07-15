using CommunityToolkit.Maui.Core;

namespace rc_network_tool.Services;

public interface IAlertService
{
    /// <summary>
    /// Displays an asynchronous alert dialog with a title, message, and a close button.
    /// </summary>
    /// <param name="title">The title of the alert. Can be <see langword="null"/> or empty to omit the title.</param>
    /// <param name="message">The message to display in the alert. This parameter is required and cannot be <see langword="null"/>.</param>
    /// <param name="closeText">The text to display on the close button. Defaults to "Close" if not specified.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task ShowAlertAsync(string? title, string message, string closeText = "Close");

    /// <summary>
    /// Displays a toast notification with the specified message and duration.
    /// </summary>
    /// <remarks>The method is asynchronous and does not block the calling thread. The toast notification's
    /// appearance and behavior may vary depending on the platform or environment.</remarks>
    /// <param name="message">The message to display in the toast notification. Cannot be null or empty.</param>
    /// <param name="duration">The duration for which the toast notification is displayed. Defaults to <see cref="ToastDuration.Short"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. If canceled, the toast may not be displayed.</param>
    /// <returns>A task that represents the asynchronous operation of displaying the toast notification.</returns>
    Task ShowToastAsync(string message, ToastDuration duration = ToastDuration.Short, CancellationToken cancellationToken = default);
}
