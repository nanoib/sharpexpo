using System.Text.Json;
using SharpExpo.Contracts;
using SharpExpo.Contracts.DTOs;
using SharpExpo.Contracts.Models;

namespace SharpExpo.Family;

/// <summary>
/// Реализация провайдера данных BIM-семейств из JSON файлов
/// </summary>
public class JsonBimFamilyDataProvider : IBimFamilyDataProvider
{
    private readonly string _familiesDirectory;
    private readonly string _familyOptionsFilePath;
    private Dictionary<string, FamilyOption>? _cachedFamilyOptions;

    public JsonBimFamilyDataProvider(string familiesDirectory, string familyOptionsFilePath)
    {
        _familiesDirectory = familiesDirectory ?? throw new ArgumentNullException(nameof(familiesDirectory));
        _familyOptionsFilePath = familyOptionsFilePath ?? throw new ArgumentNullException(nameof(familyOptionsFilePath));
    }

    public async Task<BimFamilyData?> LoadFamilyDataAsync(string familyId)
    {
        // Загружаем кэш опций семейства, если еще не загружен
        if (_cachedFamilyOptions == null)
        {
            await LoadFamilyOptionsCacheAsync();
        }

        // Загружаем данные семейства
        var familyFilePath = Path.Combine(_familiesDirectory, $"{familyId}.json");
        if (!File.Exists(familyFilePath))
        {
            return null;
        }

        var familyJson = await File.ReadAllTextAsync(familyFilePath);
        var familyDto = JsonSerializer.Deserialize<BimFamilyDto>(familyJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (familyDto == null)
        {
            return null;
        }

        // Преобразуем DTO в модель
        var family = new BimFamily
        {
            Id = familyDto.Id,
            Name = familyDto.Name,
            FamilyOptionIds = familyDto.FamilyOptionIds,
            CategoryOrder = familyDto.CategoryOrder
        };

        // Загружаем соответствующие FamilyOption
        var familyOptions = new Dictionary<string, FamilyOption>();
        foreach (var optionId in family.FamilyOptionIds)
        {
            if (_cachedFamilyOptions!.TryGetValue(optionId, out var option))
            {
                familyOptions[optionId] = option;
            }
        }

        return new BimFamilyData
        {
            Family = family,
            FamilyOptions = familyOptions
        };
    }

    public BimFamilyData? LoadFamilyData(string familyId)
    {
        // Загружаем кэш опций семейства, если еще не загружен
        if (_cachedFamilyOptions == null)
        {
            LoadFamilyOptionsCache();
        }

        // Загружаем данные семейства
        var familyFilePath = Path.Combine(_familiesDirectory, $"{familyId}.json");
        if (!File.Exists(familyFilePath))
        {
            return null;
        }

        var familyJson = File.ReadAllText(familyFilePath);
        var familyDto = JsonSerializer.Deserialize<BimFamilyDto>(familyJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (familyDto == null)
        {
            return null;
        }

        // Преобразуем DTO в модель
        var family = new BimFamily
        {
            Id = familyDto.Id,
            Name = familyDto.Name,
            FamilyOptionIds = familyDto.FamilyOptionIds,
            CategoryOrder = familyDto.CategoryOrder
        };

        // Загружаем соответствующие FamilyOption
        var familyOptions = new Dictionary<string, FamilyOption>();
        foreach (var optionId in family.FamilyOptionIds)
        {
            if (_cachedFamilyOptions!.TryGetValue(optionId, out var option))
            {
                familyOptions[optionId] = option;
            }
        }

        return new BimFamilyData
        {
            Family = family,
            FamilyOptions = familyOptions
        };
    }

    public async Task<IEnumerable<BimFamily>> LoadAllFamiliesAsync()
    {
        if (!Directory.Exists(_familiesDirectory))
        {
            return new List<BimFamily>();
        }

        var families = new List<BimFamily>();
        var jsonFiles = Directory.GetFiles(_familiesDirectory, "*.json");

        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var jsonContent = await File.ReadAllTextAsync(jsonFile);
                var familyDto = JsonSerializer.Deserialize<BimFamilyDto>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (familyDto != null)
                {
                    families.Add(new BimFamily
                    {
                        Id = familyDto.Id,
                        Name = familyDto.Name,
                        FamilyOptionIds = familyDto.FamilyOptionIds,
                        CategoryOrder = familyDto.CategoryOrder
                    });
                }
            }
            catch
            {
                // Игнорируем файлы с ошибками
            }
        }

        return families;
    }

