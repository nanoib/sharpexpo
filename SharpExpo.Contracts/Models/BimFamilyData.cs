using System.Collections.Generic;
using System.Linq;

namespace SharpExpo.Contracts.Models;

/// <summary>
/// Полные данные BIM-семейства для отображения в панели
/// Содержит BIM-семейство с разрешенными FamilyOption
/// </summary>
public class BimFamilyData
{
    /// <summary>
    /// BIM-семейство
    /// </summary>
    public BimFamily Family { get; set; } = null!;

    /// <summary>
    /// Словарь опций семейства по их Id
    /// </summary>
    public Dictionary<string, FamilyOption> FamilyOptions { get; set; } = new();

    /// <summary>
    /// Получает все OptionProperty, отсортированные по порядку категорий
    /// </summary>
    /// <returns>Список OptionProperty, сгруппированный по категориям в нужном порядке</returns>
    public Dictionary<string, List<OptionProperty>> GetOrderedOptionsByCategory()
    {
        var result = new Dictionary<string, List<OptionProperty>>();

        // Собираем все OptionProperty из всех FamilyOption
        var allOptionProperties = new List<OptionProperty>();
        foreach (var familyOptionId in Family.FamilyOptionIds)
        {
            if (FamilyOptions.TryGetValue(familyOptionId, out var familyOption))
            {
                allOptionProperties.AddRange(familyOption.OptionProperties);
            }
        }

        // Группируем по категориям
        var groupedByCategory = allOptionProperties
            .GroupBy(op => op.CategoryName)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Сортируем по порядку категорий из Family.CategoryOrder
        var orderedCategories = new List<string>();
        
        // Сначала добавляем категории в порядке, указанном в CategoryOrder
        foreach (var categoryName in Family.CategoryOrder)
        {
            if (groupedByCategory.ContainsKey(categoryName))
            {
                orderedCategories.Add(categoryName);
                result[categoryName] = groupedByCategory[categoryName];
            }
        }

        // Затем добавляем категории, которых нет в CategoryOrder (если такие есть)
        foreach (var categoryName in groupedByCategory.Keys)
        {
            if (!orderedCategories.Contains(categoryName))
            {
                orderedCategories.Add(categoryName);
                result[categoryName] = groupedByCategory[categoryName];
            }
        }

        return result;
    }

    /// <summary>
    /// Валидация данных семейства
    /// </summary>
    /// <returns>True, если данные валидны</returns>
    public bool Validate()
    {
        if (Family == null || !Family.Validate())
        {
            return false;
        }

        // Проверяем, что все FamilyOptionIds имеют соответствующие FamilyOption
        foreach (var optionId in Family.FamilyOptionIds)
        {
            if (!FamilyOptions.ContainsKey(optionId))
            {
                return false;
            }

            if (!FamilyOptions[optionId].Validate())
            {
                return false;
            }
        }

        return true;
    }
}

