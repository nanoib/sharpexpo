namespace SharpExpo.Contracts.Models;

/// <summary>
/// BIM-семейство компонентов
/// Представляет один BIM-компонент с его опциями
/// </summary>
public class BimFamily
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
    /// Список опций семейства, входящих в это BIM-семейство
    /// </summary>
    public List<string> FamilyOptionIds { get; set; } = new();

    /// <summary>
    /// Порядок категорий для отображения в панели BIM-свойств
    /// Определяет последовательность группировки свойств
    /// </summary>
    public List<string> CategoryOrder { get; set; } = new();

    /// <summary>
    /// Валидация BIM-семейства
    /// </summary>
    /// <returns>True, если семейство валидно</returns>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            return false;
        }

        if (FamilyOptionIds == null || FamilyOptionIds.Count == 0)
        {
            return false;
        }

        return true;
    }
}

