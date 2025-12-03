namespace SharpExpo.UI.ViewModels;

/// <summary>
/// ViewModel для строки свойства в DataGrid
/// </summary>
public class PropertyRowViewModel : ViewModelBase
{
    /// <summary>
    /// Указывает, является ли строка заголовком секции
    /// </summary>
    public bool IsSectionHeader { get; set; }

    /// <summary>
    /// Название секции (для заголовков)
    /// </summary>
    public string? SectionName { get; set; }

    /// <summary>
    /// Название свойства
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Значение свойства
    /// </summary>
    public string? PropertyValue { get; set; }

    /// <summary>
    /// Указывает, заблокировано ли свойство
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Указывает, имеет ли свойство категорию
    /// </summary>
    public bool HasCategory { get; set; }

    /// <summary>
    /// Указывает, имеет ли свойство выпадающий список
    /// </summary>
    public bool HasDropdown { get; set; }

    /// <summary>
    /// Указывает, развернута ли секция
    /// </summary>
    public bool IsExpanded { get; set; }
}

