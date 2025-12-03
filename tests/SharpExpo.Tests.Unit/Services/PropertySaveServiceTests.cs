using System.IO;
using System.Text.Json;
using SharpExpo.Contracts;
using SharpExpo.Contracts.DTOs;
using SharpExpo.Contracts.Models;
using SharpExpo.UI.Services;
using SharpExpo.UI.ViewModels;
using Xunit;

namespace SharpExpo.Tests.Unit.Services;

/// <summary>
/// Unit tests for the <see cref="PropertySaveService"/> class.
/// </summary>
public class PropertySaveServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testFamilyOptionsPath;
    private readonly IFileService _fileService;
    private readonly ILogger _logger;
    private readonly IMessageService _messageService;
    private readonly MockDataProvider _dataProvider;

    public PropertySaveServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"test-propertysave-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _testFamilyOptionsPath = Path.Combine(_testDirectory, "family-options.json");
        _fileService = new FileService();
        _logger = new Logger(Path.Combine(_testDirectory, "test-log.txt"));
        _messageService = new MessageService();
        _dataProvider = new MockDataProvider();
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange & Act
        var service = new PropertySaveService(_fileService, _logger, _messageService, _dataProvider);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullFileService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PropertySaveService(null!, _logger, _messageService, _dataProvider));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PropertySaveService(_fileService, null!, _messageService, _dataProvider));
    }

    [Fact]
    public void Constructor_WithNullMessageService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PropertySaveService(_fileService, _logger, null!, _dataProvider));
    }

    [Fact]
    public void Constructor_WithNullDataProvider_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PropertySaveService(_fileService, _logger, _messageService, null!));
    }

    [Fact]
    public async Task SavePropertyValueAsync_WithStringProperty_SavesValue()
    {
        // Arrange
        var service = new PropertySaveService(_fileService, _logger, _messageService, _dataProvider);
        var initialJson = CreateTestFamilyOptionsJson("option1", "prop1", "String", "Old Value");
        await File.WriteAllTextAsync(_testFamilyOptionsPath, initialJson);

        var propertyRow = CreatePropertyRowViewModel("prop1", "option1", OptionValueType.String, "Old Value");
        var newValue = "New Value";

        // Act
        await service.SavePropertyValueAsync(propertyRow, newValue, _testFamilyOptionsPath);

        // Assert
        var savedContent = await File.ReadAllTextAsync(_testFamilyOptionsPath);
        var savedDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(savedContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(savedDto);
        var savedProperty = savedDto!.FamilyOptions
            .SelectMany(fo => fo.OptionProperties)
            .FirstOrDefault(op => op.Id == "prop1");

        Assert.NotNull(savedProperty);
        Assert.Equal("New Value", savedProperty!.Value?.ToString());
        Assert.Equal("New Value", propertyRow.PropertyValue);
        Assert.Equal("New Value", propertyRow.OriginalProperty!.StringValue);
    }

    [Fact]
    public async Task SavePropertyValueAsync_WithDoubleProperty_SavesValue()
    {
        // Arrange
        var service = new PropertySaveService(_fileService, _logger, _messageService, _dataProvider);
        var initialJson = CreateTestFamilyOptionsJson("option1", "prop1", "Double", 123.45);
        await File.WriteAllTextAsync(_testFamilyOptionsPath, initialJson);

        var propertyRow = CreatePropertyRowViewModel("prop1", "option1", OptionValueType.Double, "123.45");
        var newValue = "456.78";

        // Act
        await service.SavePropertyValueAsync(propertyRow, newValue, _testFamilyOptionsPath);

        // Assert
        var savedContent = await File.ReadAllTextAsync(_testFamilyOptionsPath);
        var savedDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(savedContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(savedDto);
        var savedProperty = savedDto!.FamilyOptions
            .SelectMany(fo => fo.OptionProperties)
            .FirstOrDefault(op => op.Id == "prop1");

        Assert.NotNull(savedProperty);
        Assert.True(savedProperty!.Value is double || savedProperty.Value is JsonElement);
        
        double savedDoubleValue;
        if (savedProperty.Value is double d)
        {
            savedDoubleValue = d;
        }
        else if (savedProperty.Value is JsonElement je && je.ValueKind == JsonValueKind.Number)
        {
            savedDoubleValue = je.GetDouble();
        }
        else
        {
            throw new InvalidOperationException("Value is not a number");
        }

        Assert.True(Math.Abs(456.78 - savedDoubleValue) < 0.01);
        Assert.NotNull(propertyRow.OriginalProperty!.DoubleValue);
        Assert.True(Math.Abs(456.78 - propertyRow.OriginalProperty.DoubleValue.Value) < 0.01);
    }

    [Fact]
    public async Task SavePropertyValueAsync_WithInvalidDouble_DoesNotSaveValue()
    {
        // Arrange
        var service = new PropertySaveService(_fileService, _logger, _messageService, _dataProvider);
        var initialJson = CreateTestFamilyOptionsJson("option1", "prop1", "Double", 123.45);
        await File.WriteAllTextAsync(_testFamilyOptionsPath, initialJson);

        var propertyRow = CreatePropertyRowViewModel("prop1", "option1", OptionValueType.Double, "123.45");
        var originalDoubleValue = propertyRow.OriginalProperty!.DoubleValue;
        var invalidValue = "not a number";

        // Act
        // Service catches FormatException and shows error message, doesn't throw
        await service.SavePropertyValueAsync(propertyRow, invalidValue, _testFamilyOptionsPath);

        // Assert - value should not be saved
        var savedContent = await File.ReadAllTextAsync(_testFamilyOptionsPath);
        var savedDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(savedContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(savedDto);
        var savedProperty = savedDto!.FamilyOptions
            .SelectMany(fo => fo.OptionProperties)
            .FirstOrDefault(op => op.Id == "prop1");

        Assert.NotNull(savedProperty);
        // Value should remain as original (123.45), not changed to invalid value
        Assert.True(savedProperty!.Value is double || savedProperty.Value is JsonElement);
        
        double savedDoubleValue;
        if (savedProperty.Value is double d)
        {
            savedDoubleValue = d;
        }
        else if (savedProperty.Value is JsonElement je && je.ValueKind == JsonValueKind.Number)
        {
            savedDoubleValue = je.GetDouble();
        }
        else
        {
            throw new InvalidOperationException("Value is not a number");
        }

        Assert.True(Math.Abs(123.45 - savedDoubleValue) < 0.01);
        // Original property should not be changed
        Assert.Equal(originalDoubleValue, propertyRow.OriginalProperty.DoubleValue);
    }

    [Fact]
    public async Task SavePropertyValueAsync_WithNullPropertyRow_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new PropertySaveService(_fileService, _logger, _messageService, _dataProvider);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.SavePropertyValueAsync(null!, "value", _testFamilyOptionsPath));
    }

    [Fact]
    public async Task SavePropertyValueAsync_WithNullNewValue_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new PropertySaveService(_fileService, _logger, _messageService, _dataProvider);
        var propertyRow = CreatePropertyRowViewModel("prop1", "option1", OptionValueType.String, "value");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.SavePropertyValueAsync(propertyRow, null!, _testFamilyOptionsPath));
    }

    [Fact]
    public async Task SavePropertyValueAsync_WithNullFilePath_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new PropertySaveService(_fileService, _logger, _messageService, _dataProvider);
        var propertyRow = CreatePropertyRowViewModel("prop1", "option1", OptionValueType.String, "value");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.SavePropertyValueAsync(propertyRow, "value", null!));
    }

    [Fact]
    public async Task SavePropertyValueAsync_WithNonExistentFile_DoesNotSave()
    {
        // Arrange
        var service = new PropertySaveService(_fileService, _logger, _messageService, _dataProvider);
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.json");
        var propertyRow = CreatePropertyRowViewModel("prop1", "option1", OptionValueType.String, "value");

        // Act
        await service.SavePropertyValueAsync(propertyRow, "new value", nonExistentPath);

        // Assert - Should not throw, but should log error
        Assert.False(File.Exists(nonExistentPath));
    }

    [Fact]
    public async Task SavePropertyValueAsync_WithNullOriginalProperty_DoesNotSave()
    {
        // Arrange
        var service = new PropertySaveService(_fileService, _logger, _messageService, _dataProvider);
        var initialJson = CreateTestFamilyOptionsJson("option1", "prop1", "String", "value");
        await File.WriteAllTextAsync(_testFamilyOptionsPath, initialJson);

        var propertyRow = new PropertyRowViewModel
        {
            PropertyName = "Test Property",
            OriginalProperty = null,
            FamilyOptionId = "option1"
        };

        // Act
        await service.SavePropertyValueAsync(propertyRow, "new value", _testFamilyOptionsPath);

        // Assert - Should not throw, but should not save
        var savedContent = await File.ReadAllTextAsync(_testFamilyOptionsPath);
        var savedDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(savedContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(savedDto);
        var savedProperty = savedDto!.FamilyOptions
            .SelectMany(fo => fo.OptionProperties)
            .FirstOrDefault(op => op.Id == "prop1");

        Assert.NotNull(savedProperty);
        Assert.Equal("value", savedProperty!.Value?.ToString());
    }

    [Fact]
    public async Task SavePropertyValueAsync_ClearsDataProviderCache()
    {
        // Arrange
        var service = new PropertySaveService(_fileService, _logger, _messageService, _dataProvider);
        var initialJson = CreateTestFamilyOptionsJson("option1", "prop1", "String", "value");
        await File.WriteAllTextAsync(_testFamilyOptionsPath, initialJson);

        var propertyRow = CreatePropertyRowViewModel("prop1", "option1", OptionValueType.String, "value");

        // Act
        await service.SavePropertyValueAsync(propertyRow, "new value", _testFamilyOptionsPath);

        // Assert
        Assert.True(_dataProvider.ClearCacheCalled);
    }

    private static string CreateTestFamilyOptionsJson(string optionId, string propertyId, string valueType, object value)
    {
        var dto = new FamilyOptionsCollectionDto
        {
            FamilyOptions = new List<FamilyOptionItemDto>
            {
                new FamilyOptionItemDto
                {
                    Id = optionId,
                    OptionProperties = new List<OptionPropertyDto>
                    {
                        new OptionPropertyDto
                        {
                            Id = propertyId,
                            PropertyName = "Test Property",
                            ValueType = valueType,
                            Value = value
                        }
                    }
                }
            }
        };

        return JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null
        });
    }

    private static PropertyRowViewModel CreatePropertyRowViewModel(string propertyId, string familyOptionId, OptionValueType valueType, string currentValue)
    {
        var optionProperty = new OptionProperty
        {
            Id = propertyId,
            PropertyName = "Test Property",
            ValueType = valueType
        };

        if (valueType == OptionValueType.String)
        {
            optionProperty.StringValue = currentValue;
        }
        else if (valueType == OptionValueType.Double)
        {
            if (double.TryParse(currentValue, out var doubleValue))
            {
                optionProperty.DoubleValue = doubleValue;
            }
        }

        return new PropertyRowViewModel
        {
            PropertyName = "Test Property",
            PropertyValue = currentValue,
            OriginalProperty = optionProperty,
            FamilyOptionId = familyOptionId,
            ValueType = valueType
        };
    }

    private class MockDataProvider : IBimFamilyDataProvider
    {
        public bool ClearCacheCalled { get; private set; }

        public void ClearCache()
        {
            ClearCacheCalled = true;
        }

        public Task<BimFamilyData?> LoadFamilyDataAsync(string familyId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BimFamily>> LoadAllFamiliesAsync()
        {
            throw new NotImplementedException();
        }

        public BimFamilyData? LoadFamilyData(string familyId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BimFamily> LoadAllFamilies()
        {
            throw new NotImplementedException();
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}

