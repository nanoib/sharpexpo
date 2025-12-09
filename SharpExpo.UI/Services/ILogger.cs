namespace SharpExpo.UI.Services;

/// <summary>
/// Provides logging functionality for the application.
/// This interface abstracts logging operations to enable dependency injection and testability.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Gets the path to the log file.
    /// </summary>
    /// <value>The full path to the log file where messages are written.</value>
    string LogFilePath { get; }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Log(string message);

    /// <summary>
    /// Logs an error message with optional exception details.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="exception">Optional exception that caused the error. If provided, exception details are included in the log.</param>
    void LogError(string message, Exception? exception = null);
}




