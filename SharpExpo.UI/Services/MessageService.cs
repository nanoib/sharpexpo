using System.Windows;

namespace SharpExpo.UI.Services;

/// <summary>
/// WPF implementation of <see cref="IMessageService"/> that displays messages using MessageBox.
/// </summary>
/// <remarks>
/// WHY: This class implements IMessageService to abstract UI message display from business logic.
/// This enables testing without actual UI components and allows for future replacement with different UI frameworks.
/// </remarks>
public class MessageService : IMessageService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageService"/> class.
    /// </summary>
    public MessageService()
    {
    }

    /// <inheritdoc/>
    public void ShowInformation(string message, string title = "Information")
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(title);

        if (Application.Current?.Dispatcher != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }

    /// <inheritdoc/>
    public void ShowWarning(string message, string title = "Warning")
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(title);

        if (Application.Current?.Dispatcher != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }
    }

    /// <inheritdoc/>
    public void ShowError(string message, string title = "Error")
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(title);

        if (Application.Current?.Dispatcher != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }
}




