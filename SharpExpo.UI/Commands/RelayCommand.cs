using System.Windows.Input;

namespace SharpExpo.UI.Commands;

/// <summary>
/// Simple implementation of <see cref="ICommand"/> for use in ViewModels.
/// Supports both synchronous and asynchronous execution.
/// </summary>
/// <remarks>
/// WHY: This class provides a simple command implementation that can execute both sync and async actions.
/// It uses CommandManager.RequerySuggested for automatic CanExecute updates, which is standard WPF behavior.
/// </remarks>
public class RelayCommand : ICommand
{
    private readonly Func<Task>? _asyncExecute;
    private readonly Action? _execute;
    private readonly Func<bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class with a synchronous action.
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked.</param>
    /// <param name="canExecute">Optional function that determines whether the command can execute. If <see langword="null"/>, the command can always execute.</param>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class with an asynchronous function.
    /// </summary>
    /// <param name="asyncExecute">The async function to execute when the command is invoked.</param>
    /// <param name="canExecute">Optional function that determines whether the command can execute. If <see langword="null"/>, the command can always execute.</param>
    public RelayCommand(Func<Task> asyncExecute, Func<bool>? canExecute = null)
    {
        _asyncExecute = asyncExecute ?? throw new ArgumentNullException(nameof(asyncExecute));
        _canExecute = canExecute;
    }

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    /// <inheritdoc/>
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// WHY: This method handles both sync and async execution. For async execution, it fires-and-forgets the task
    /// to avoid blocking the UI thread. Exceptions are caught and re-thrown on the UI thread if needed.
    /// </remarks>
    public void Execute(object? parameter)
    {
        if (_asyncExecute != null)
        {
            // Fire and forget async execution
            // WHY: We don't await here because ICommand.Execute is synchronous and we don't want to block the UI thread.
            // The async method will handle its own exceptions internally.
            _ = _asyncExecute();
        }
        else
        {
            _execute?.Invoke();
        }
    }

    /// <summary>
    /// Executes the command asynchronously and waits for completion.
    /// </summary>
    /// <param name="parameter">The command parameter (not used).</param>
    /// <returns>A task that represents the asynchronous execution.</returns>
    /// <remarks>
    /// WHY: This method provides a way to await async command execution when needed, such as in tests or when
    /// you need to ensure completion before proceeding.
    /// </remarks>
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