    public IEnumerable<BimFamily> LoadAllFamilies()
    {
        if (!Directory.Exists(_familiesDirectory))
        {
            return new List<BimFamily>();
        }

        var families = new List<BimFamily>();
        var jsonFiles = Directory.GetFiles(_familiesDirectory, "*.json");

        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var jsonContent = File.ReadAllText(jsonFile);
                var familyDto = JsonSerializer.Deserialize<BimFamilyDto>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (familyDto != null)
                {
                    families.Add(new BimFamily
                    {
                        Id = familyDto.Id,
                        Name = familyDto.Name,
                        FamilyOptionIds = familyDto.FamilyOptionIds,
                        CategoryOrder = familyDto.CategoryOrder
                    });
                }
            }
            catch
            {
                // Игнорируем файлы с ошибками
            }
        }

        return families;
    }

    private async Task LoadFamilyOptionsCacheAsync()
    {
        if (!File.Exists(_familyOptionsFilePath))
        {
            _cachedFamilyOptions = new Dictionary<string, FamilyOption>();
            return;
        }

        var jsonContent = await File.ReadAllTextAsync(_familyOptionsFilePath);
        var optionsDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        _cachedFamilyOptions = new Dictionary<string, FamilyOption>();

        if (optionsDto?.FamilyOptions != null)
        {
            foreach (var optionDto in optionsDto.FamilyOptions)
            {
                var familyOption = ConvertToFamilyOption(optionDto);
                if (familyOption != null)
                {
                    _cachedFamilyOptions[familyOption.Id] = familyOption;
                }
            }
        }
    }

    private void LoadFamilyOptionsCache()
    {
        if (!File.Exists(_familyOptionsFilePath))
        {
            _cachedFamilyOptions = new Dictionary<string, FamilyOption>();
            return;
        }

        var jsonContent = File.ReadAllText(_familyOptionsFilePath);
        var optionsDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        _cachedFamilyOptions = new Dictionary<string, FamilyOption>();

        if (optionsDto?.FamilyOptions != null)
        {
            foreach (var optionDto in optionsDto.FamilyOptions)
            {
                var familyOption = ConvertToFamilyOption(optionDto);
                if (familyOption != null)
                {
                    _cachedFamilyOptions[familyOption.Id] = familyOption;
                }
            }
        }
    }

    private FamilyOption? ConvertToFamilyOption(FamilyOptionItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            return null;
        }

        var familyOption = new FamilyOption
        {
            Id = dto.Id,
            OptionProperties = new List<OptionProperty>()
        };

        if (dto.OptionProperties != null)
        {
            foreach (var valueDto in dto.OptionProperties)
            {
                var optionProperty = ConvertToOptionProperty(valueDto);
                if (optionProperty != null)
                {
                    familyOption.OptionProperties.Add(optionProperty);
                }
            }
        }

        return familyOption;
    }

    private OptionProperty? ConvertToOptionProperty(OptionPropertyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.PropertyName))
        {
            return null;
        }

        // Парсим тип значения
        var valueType = ParseValueType(dto.ValueType);
        if (valueType == null)
        {
            return null;
        }

        var optionProperty = new OptionProperty
        {
            Id = dto.Id,
            PropertyName = dto.PropertyName,
            Description = dto.Description,
            ValueType = valueType.Value,
            CategoryName = dto.CategoryName ?? string.Empty
        };

        // Устанавливаем значение в зависимости от типа
        switch (valueType.Value)
        {
            case OptionValueType.String:
                // Для String типа null преобразуем в пустую строку
                optionProperty.StringValue = dto.Value?.ToString() ?? string.Empty;
                break;

            case OptionValueType.Double:
                if (dto.Value != null)
                {
                    if (double.TryParse(dto.Value.ToString(), out var doubleVal))
                    {
                        optionProperty.DoubleValue = doubleVal;
                    }
                }
                break;

            case OptionValueType.Enumeration:
                // Для Enumeration типа null преобразуем в пустую строку
                optionProperty.EnumValue = dto.Value?.ToString() ?? string.Empty;
                break;
        }

        return optionProperty;
    }

    private OptionValueType? ParseValueType(string? valueTypeString)
    {
        if (string.IsNullOrWhiteSpace(valueTypeString))
        {
            return OptionValueType.String;
        }

        return valueTypeString.ToLowerInvariant() switch
        {
            "string" or "строка" => OptionValueType.String,
            "double" or "число" or "number" => OptionValueType.Double,
            "enumeration" or "перечисление" or "enum" => OptionValueType.Enumeration,
            _ => null
        };
    }
}

