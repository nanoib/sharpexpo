using System;
using System.IO;
using System.Linq;
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
/// Интеграционный тест для проверки сохранения и перезагрузки данных
/// Симулирует реальный сценарий: изменение значения, сохранение, перезапуск приложения
/// </summary>
public class IntegrationSaveReloadTest : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testFamilyOptionsPath;
    private readonly string _testFamiliesDirectory;
    private const string TestFamilyId = "integration-test-family";
    private const string TestFamilyOptionId = "option-integration-test";
    private const string TestPropertyId = "prop-integration-name";

    public IntegrationSaveReloadTest()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _testFamiliesDirectory = Path.Combine(_testDirectory, "Families");
        _testFamilyOptionsPath = Path.Combine(_testDirectory, "family-options.json");

        Directory.CreateDirectory(_testFamiliesDirectory);

        // Создаем начальные тестовые данные
        CreateTestData();
    }

    private void CreateTestData()
    {
        // Создаем family-options.json с начальным значением "1"
        var initialFamilyOptions = new FamilyOptionsCollectionDto
        {
            FamilyOptions = new List<FamilyOptionItemDto>
            {
                new FamilyOptionItemDto
                {
                    Id = TestFamilyOptionId,
                    OptionProperties = new List<OptionPropertyDto>
                    {
                        new OptionPropertyDto
                        {
                            Id = TestPropertyId,
                            PropertyName = "Наименование",
                            Description = "Тестовое наименование для интеграционного теста",
                            ValueType = "String",
                            CategoryName = "Наименование",
                            Value = "1" // Начальное значение
                        }
                    }
                }
            }
        };

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null
        };

        File.WriteAllText(_testFamilyOptionsPath, JsonSerializer.Serialize(initialFamilyOptions, jsonOptions));

        // Создаем файл семейства
        var familyDto = new BimFamilyDto
        {
            Id = TestFamilyId,
            Name = "Интеграционное тестовое семейство",
            FamilyOptionIds = new List<string> { TestFamilyOptionId },
            CategoryOrder = new List<string> { "Наименование" }
        };

        var familyFile = Path.Combine(_testFamiliesDirectory, $"{TestFamilyId}.json");
        File.WriteAllText(familyFile, JsonSerializer.Serialize(familyDto, jsonOptions));
    }

    [Fact]
    public async Task SaveAndReload_StringValue_PersistsAfterRestart()
    {
        // Arrange - Первый запуск приложения
        var dataProvider1 = TestServiceFactory.CreateDataProvider(_testFamiliesDirectory, _testFamilyOptionsPath);
        var viewModel1 = TestServiceFactory.CreateMainWindowViewModel(
            dataProvider1,
            TestFamilyId,
            _testFamilyOptionsPath,
            _testFamiliesDirectory);

        // Загружаем данные
        if (viewModel1.LoadCommand is RelayCommand relayCommand1)
        {
            await relayCommand1.ExecuteAsync(null);
        }

        // Проверяем, что данные загружены
        Assert.True(viewModel1.Properties.Count > 0, "Данные не загружены в первом запуске");

        // Находим свойство "Наименование"
        var propertyRow = viewModel1.Properties.FirstOrDefault(p =>
            !p.IsSectionHeader &&
            p.OriginalProperty != null &&
            p.OriginalProperty.Id == TestPropertyId);

        Assert.NotNull(propertyRow);
        Assert.Equal("1", propertyRow.PropertyValue); // Проверяем начальное значение

        // Act - Изменяем значение на "Hello" и сохраняем
        var newValue = "Hello";
        await viewModel1.SavePropertyValueAsync(propertyRow, newValue);

        // Проверяем, что значение обновилось в ViewModel
        Assert.NotNull(propertyRow.OriginalProperty);
        Assert.Equal(newValue, propertyRow.PropertyValue);
        Assert.Equal(newValue, propertyRow.OriginalProperty.StringValue);

        // Проверяем, что файл действительно обновился
        var savedContent = await File.ReadAllTextAsync(_testFamilyOptionsPath);
        var savedDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(savedContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(savedDto);
        var savedProperty = savedDto.FamilyOptions
            .FirstOrDefault(fo => fo.Id == TestFamilyOptionId)
            ?.OptionProperties.FirstOrDefault(op => op.Id == TestPropertyId);

        Assert.NotNull(savedProperty);
        Assert.Equal(newValue, savedProperty!.Value?.ToString());

        // Arrange - Второй запуск приложения (симуляция перезапуска)
        // Создаем новый провайдер и ViewModel, как будто приложение перезапустилось
        var dataProvider2 = TestServiceFactory.CreateDataProvider(_testFamiliesDirectory, _testFamilyOptionsPath);
        var viewModel2 = TestServiceFactory.CreateMainWindowViewModel(
            dataProvider2,
            TestFamilyId,
            _testFamilyOptionsPath,
            _testFamiliesDirectory);

        // Загружаем данные заново
        if (viewModel2.LoadCommand is RelayCommand relayCommand2)
        {
            await relayCommand2.ExecuteAsync(null);
        }

        // Проверяем, что данные загружены
        Assert.True(viewModel2.Properties.Count > 0, "Данные не загружены во втором запуске");

        // Находим то же свойство
        var reloadedPropertyRow = viewModel2.Properties.FirstOrDefault(p =>
            !p.IsSectionHeader &&
            p.OriginalProperty != null &&
            p.OriginalProperty.Id == TestPropertyId);

        Assert.NotNull(reloadedPropertyRow);
        Assert.NotNull(reloadedPropertyRow.OriginalProperty);

        // Assert - Проверяем, что сохраненное значение загрузилось
        Assert.Equal(newValue, reloadedPropertyRow.PropertyValue);
        Assert.Equal(newValue, reloadedPropertyRow.OriginalProperty.StringValue);

        // Дополнительная проверка: читаем файл напрямую
        var finalContent = await File.ReadAllTextAsync(_testFamilyOptionsPath);
        var finalDto = JsonSerializer.Deserialize<FamilyOptionsCollectionDto>(finalContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(finalDto);
        var finalProperty = finalDto.FamilyOptions
            .FirstOrDefault(fo => fo.Id == TestFamilyOptionId)
            ?.OptionProperties.FirstOrDefault(op => op.Id == TestPropertyId);

        Assert.NotNull(finalProperty);
        Assert.Equal(newValue, finalProperty!.Value?.ToString());
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}

