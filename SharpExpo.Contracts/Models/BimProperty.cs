namespace SharpExpo.Contracts.Models;

/// <summary>
/// Представляет свойство BIM компонента
/// </summary>
public class BimProperty
{
    /// <summary>
    /// Название свойства
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Значение свойства
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Указывает, заблокировано ли свойство (read-only)
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Категория свойства (для иконки 'C')
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Указывает, имеет ли свойство выпадающий список
    /// </summary>
    public bool HasDropdown { get; set; }
}

