using System;
using System.IO;

namespace SharpExpo.UI.Services;

/// <summary>
/// Простой логгер для отладки
/// </summary>
public static class Logger
{
    public static string LogFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SharpExpo",
        "log.txt");

    static Logger()
    {
        var logDirectory = Path.GetDirectoryName(LogFilePath);
        if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    public static void Log(string message)
    {
        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        System.Diagnostics.Debug.WriteLine(logMessage);
        
        try
        {
            File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
        }
        catch
        {
            // Игнорируем ошибки записи в лог
        }
    }

    public static void LogError(string message, Exception? ex = null)
    {
        var errorMessage = $"[ERROR] {message}";
        if (ex != null)
        {
            errorMessage += $"{Environment.NewLine}Exception: {ex.GetType().Name}{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack: {ex.StackTrace}";
        }
        Log(errorMessage);
    }
}

