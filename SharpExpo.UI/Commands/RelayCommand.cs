using System.Windows.Input;
using SharpExpo.UI.Services;

namespace SharpExpo.UI.Commands;

/// <summary>
/// Простая реализация ICommand для использования в ViewModel
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Func<Task>? _asyncExecute;
    private readonly Action? _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public RelayCommand(Func<Task> asyncExecute, Func<bool>? canExecute = null)
    {
        _asyncExecute = asyncExecute ?? throw new ArgumentNullException(nameof(asyncExecute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    public void Execute(object? parameter)
    {
        try
        {
            if (_asyncExecute != null)
            {
                // Запускаем async метод и обрабатываем исключения
                var task = _asyncExecute();
                if (task != null)
                {
                    task.ContinueWith(t =>
                    {
                        if (t.IsFaulted && t.Exception != null)
                        {
                            Logger.LogError("Ошибка в async команде", t.Exception);
                            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                            {
                                System.Windows.MessageBox.Show(
                                    $"Ошибка выполнения команды: {t.Exception.GetBaseException().Message}\n\nДетали в логе: {Logger.LogFilePath}",
                                    "Ошибка",
                                    System.Windows.MessageBoxButton.OK,
                                    System.Windows.MessageBoxImage.Error);
                            });
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }
            }
            else
            {
                _execute?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Ошибка выполнения команды", ex);
            System.Windows.MessageBox.Show(
                $"Ошибка выполнения команды: {ex.Message}\n\nДетали в логе: {Logger.LogFilePath}",
                "Ошибка",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Выполняет команду асинхронно и ждет завершения
    /// </summary>
    public async Task ExecuteAsync(object? parameter)
    {
        if (_asyncExecute != null)
        {
            await _asyncExecute();
        }
        else if (_execute != null)
        {
            _execute();
        }
    }
}

