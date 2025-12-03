namespace SharpExpo.Contracts.Models;

/// <summary>
/// Опция семейства BIM-компонента
/// Неразделимый, но пополняемый список OptionProperty
/// </summary>
public class FamilyOption
{
    /// <summary>
    /// Уникальный идентификатор опции семейства
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Список свойств опций, входящих в это семейство
    /// </summary>
    public List<OptionProperty> OptionProperties { get; set; } = new();

    /// <summary>
    /// Валидация опции семейства
    /// </summary>
    /// <returns>True, если опция валидна</returns>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            return false;
        }

        if (OptionProperties == null || OptionProperties.Count == 0)
        {
            return false;
        }

        // Проверяем, что все свойства опций валидны
        return OptionProperties.All(op => op.ValidateValue());
    }
}

