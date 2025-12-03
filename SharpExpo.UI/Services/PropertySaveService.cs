using System.Text;
using System.Text.Json;
using SharpExpo.Contracts;
using SharpExpo.Contracts.DTOs;
using SharpExpo.Contracts.Models;
using SharpExpo.UI.ViewModels;

namespace SharpExpo.UI.Services;

/// <summary>
/// Default implementation of <see cref="IPropertySaveService"/> that saves property values to JSON files.
/// </summary>
/// <remarks>
/// WHY: This class implements IPropertySaveService to separate file saving logic from the ViewModel.
/// This follows the Single Responsibility Principle and makes the saving logic testable independently.
/// </remarks>
public class PropertySaveService : IPropertySaveService
{
    private readonly IFileService _fileService;
    private readonly ILogger _logger;
    private readonly IMessageService _messageService;
    private readonly IBimFamilyDataProvider _dataProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertySaveService"/> class.
    /// </summary>
    /// <param name="fileService">The file service to use for file operations.</param>
    /// <param name="logger">The logger to use for logging operations.</param>
    /// <param name="messageService">The message service to use for displaying messages to the user.</param>
    /// <param name="dataProvider">The data provider to clear cache after saving.</param>
    public PropertySaveService(
        IFileService fileService,
        ILogger logger,
        IMessageService messageService,
        IBimFamilyDataProvider dataProvider)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
    }

    /// <inheritdoc/>
    public async Task SavePropertyValueAsync(PropertyRowViewModel propertyRow, string newValue, string familyOptionsFilePath)
    {
        ArgumentNullException.ThrowIfNull(propertyRow);
        ArgumentNullException.ThrowIfNull(newValue);
        ArgumentNullException.ThrowIfNull(familyOptionsFilePath);

        _logger.Log($"=== SavePropertyValueAsync ВЫЗВАН ===");
        _logger.Log($"PropertyName: {propertyRow.PropertyName}");
        _logger.Log($"NewValue: '{newValue}'");
        _logger.Log($"OriginalProperty: {(propertyRow.OriginalProperty != null ? "не null" : "null")}");
        _logger.Log($"FamilyOptionId: '{propertyRow.FamilyOptionId}'");

        if (propertyRow.OriginalProperty == null || string.IsNullOrEmpty(propertyRow.FamilyOptionId))
        {
            _logger.LogError("Не удалось сохранить: отсутствует OriginalProperty или FamilyOptionId", null);
            return;
        }

        try
        {
            var absolutePath = _fileService.GetFullPath(familyOptionsFilePath);
            _logger.Log($"Начало сохранения: PropertyId={propertyRow.OriginalProperty.Id}, FamilyOptionId={propertyRow.FamilyOptionId}, NewValue={newValue}");
            _logger.Log($"Путь к файлу: {absolutePath}");
            _logger.Log($"Файл существует: {_fileService.FileExists(absolutePath)}");

            if (!_fileService.FileExists(absolutePath))
            {
                _logger.LogError($"Файл не найден для загрузки: {absolutePath}", null);
                return;
            }

            // Load current JSON
            var jsonContent = await _fileService.ReadAllTextAsync(absolutePath);
            _logger.Log($"Размер загруженного JSON: {jsonContent.Length} байт");

            var optionsDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(
                jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (optionsDto == null)
            {
                _logger.LogError("Не удалось десериализовать family-options.json", null);
                return;
            }

            // Find the FamilyOption and OptionProperty
            var familyOption = optionsDto.FamilyOptions.FirstOrDefault(fo => fo.Id == propertyRow.FamilyOptionId);
            if (familyOption == null)
            {
                _logger.LogError($"FamilyOption с ID {propertyRow.FamilyOptionId} не найден", null);
                return;
            }

            var optionPropertyDto = familyOption.OptionProperties.FirstOrDefault(op => op.Id == propertyRow.OriginalProperty.Id);
            if (optionPropertyDto == null)
            {
                _logger.LogError($"OptionProperty с ID {propertyRow.OriginalProperty.Id} не найден", null);
                return;
            }

            // Update value based on type
            UpdatePropertyValue(propertyRow, newValue, optionPropertyDto);

            // Save back to JSON (using PascalCase, as in original file)
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null, // Use original property names (PascalCase)
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Preserve Cyrillic
            };

            var updatedJson = JsonSerializer.Serialize(optionsDto, jsonOptions);
            _logger.Log($"Размер JSON для записи: {updatedJson.Length} байт");

            // Save file
            await _fileService.WriteAllTextAsync(absolutePath, updatedJson);
            _logger.Log($"Файл записан: {absolutePath}");

            // Verify save
            await VerifySaveAsync(absolutePath, propertyRow, newValue);

            // Clear provider cache
            _dataProvider.ClearCache();

            // Update displayed value
            UpdateDisplayValue(propertyRow, newValue);

            _logger.Log($"=== СОХРАНЕНИЕ ЗАВЕРШЕНО ===");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при сохранении значения свойства", ex);
            _messageService.ShowError($"Ошибка при сохранении: {ex.Message}", "Ошибка");
        }
    }

    /// <summary>
    /// Updates the property value in the DTO and the original property model.
    /// </summary>
    /// <param name="propertyRow">The property row view model.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <param name="optionPropertyDto">The DTO to update.</param>
    /// <remarks>
    /// WHY: This method handles type-specific value updates, ensuring both the DTO and the model are updated consistently.
    /// </remarks>
    private static void UpdatePropertyValue(PropertyRowViewModel propertyRow, string newValue, OptionPropertyDto optionPropertyDto)
    {
        if (propertyRow.ValueType == OptionValueType.String)
        {
            var oldValue = optionPropertyDto.Value?.ToString() ?? "null";
            optionPropertyDto.Value = newValue;
            propertyRow.OriginalProperty!.StringValue = newValue;
            propertyRow.PropertyValue = newValue;
        }
        else if (propertyRow.ValueType == OptionValueType.Double)
        {
            if (double.TryParse(newValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var doubleValue))
            {
                optionPropertyDto.Value = doubleValue;
                propertyRow.OriginalProperty!.DoubleValue = doubleValue;
            }
            else
            {
                throw new FormatException($"Неверный формат числа: {newValue}");
            }
        }
    }

    /// <summary>
    /// Verifies that the save operation was successful by reading the file back.
    /// </summary>
    /// <param name="filePath">The path to the file to verify.</param>
    /// <param name="propertyRow">The property row view model.</param>
    /// <param name="expectedValue">The expected value that should be in the file.</param>
    /// <returns>A task that represents the asynchronous verification operation.</returns>
    /// <remarks>
    /// WHY: This method provides verification to ensure data integrity after save operations.
    /// </remarks>
    private async Task VerifySaveAsync(string filePath, PropertyRowViewModel propertyRow, string expectedValue)
    {
        try
        {
            await Task.Delay(100); // Small delay to ensure file write completion
            var savedContent = await _fileService.ReadAllTextAsync(filePath);

            var verificationDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(
                savedContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (verificationDto != null)
            {
                var savedProp = verificationDto.FamilyOptions
                    .SelectMany(fo => fo.OptionProperties)
                    .FirstOrDefault(op => op.Id == propertyRow.OriginalProperty!.Id);

                if (savedProp != null)
                {
                    var savedValue = savedProp.Value?.ToString();
                    if (savedValue != expectedValue)
                    {
                        _logger.LogError($"⚠ ВНИМАНИЕ: Сохраненное значение не совпадает с ожидаемым! Ожидалось: '{expectedValue}', Сохранено: '{savedValue}'", null);
                        _messageService.ShowWarning(
                            $"ВНИМАНИЕ: Сохраненное значение не совпадает с ожидаемым!\n\n" +
                            $"Ожидалось: {expectedValue}\n" +
                            $"Сохранено: {savedValue}\n\n" +
                            $"Проверьте лог-файл: {_logger.LogFilePath}",
                            "Предупреждение");
                    }
                    else
                    {
                        _logger.Log($"✓ Значение успешно сохранено и проверено!");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при проверке сохранения", ex);
        }
    }

    /// <summary>
    /// Updates the displayed value in the property row view model.
    /// </summary>
    /// <param name="propertyRow">The property row view model to update.</param>
    /// <param name="newValue">The new value to display.</param>
    private static void UpdateDisplayValue(PropertyRowViewModel propertyRow, string newValue)
    {
        if (propertyRow.ValueType == OptionValueType.String)
        {
            propertyRow.PropertyValue = newValue;
        }
        else if (propertyRow.ValueType == OptionValueType.Double)
        {
            propertyRow.PropertyValue = propertyRow.OriginalProperty!.GetDisplayValue();
        }
    }
}

