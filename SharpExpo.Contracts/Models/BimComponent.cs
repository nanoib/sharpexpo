namespace SharpExpo.Contracts.Models;

/// <summary>
/// Представляет BIM компонент со всеми его свойствами
/// </summary>
public class BimComponent
{
    /// <summary>
    /// Уникальный идентификатор компонента
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Наименование компонента
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Секции свойств компонента
    /// </summary>
    public List<BimPropertySection> Sections { get; set; } = new();
}

