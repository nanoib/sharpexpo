using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SharpExpo.UI.ViewModels;

/// <summary>
/// Базовый класс для ViewModel с реализацией INotifyPropertyChanged
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

