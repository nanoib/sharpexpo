namespace SharpExpo.Contracts.DTOs;

/// <summary>
/// DTO для десериализации JSON файла опций семейства
/// </summary>
public class FamilyOptionsCollectionDto
{
    /// <summary>
    /// Список всех опций семейства
    /// </summary>
    public List<FamilyOptionItemDto> FamilyOptions { get; set; } = new();
}

/// <summary>
/// DTO для одной опции семейства
/// </summary>
public class FamilyOptionItemDto
{
    /// <summary>
    /// Уникальный идентификатор опции семейства
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Список свойств опций
    /// </summary>
    public List<OptionPropertyDto> OptionProperties { get; set; } = new();
}

/// <summary>
/// DTO для свойства опции
/// </summary>
public class OptionPropertyDto
{
    /// <summary>
    /// Уникальный идентификатор свойства опции
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Имя свойства
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Пояснение
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Тип свойства (строка, число, перечисление)
    /// </summary>
    public string ValueType { get; set; } = "String";

    /// <summary>
    /// Имя категории
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Значение (может быть строкой, числом или значением перечисления)
    /// </summary>
    public object? Value { get; set; }
}

