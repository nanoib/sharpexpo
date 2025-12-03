using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using SharpExpo.UI.Services;

namespace SharpExpo.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Обработка необработанных исключений
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        
        Logger.Log("Приложение запущено");
        Logger.Log($"Лог файл: {Logger.LogFilePath}");
        Console.WriteLine($"Лог файл: {Logger.LogFilePath}");
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Logger.LogError("Необработанное исключение в UI потоке", e.Exception);
        
        MessageBox.Show(
            $"Произошла необработанная ошибка:\n{e.Exception.Message}\n\nДетали в логе: {Logger.LogFilePath}",
            "Критическая ошибка",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        
        // Предотвращаем закрытие приложения
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        Logger.LogError("Необработанное исключение в домене приложения", exception);
        
        if (exception != null)
        {
            MessageBox.Show(
                $"Критическая ошибка:\n{exception.Message}\n\nДетали в логе: {Logger.LogFilePath}",
                "Критическая ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}

