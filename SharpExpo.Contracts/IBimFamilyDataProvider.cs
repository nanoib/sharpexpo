using SharpExpo.Contracts.Models;

namespace SharpExpo.Contracts;

/// <summary>
/// Интерфейс для предоставления данных BIM-семейств
/// </summary>
public interface IBimFamilyDataProvider
{
    /// <summary>
    /// Загружает данные конкретного BIM-семейства по его Id
    /// </summary>
    /// <param name="familyId">Идентификатор BIM-семейства</param>
    /// <returns>Данные BIM-семейства или null, если не найдено</returns>
    Task<BimFamilyData?> LoadFamilyDataAsync(string familyId);

    /// <summary>
    /// Загружает данные конкретного BIM-семейства по его Id синхронно
    /// </summary>
    /// <param name="familyId">Идентификатор BIM-семейства</param>
    /// <returns>Данные BIM-семейства или null, если не найдено</returns>
    BimFamilyData? LoadFamilyData(string familyId);

    /// <summary>
    /// Загружает список всех доступных BIM-семейств
    /// </summary>
    /// <returns>Список идентификаторов и имен BIM-семейств</returns>
    Task<IEnumerable<BimFamily>> LoadAllFamiliesAsync();

    /// <summary>
    /// Загружает список всех доступных BIM-семейств синхронно
    /// </summary>
    /// <returns>Список идентификаторов и имен BIM-семейств</returns>
    IEnumerable<BimFamily> LoadAllFamilies();
}


