using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using SharpExpo.Contracts.DTOs;
using SharpExpo.Contracts.Models;
using SharpExpo.Tests.Unit.Helpers;
using SharpExpo.UI.Commands;
using SharpExpo.UI.ViewModels;
using Xunit;

namespace SharpExpo.Tests.Unit.ViewModels;

/// <summary>
/// Тесты для функционала сохранения значений свойств в MainWindowViewModel
/// </summary>
public class MainWindowViewModelSaveTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testFamilyOptionsPath;
    private readonly string _testFamiliesDirectory;

    public MainWindowViewModelSaveTests()
    {
        // Создаем временную директорию для тестов
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        _testFamiliesDirectory = Path.Combine(_testDirectory, "Families");
        Directory.CreateDirectory(_testFamiliesDirectory);
        
        _testFamilyOptionsPath = Path.Combine(_testDirectory, "family-options.json");
        
        // Создаем тестовый файл family-options.json
        CreateTestFamilyOptionsFile();
        
        // Создаем тестовый файл семейства
        CreateTestFamilyFile();
    }

    public void Dispose()
    {
        // Удаляем временную директорию после тестов
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    private void CreateTestFamilyOptionsFile()
    {
        var testData = new FamilyOptionsCollectionDto
        {
            FamilyOptions = new List<FamilyOptionItemDto>
            {
                new FamilyOptionItemDto
                {
                    Id = "option-test",
                    OptionProperties = new List<OptionPropertyDto>
                    {
                        new OptionPropertyDto
                        {
                            Id = "prop-string",
                            PropertyName = "Тестовое свойство",
                            Description = "Описание",
                            ValueType = "String",
                            CategoryName = "Тест",
                            Value = "Исходное значение"
                        },
                        new OptionPropertyDto
                        {
                            Id = "prop-double",
                            PropertyName = "Числовое свойство",
                            Description = "Описание",
                            ValueType = "Double",
                            CategoryName = "Тест",
                            Value = 123.45
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(testData, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null
        });

        File.WriteAllText(_testFamilyOptionsPath, json);
    }

    private void CreateTestFamilyFile()
    {
        var familyId = "test-family-id";
        var familyFile = Path.Combine(_testFamiliesDirectory, $"{familyId}.json");
        
        var familyDto = new BimFamilyDto
        {
            Id = familyId,
            Name = "Тестовое семейство",
            FamilyOptionIds = new List<string> { "option-test" },
            CategoryOrder = new List<string> { "Тест" }
        };

        var json = JsonSerializer.Serialize(familyDto, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null
        });

        File.WriteAllText(familyFile, json);
    }

    [Fact]
    public async Task SavePropertyValueAsync_StringType_UpdatesValueInFile()
    {
        // Arrange
        // Проверяем, что тестовые файлы созданы
        Assert.True(File.Exists(_testFamilyOptionsPath), $"Файл family-options.json не найден: {_testFamilyOptionsPath}");
        var familyFile = Path.Combine(_testFamiliesDirectory, "test-family-id.json");
        Assert.True(File.Exists(familyFile), $"Файл семейства не найден: {familyFile}");
        
        var dataProvider = TestServiceFactory.CreateDataProvider(_testFamiliesDirectory, _testFamilyOptionsPath);
        
        // Проверяем, что семейство можно загрузить
        var testFamilyData = await dataProvider.LoadFamilyDataAsync("test-family-id");
        Assert.NotNull(testFamilyData);
        Assert.True(testFamilyData.Validate(), "Данные семейства не прошли валидацию");
        
        var viewModel = TestServiceFactory.CreateMainWindowViewModel(
            dataProvider,
            "test-family-id",
            _testFamilyOptionsPath,
            _testFamiliesDirectory);
        
        // Загружаем данные через ExecuteAsync
        if (viewModel.LoadCommand is RelayCommand relayCommand)
        {
            await relayCommand.ExecuteAsync(null);
        }
        else
        {
            // Альтернатива: используем reflection для доступа к приватному методу LoadDataAsync
            var loadMethod = typeof(MainWindowViewModel).GetMethod("LoadDataAsync", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (loadMethod != null)
            {
                var loadTask = (Task)loadMethod.Invoke(viewModel, null)!;
                await loadTask;
            }
            else
            {
                // Последний вариант: ждем через Execute
                if (viewModel.LoadCommand.CanExecute(null))
                {
                    viewModel.LoadCommand.Execute(null);
                    await Task.Delay(2000);
                }
            }
        }
        
        // Проверяем, что данные загружены
        Assert.True(viewModel.Properties.Count > 0, $"Свойства не загружены. Количество: {viewModel.Properties.Count}");
        
        // Находим редактируемое свойство
        var propertyRow = viewModel.Properties.FirstOrDefault(p => 
            !p.IsSectionHeader && 
            p.OriginalProperty != null && 
            p.OriginalProperty.Id == "prop-string");
        
        Assert.NotNull(propertyRow);
        Assert.NotNull(propertyRow.OriginalProperty);
        
        var originalValue = propertyRow.PropertyValue;
        var newValue = "Новое значение";

        // Act
        await viewModel.SavePropertyValueAsync(propertyRow, newValue);

        // Assert
        // Проверяем, что файл обновлен
        var savedContent = await File.ReadAllTextAsync(_testFamilyOptionsPath);
        var savedDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(savedContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(savedDto);
        var savedProperty = savedDto.FamilyOptions
            .SelectMany(fo => fo.OptionProperties)
            .FirstOrDefault(op => op.Id == "prop-string");

        Assert.NotNull(savedProperty);
        Assert.Equal(newValue, savedProperty.Value?.ToString());
        
        // Проверяем, что модель обновлена
        Assert.Equal(newValue, propertyRow.OriginalProperty.StringValue);
        Assert.Equal(newValue, propertyRow.PropertyValue);
    }

    [Fact]
    public async Task SavePropertyValueAsync_DoubleType_UpdatesValueInFile()
    {
        // Arrange
        // Проверяем, что тестовые файлы созданы
        Assert.True(File.Exists(_testFamilyOptionsPath), $"Файл family-options.json не найден: {_testFamilyOptionsPath}");
        var familyFile = Path.Combine(_testFamiliesDirectory, "test-family-id.json");
        Assert.True(File.Exists(familyFile), $"Файл семейства не найден: {familyFile}");
        
        var dataProvider = TestServiceFactory.CreateDataProvider(_testFamiliesDirectory, _testFamilyOptionsPath);
        
        // Проверяем, что семейство можно загрузить
        var testFamilyData = await dataProvider.LoadFamilyDataAsync("test-family-id");
        Assert.NotNull(testFamilyData);
        Assert.True(testFamilyData.Validate(), "Данные семейства не прошли валидацию");
        
        var viewModel = TestServiceFactory.CreateMainWindowViewModel(
            dataProvider,
            "test-family-id",
            _testFamilyOptionsPath,
            _testFamiliesDirectory);
        
        // Загружаем данные через ExecuteAsync
        if (viewModel.LoadCommand is RelayCommand relayCommand)
        {
            await relayCommand.ExecuteAsync(null);
        }
        else
        {
            // Альтернатива: используем reflection для доступа к приватному методу LoadDataAsync
            var loadMethod = typeof(MainWindowViewModel).GetMethod("LoadDataAsync", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (loadMethod != null)
            {
                var loadTask = (Task)loadMethod.Invoke(viewModel, null)!;
                await loadTask;
            }
            else
            {
                // Последний вариант: ждем через Execute
                if (viewModel.LoadCommand.CanExecute(null))
                {
                    viewModel.LoadCommand.Execute(null);
                    await Task.Delay(2000);
                }
            }
        }
        
        // Проверяем, что данные загружены
        Assert.True(viewModel.Properties.Count > 0, $"Свойства не загружены. Количество: {viewModel.Properties.Count}");
        
        // Находим редактируемое свойство
        var propertyRow = viewModel.Properties.FirstOrDefault(p => 
            !p.IsSectionHeader && 
            p.OriginalProperty != null && 
            p.OriginalProperty.Id == "prop-double");
        
        Assert.NotNull(propertyRow);
        Assert.NotNull(propertyRow.OriginalProperty);
        
        var newValue = "456.78";

        // Act
        await viewModel.SavePropertyValueAsync(propertyRow, newValue);

        // Assert
        // Проверяем, что файл обновлен
        var savedContent = await File.ReadAllTextAsync(_testFamilyOptionsPath);
        var savedDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(savedContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(savedDto);
        var savedProperty = savedDto.FamilyOptions
            .SelectMany(fo => fo.OptionProperties)
            .FirstOrDefault(op => op.Id == "prop-double");

        Assert.NotNull(savedProperty);
        Assert.True(savedProperty.Value is double || savedProperty.Value is JsonElement);
        
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
            throw new InvalidOperationException("Значение не является числом");
        }
        
        Assert.True(Math.Abs(456.78 - savedDoubleValue) < 0.01);
        
        // Проверяем, что модель обновлена
        Assert.NotNull(propertyRow.OriginalProperty.DoubleValue);
        Assert.True(Math.Abs(456.78 - propertyRow.OriginalProperty.DoubleValue.Value) < 0.01);
    }

    [Fact]
    public async Task SavePropertyValueAsync_InvalidDouble_DoesNotUpdateFile()
    {
        // Arrange
        // Проверяем, что тестовые файлы созданы
        Assert.True(File.Exists(_testFamilyOptionsPath), $"Файл family-options.json не найден: {_testFamilyOptionsPath}");
        var familyFile = Path.Combine(_testFamiliesDirectory, "test-family-id.json");
        Assert.True(File.Exists(familyFile), $"Файл семейства не найден: {familyFile}");
        
        var dataProvider = TestServiceFactory.CreateDataProvider(_testFamiliesDirectory, _testFamilyOptionsPath);
        
        // Проверяем, что семейство можно загрузить
        var testFamilyData = await dataProvider.LoadFamilyDataAsync("test-family-id");
        Assert.NotNull(testFamilyData);
        Assert.True(testFamilyData.Validate(), "Данные семейства не прошли валидацию");
        
        var viewModel = TestServiceFactory.CreateMainWindowViewModel(
            dataProvider,
            "test-family-id",
            _testFamilyOptionsPath,
            _testFamiliesDirectory);
        
        // Загружаем данные через ExecuteAsync
        if (viewModel.LoadCommand is RelayCommand relayCommand)
        {
            await relayCommand.ExecuteAsync(null);
        }
        else
        {
            // Альтернатива: используем reflection для доступа к приватному методу LoadDataAsync
            var loadMethod = typeof(MainWindowViewModel).GetMethod("LoadDataAsync", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (loadMethod != null)
            {
                var loadTask = (Task)loadMethod.Invoke(viewModel, null)!;
                await loadTask;
            }
            else
            {
                // Последний вариант: ждем через Execute
                if (viewModel.LoadCommand.CanExecute(null))
                {
                    viewModel.LoadCommand.Execute(null);
                    await Task.Delay(2000);
                }
            }
        }
        
        // Проверяем, что данные загружены
        Assert.True(viewModel.Properties.Count > 0, $"Свойства не загружены. Количество: {viewModel.Properties.Count}");
        
        // Находим редактируемое свойство
        var propertyRow = viewModel.Properties.FirstOrDefault(p => 
            !p.IsSectionHeader && 
            p.OriginalProperty != null && 
            p.OriginalProperty.Id == "prop-double");
        
        Assert.NotNull(propertyRow);
        Assert.NotNull(propertyRow.OriginalProperty);
        
        var originalValue = propertyRow.PropertyValue;
        var originalDoubleValue = propertyRow.OriginalProperty.DoubleValue;
        var invalidValue = "не число";

        // Act
        await viewModel.SavePropertyValueAsync(propertyRow, invalidValue);

        // Assert
        // Проверяем, что файл НЕ обновлен
        var savedContent = await File.ReadAllTextAsync(_testFamilyOptionsPath);
        var savedDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(savedContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(savedDto);
        var savedProperty = savedDto.FamilyOptions
            .SelectMany(fo => fo.OptionProperties)
            .FirstOrDefault(op => op.Id == "prop-double");

        Assert.NotNull(savedProperty);
        // Значение должно остаться прежним
        Assert.NotEqual(invalidValue, savedProperty.Value?.ToString());
        
        // Модель не должна быть обновлена
        Assert.Equal(originalDoubleValue, propertyRow.OriginalProperty.DoubleValue);
    }

    [Fact]
    public async Task SavePropertyValueAsync_ReloadsCorrectly()
    {
        // Arrange
        // Проверяем, что тестовые файлы созданы
        Assert.True(File.Exists(_testFamilyOptionsPath), $"Файл family-options.json не найден: {_testFamilyOptionsPath}");
        var familyFile = Path.Combine(_testFamiliesDirectory, "test-family-id.json");
        Assert.True(File.Exists(familyFile), $"Файл семейства не найден: {familyFile}");
        
        var dataProvider1 = TestServiceFactory.CreateDataProvider(_testFamiliesDirectory, _testFamilyOptionsPath);
        
        // Проверяем, что семейство можно загрузить
        var testFamilyData1 = await dataProvider1.LoadFamilyDataAsync("test-family-id");
        Assert.NotNull(testFamilyData1);
        Assert.True(testFamilyData1.Validate(), "Данные семейства не прошли валидацию");
        
        var viewModel1 = TestServiceFactory.CreateMainWindowViewModel(
            dataProvider1,
            "test-family-id",
            _testFamilyOptionsPath,
            _testFamiliesDirectory);
        
        // Загружаем данные через ExecuteAsync
        if (viewModel1.LoadCommand is RelayCommand relayCommand1)
        {
            await relayCommand1.ExecuteAsync(null);
        }
        else if (viewModel1.LoadCommand.CanExecute(null))
        {
            viewModel1.LoadCommand.Execute(null);
            await Task.Delay(2000);
        }
        
        // Проверяем, что данные загружены
        Assert.True(viewModel1.Properties.Count > 0, $"Свойства не загружены в viewModel1. Количество: {viewModel1.Properties.Count}");
        
        var propertyRow = viewModel1.Properties.FirstOrDefault(p => 
            !p.IsSectionHeader && 
            p.OriginalProperty != null && 
            p.OriginalProperty.Id == "prop-string");
        
        Assert.NotNull(propertyRow);
        
        var newValue = "Значение после перезагрузки";

        // Act - сохраняем значение
        await viewModel1.SavePropertyValueAsync(propertyRow, newValue);

        // Act - создаем новый провайдер и загружаем данные заново
        var dataProvider2 = TestServiceFactory.CreateDataProvider(_testFamiliesDirectory, _testFamilyOptionsPath);
        var viewModel2 = TestServiceFactory.CreateMainWindowViewModel(
            dataProvider2,
            "test-family-id",
            _testFamilyOptionsPath,
            _testFamiliesDirectory);
        if (viewModel2.LoadCommand is RelayCommand relayCommand2)
        {
            await relayCommand2.ExecuteAsync(null);
        }
        else if (viewModel2.LoadCommand.CanExecute(null))
        {
            viewModel2.LoadCommand.Execute(null);
            await Task.Delay(2000);
        }

        // Проверяем, что данные загружены
        Assert.True(viewModel2.Properties.Count > 0, $"Свойства не загружены в viewModel2. Количество: {viewModel2.Properties.Count}");

        // Assert - проверяем, что значение сохранилось
        var reloadedPropertyRow = viewModel2.Properties.FirstOrDefault(p => 
            !p.IsSectionHeader && 
            p.OriginalProperty != null && 
            p.OriginalProperty.Id == "prop-string");
        
        Assert.NotNull(reloadedPropertyRow);
        Assert.NotNull(reloadedPropertyRow.OriginalProperty);
        Assert.Equal(newValue, reloadedPropertyRow.PropertyValue);
        Assert.Equal(newValue, reloadedPropertyRow.OriginalProperty.StringValue);
    }
}

