using System.Text.Json;
using SharpExpo.Contracts;
using SharpExpo.Contracts.Models;

namespace SharpExpo.Family;

/// <summary>
/// Реализация провайдера данных BIM компонентов из JSON файла
/// </summary>
public class JsonBimDataProvider : IBimDataProvider
{
    private readonly string _jsonFilePath;

    public JsonBimDataProvider(string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath ?? throw new ArgumentNullException(nameof(jsonFilePath));
    }

    public async Task<IEnumerable<BimComponent>> LoadComponentsAsync()
    {
        if (!File.Exists(_jsonFilePath))
        {
            throw new FileNotFoundException($"JSON файл не найден: {_jsonFilePath}");
        }

        var jsonContent = await File.ReadAllTextAsync(_jsonFilePath);
        var components = JsonSerializer.Deserialize<List<BimComponent>>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return components ?? new List<BimComponent>();
    }

    public IEnumerable<BimComponent> LoadComponents()
    {
        if (!File.Exists(_jsonFilePath))
        {
            throw new FileNotFoundException($"JSON файл не найден: {_jsonFilePath}");
        }

        var jsonContent = File.ReadAllText(_jsonFilePath);
        var components = JsonSerializer.Deserialize<List<BimComponent>>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return components ?? new List<BimComponent>();
    }
}

