namespace SharpExpo.Contracts.Models;

/// <summary>
/// Свойство опции BIM-компонента
/// </summary>
public class OptionProperty
{
    /// <summary>
    /// Уникальный идентификатор свойства опции
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Имя свойства, отображаемое в первом столбце панели BIM-свойств
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Пояснение к свойству (внутренняя характеристика, не отображается в UI)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Тип свойства, определяющий допустимые значения
    /// </summary>
    public OptionValueType ValueType { get; set; }

    /// <summary>
    /// Имя категории, к которой относится это свойство
    /// Определяет группировку свойств в панели BIM-свойств
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Текущее значение свойства (строка)
    /// </summary>
    public string? StringValue { get; set; }

    /// <summary>
    /// Текущее значение свойства (число)
    /// </summary>
    public double? DoubleValue { get; set; }

    /// <summary>
    /// Текущее значение свойства (перечисление)
    /// </summary>
    public string? EnumValue { get; set; }

    /// <summary>
    /// Валидация значения в соответствии с типом свойства
    /// </summary>
    /// <returns>True, если значение соответствует типу</returns>
    public bool ValidateValue()
    {
        return ValueType switch
        {
            // Для String разрешаем пустые строки (но не null)
            OptionValueType.String => StringValue != null,
            // Для Double требуется наличие значения
            OptionValueType.Double => DoubleValue.HasValue,
            // Для Enumeration разрешаем пустые строки (но не null)
            OptionValueType.Enumeration => EnumValue != null,
            _ => false
        };
    }

    /// <summary>
    /// Получает строковое представление значения для отображения
    /// </summary>
    public string GetDisplayValue()
    {
        return ValueType switch
        {
            OptionValueType.String => StringValue ?? string.Empty,
            OptionValueType.Double => DoubleValue?.ToString("F2") ?? string.Empty,
            OptionValueType.Enumeration => EnumValue ?? string.Empty,
            _ => string.Empty
        };
    }
}


