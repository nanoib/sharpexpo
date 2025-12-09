using System.IO;

namespace SharpExpo.UI.Services;

/// <summary>
/// Default implementation of <see cref="ILogger"/> that writes log messages to both debug output and a file.
/// This implementation provides file-based logging with automatic directory creation.
/// </summary>
/// <remarks>
/// WHY: This class implements ILogger to enable dependency injection and improve testability.
/// The file-based logging provides persistent logs for debugging production issues.
/// </remarks>
public class Logger : ILogger
{
    private readonly string _logFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="logFilePath">The path to the log file. If <see langword="null"/>, uses the default path in LocalApplicationData.</param>
    public Logger(string? logFilePath = null)
    {
        _logFilePath = logFilePath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SharpExpo",
        "log.txt");

        var logDirectory = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    /// <inheritdoc/>
    public string LogFilePath => _logFilePath;

    /// <inheritdoc/>
    public void Log(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        System.Diagnostics.Debug.WriteLine(logMessage);
        
        try
        {
            File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
        }
        catch
        {
            // WHY: We silently ignore file write errors to prevent logging failures from crashing the application.
            // In production, this could be enhanced with a fallback mechanism or retry logic.
        }
    }

    /// <inheritdoc/>
    public void LogError(string message, Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        var errorMessage = $"[ERROR] {message}";
        if (exception != null)
        {
            errorMessage += $"{Environment.NewLine}Exception: {exception.GetType().Name}{Environment.NewLine}Message: {exception.Message}{Environment.NewLine}Stack: {exception.StackTrace}";
        }
        Log(errorMessage);
    }
}




