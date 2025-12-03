using SharpExpo.Contracts.Models;

namespace SharpExpo.Contracts;

/// <summary>
/// Интерфейс для предоставления данных BIM компонентов
/// </summary>
public interface IBimDataProvider
{
    /// <summary>
    /// Загружает данные BIM компонента из источника данных
    /// </summary>
    /// <returns>Список BIM компонентов</returns>
    Task<IEnumerable<BimComponent>> LoadComponentsAsync();

    /// <summary>
    /// Загружает данные BIM компонента из источника данных синхронно
    /// </summary>
    /// <returns>Список BIM компонентов</returns>
    IEnumerable<BimComponent> LoadComponents();
}

