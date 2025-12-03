using System.Windows.Input;
using SharpExpo.Contracts.Models;

namespace SharpExpo.UI.ViewModels;

/// <summary>
/// ViewModel для строки свойства в DataGrid
/// </summary>
public class PropertyRowViewModel : ViewModelBase
{
    private bool _isExpanded;
    private string? _propertyValue;

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
    public string? PropertyValue
    {
        get => _propertyValue;
        set
        {
            if (_propertyValue != value)
            {
                _propertyValue = value;
                OnPropertyChanged();
            }
        }
    }

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
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Описание свойства (для тултипа)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Команда для переключения состояния развернутости секции
    /// </summary>
    public ICommand? ToggleExpandCommand { get; set; }

    /// <summary>
    /// Ссылка на оригинальный OptionProperty для сохранения изменений
    /// </summary>
    public OptionProperty? OriginalProperty { get; set; }

    /// <summary>
    /// ID FamilyOption, к которому относится это свойство
    /// </summary>
    public string? FamilyOptionId { get; set; }

    /// <summary>
    /// Тип значения свойства
    /// </summary>
    public OptionValueType ValueType { get; set; }

    /// <summary>
    /// Указывает, можно ли редактировать это свойство
    /// </summary>
    public bool IsEditable => !IsSectionHeader && !IsLocked && 
                              (ValueType == OptionValueType.String || ValueType == OptionValueType.Double);
}

