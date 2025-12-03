namespace SharpExpo.UI.Services;

/// <summary>
/// Provides user message display functionality.
/// This interface abstracts message display operations to enable dependency injection and testability.
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Shows an information message to the user.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    /// <param name="title">The title of the message box. Defaults to "Information".</param>
    void ShowInformation(string message, string title = "Information");

    /// <summary>
    /// Shows a warning message to the user.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    /// <param name="title">The title of the message box. Defaults to "Warning".</param>
    void ShowWarning(string message, string title = "Warning");

    /// <summary>
    /// Shows an error message to the user.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    /// <param name="title">The title of the message box. Defaults to "Error".</param>
    void ShowError(string message, string title = "Error");
}


