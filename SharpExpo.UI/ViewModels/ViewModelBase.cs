using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SharpExpo.UI.ViewModels;

/// <summary>
/// Base class for view models with implementation of <see cref="INotifyPropertyChanged"/>.
/// Provides property change notification functionality for data binding.
/// </summary>
/// <remarks>
/// WHY: This base class provides a common implementation of INotifyPropertyChanged for all view models.
/// It uses the CallerMemberName attribute to automatically determine the property name, reducing boilerplate code.
/// </remarks>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed. If not specified, the caller member name is used.</param>
    /// <remarks>
    /// WHY: This method uses the CallerMemberName attribute to automatically determine the property name,
    /// allowing view models to call OnPropertyChanged() without specifying the property name explicitly.
    /// This reduces errors from typos and makes the code more maintainable.
    /// </remarks>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
