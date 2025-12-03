using System.Text.Json;
using SharpExpo.Contracts;
using SharpExpo.Contracts.DTOs;
using SharpExpo.Contracts.Models;
using SharpExpo.Family.Mappers;

namespace SharpExpo.Family;

/// <summary>
/// JSON-based implementation of <see cref="IBimFamilyDataProvider"/> that loads BIM family data from JSON files.
/// </summary>
/// <remarks>
/// WHY: This class implements IBimFamilyDataProvider to provide data access for BIM families.
/// It uses caching to improve performance when loading multiple families and delegates mapping to IDtoMapper
/// to follow the Single Responsibility Principle.
/// </remarks>
public class JsonBimFamilyDataProvider : IBimFamilyDataProvider
{
    private readonly string _familiesDirectory;
    private readonly string _familyOptionsFilePath;
    private readonly IDtoMapper _mapper;
    private Dictionary<string, FamilyOption>? _cachedFamilyOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonBimFamilyDataProvider"/> class.
    /// </summary>
    /// <param name="familiesDirectory">The directory containing family JSON files.</param>
    /// <param name="familyOptionsFilePath">The path to the family-options.json file.</param>
    /// <param name="mapper">The mapper to use for converting DTOs to domain models. If <see langword="null"/>, uses a default mapper.</param>
    public JsonBimFamilyDataProvider(string familiesDirectory, string familyOptionsFilePath, IDtoMapper? mapper = null)
    {
        _familiesDirectory = Path.GetFullPath(familiesDirectory ?? throw new ArgumentNullException(nameof(familiesDirectory)));
        _familyOptionsFilePath = Path.GetFullPath(familyOptionsFilePath ?? throw new ArgumentNullException(nameof(familyOptionsFilePath)));
        _mapper = mapper ?? new DtoMapper();
        
        System.Diagnostics.Debug.WriteLine($"[JsonBimFamilyDataProvider] Инициализация:");
        System.Diagnostics.Debug.WriteLine($"[JsonBimFamilyDataProvider]   FamiliesDirectory: {_familiesDirectory}");
        System.Diagnostics.Debug.WriteLine($"[JsonBimFamilyDataProvider]   FamilyOptionsFilePath: {_familyOptionsFilePath}");
    }

    /// <inheritdoc/>
    public async Task<BimFamilyData?> LoadFamilyDataAsync(string familyId)
    {
        ArgumentNullException.ThrowIfNull(familyId);

        // Load family options cache if not already loaded
        if (_cachedFamilyOptions == null)
        {
            await LoadFamilyOptionsCacheAsync();
        }

        // Load family data
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

        var family = _mapper.MapToBimFamily(familyDto);
        if (family == null)
        {
            return null;
        }

        // Load corresponding FamilyOptions
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

    /// <inheritdoc/>
    public BimFamilyData? LoadFamilyData(string familyId)
    {
        ArgumentNullException.ThrowIfNull(familyId);

        // Load family options cache if not already loaded
        if (_cachedFamilyOptions == null)
        {
            LoadFamilyOptionsCache();
        }

        // Load family data
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

        var family = _mapper.MapToBimFamily(familyDto);
        if (family == null)
        {
            return null;
        }

        // Load corresponding FamilyOptions
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

    /// <inheritdoc/>
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

                var family = _mapper.MapToBimFamily(familyDto);
                if (family != null)
                {
                    families.Add(family);
                }
            }
            catch
            {
                // WHY: We silently ignore files with errors to allow loading other families even if some files are corrupted.
                // In production, this could be enhanced with logging or error reporting.
            }
        }

        return families;
    }

    /// <inheritdoc/>
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

                var family = _mapper.MapToBimFamily(familyDto);
                if (family != null)
                {
                    families.Add(family);
                }
            }
            catch
            {
                // WHY: We silently ignore files with errors to allow loading other families even if some files are corrupted.
                // In production, this could be enhanced with logging or error reporting.
            }
        }

        return families;
    }

    /// <inheritdoc/>
    public void ClearCache()
    {
        _cachedFamilyOptions = null;
    }

    /// <summary>
    /// Loads the family options cache asynchronously from the family-options.json file.
    /// </summary>
    /// <returns>A task that represents the asynchronous cache loading operation.</returns>
    /// <remarks>
    /// WHY: This method loads and caches family options to improve performance when loading multiple families.
    /// The cache is stored in memory and can be cleared using <see cref="ClearCache"/>.
    /// </remarks>
    private async Task LoadFamilyOptionsCacheAsync()
    {
        var absolutePath = Path.GetFullPath(_familyOptionsFilePath);
        System.Diagnostics.Debug.WriteLine($"[JsonBimFamilyDataProvider] Загрузка кэша из: {absolutePath}");
        
        if (!File.Exists(absolutePath))
        {
            System.Diagnostics.Debug.WriteLine($"[JsonBimFamilyDataProvider] Файл не найден: {absolutePath}");
            _cachedFamilyOptions = new Dictionary<string, FamilyOption>();
            return;
        }

        var fileInfo = new FileInfo(absolutePath);
        fileInfo.Refresh();
        System.Diagnostics.Debug.WriteLine($"[JsonBimFamilyDataProvider] Файл найден, размер: {fileInfo.Length} байт, последнее изменение: {fileInfo.LastWriteTime}");

        var jsonContent = await File.ReadAllTextAsync(absolutePath);
        var optionsDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        _cachedFamilyOptions = new Dictionary<string, FamilyOption>();

        if (optionsDto?.FamilyOptions != null)
        {
            foreach (var optionDto in optionsDto.FamilyOptions)
            {
                var familyOption = _mapper.MapToFamilyOption(optionDto);
                if (familyOption != null)
                {
                    _cachedFamilyOptions[familyOption.Id] = familyOption;
                }
            }
        }
    }

    /// <summary>
    /// Loads the family options cache synchronously from the family-options.json file.
    /// </summary>
    /// <remarks>
    /// WHY: This method provides a synchronous alternative to <see cref="LoadFamilyOptionsCacheAsync"/> for scenarios
    /// where async operations are not available or desired.
    /// </remarks>
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
                var familyOption = _mapper.MapToFamilyOption(optionDto);
                if (familyOption != null)
                {
                    _cachedFamilyOptions[familyOption.Id] = familyOption;
                }
            }
        }
    }

}

