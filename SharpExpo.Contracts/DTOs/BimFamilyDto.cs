namespace SharpExpo.Contracts.DTOs;

/// <summary>
/// DTO для десериализации JSON файла BIM-семейства
/// </summary>
public class BimFamilyDto
{
    /// <summary>
    /// Уникальный идентификатор BIM-семейства
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Имя BIM-семейства
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Список идентификаторов опций семейства
    /// </summary>
    public List<string> FamilyOptionIds { get; set; } = new();

    /// <summary>
    /// Порядок категорий для отображения
    /// </summary>
    public List<string> CategoryOrder { get; set; } = new();
}


