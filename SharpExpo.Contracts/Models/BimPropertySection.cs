namespace SharpExpo.Contracts.Models;

/// <summary>
/// Представляет секцию свойств BIM компонента
/// </summary>
public class BimPropertySection
{
    /// <summary>
    /// Название секции
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Список свойств в секции
    /// </summary>
    public List<BimProperty> Properties { get; set; } = new();

    /// <summary>
    /// Указывает, развернута ли секция по умолчанию
    /// </summary>
    public bool IsExpanded { get; set; } = true;
}

