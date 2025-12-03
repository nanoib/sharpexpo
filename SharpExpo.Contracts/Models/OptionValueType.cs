namespace SharpExpo.Contracts.Models;

/// <summary>
/// Тип значения свойства OptionValue
/// </summary>
public enum OptionValueType
{
    /// <summary>
    /// Строковое значение
    /// </summary>
    String = 0,

    /// <summary>
    /// Число с плавающей точкой
    /// </summary>
    Double = 1,

    /// <summary>
    /// Перечисление (enum)
    /// </summary>
    Enumeration = 2
}

