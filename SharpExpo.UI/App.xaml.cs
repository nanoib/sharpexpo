using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using SharpExpo.UI.Services;

namespace SharpExpo.UI;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
/// <remarks>
/// WHY: This class handles application-level initialization and unhandled exception handling.
/// It uses a static logger instance for early initialization before the DI container is set up.
/// </remarks>
public partial class App : Application
{
    private static readonly ILogger _logger = new Logger();

    /// <summary>
    /// Raises the <see cref="Application.Startup"/> event.
    /// </summary>
    /// <param name="e">A <see cref="StartupEventArgs"/> that contains the event data.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Handle unhandled exceptions
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        _logger.Log("Приложение запущено");
        _logger.Log($"Лог файл: {_logger.LogFilePath}");
        _logger.Log($"Аргументы командной строки: {string.Join(" ", e.Args)}");
        Console.WriteLine($"Лог файл: {_logger.LogFilePath}");
    }

    /// <summary>
    /// Handles unhandled exceptions in the UI thread.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments containing the exception.</param>
    /// <remarks>
    /// WHY: This method prevents the application from crashing on unhandled UI thread exceptions,
    /// logging the error and showing a message to the user instead.
    /// </remarks>
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _logger.LogError("Необработанное исключение в UI потоке", e.Exception);

        MessageBox.Show(
            $"Произошла необработанная ошибка:\n{e.Exception.Message}\n\nДетали в логе: {_logger.LogFilePath}",
            "Критическая ошибка",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        // Prevent application shutdown
        e.Handled = true;
    }

    /// <summary>
    /// Handles unhandled exceptions in the application domain.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments containing the exception.</param>
    /// <remarks>
    /// WHY: This method handles exceptions that occur outside the UI thread, such as in background tasks.
    /// These exceptions cannot be handled gracefully, but we log them for debugging purposes.
    /// </remarks>
    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        _logger.LogError("Необработанное исключение в домене приложения", exception);

        if (exception != null)
        {
            MessageBox.Show(
                $"Критическая ошибка:\n{exception.Message}\n\nДетали в логе: {_logger.LogFilePath}",
                "Критическая ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
