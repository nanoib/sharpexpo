using System.IO;
using System.Text.Json;
using SharpExpo.Contracts.DTOs;
using SharpExpo.Contracts.Models;
using SharpExpo.Family;

namespace SharpExpo.Tests.Unit.Providers;

public class JsonBimFamilyDataProviderTests : IDisposable
{
    private readonly string _testFamiliesDirectory;
    private readonly string _testFamilyOptionsFile;
    private readonly string _testDataDirectory;

    public JsonBimFamilyDataProviderTests()
    {
        _testDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _testFamiliesDirectory = Path.Combine(_testDataDirectory, "Families");
        _testFamilyOptionsFile = Path.Combine(_testDataDirectory, "family-options.json");

        Directory.CreateDirectory(_testFamiliesDirectory);
    }

    [Fact]
    public async Task LoadFamilyDataAsync_WithValidData_ReturnsFamilyData()
    {
        // Arrange
        var familyId = "test-family-1";
        var familyJson = @"{
            ""Id"": ""test-family-1"",
            ""Name"": ""Test Family"",
            ""FamilyOptionIds"": [""option1""],
            ""CategoryOrder"": [""Category1""]
        }";

        var familyOptionsJson = @"{
            ""FamilyOptions"": [
                {
                    ""Id"": ""option1"",
                    ""OptionProperties"": [
                        {
                            ""Id"": ""prop1"",
                            ""PropertyName"": ""Property 1"",
                            ""Description"": ""Test property"",
                            ""ValueType"": ""String"",
                            ""CategoryName"": ""Category1"",
                            ""Value"": ""Test Value""
                        }
                    ]
                }
            ]
        }";

        await File.WriteAllTextAsync(Path.Combine(_testFamiliesDirectory, $"{familyId}.json"), familyJson);
        await File.WriteAllTextAsync(_testFamilyOptionsFile, familyOptionsJson);

        var provider = new JsonBimFamilyDataProvider(_testFamiliesDirectory, _testFamilyOptionsFile);

        // Act
        var result = await provider.LoadFamilyDataAsync(familyId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(familyId, result!.Family.Id);
        Assert.Equal("Test Family", result.Family.Name);
        Assert.Single(result.FamilyOptions);
        Assert.True(result.FamilyOptions.ContainsKey("option1"));
    }

    [Fact]
    public async Task LoadFamilyDataAsync_WithNonExistentFamily_ReturnsNull()
    {
        // Arrange
        var familyOptionsJson = @"{
            ""FamilyOptions"": []
        }";

        await File.WriteAllTextAsync(_testFamilyOptionsFile, familyOptionsJson);

        var provider = new JsonBimFamilyDataProvider(_testFamiliesDirectory, _testFamilyOptionsFile);

        // Act
        var result = await provider.LoadFamilyDataAsync("non-existent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoadFamilyDataAsync_WithNullValue_ConvertsToEmptyString()
    {
        // Arrange
        var familyId = "test-family-1";
        var familyJson = @"{
            ""Id"": ""test-family-1"",
            ""Name"": ""Test Family"",
            ""FamilyOptionIds"": [""option1""],
            ""CategoryOrder"": [""Category1""]
        }";

        var familyOptionsJson = @"{
            ""FamilyOptions"": [
                {
                    ""Id"": ""option1"",
                    ""OptionProperties"": [
                        {
                            ""Id"": ""prop1"",
                            ""PropertyName"": ""Property 1"",
                            ""ValueType"": ""String"",
                            ""CategoryName"": ""Category1"",
                            ""Value"": null
                        }
                    ]
                }
            ]
        }";

        await File.WriteAllTextAsync(Path.Combine(_testFamiliesDirectory, $"{familyId}.json"), familyJson);
        await File.WriteAllTextAsync(_testFamilyOptionsFile, familyOptionsJson);

        var provider = new JsonBimFamilyDataProvider(_testFamiliesDirectory, _testFamilyOptionsFile);

        // Act
        var result = await provider.LoadFamilyDataAsync(familyId);

        // Assert
        Assert.NotNull(result);
        var optionProperty = result!.FamilyOptions["option1"].OptionProperties.First();
        Assert.Equal(string.Empty, optionProperty.StringValue);
        Assert.True(optionProperty.ValidateValue()); // Should be valid after conversion
    }

    [Fact]
    public async Task LoadAllFamiliesAsync_ReturnsAllFamilies()
    {
        // Arrange
        var family1Json = @"{
            ""Id"": ""family-1"",
            ""Name"": ""Family 1"",
            ""FamilyOptionIds"": [],
            ""CategoryOrder"": []
        }";

        var family2Json = @"{
            ""Id"": ""family-2"",
            ""Name"": ""Family 2"",
            ""FamilyOptionIds"": [],
            ""CategoryOrder"": []
        }";

        await File.WriteAllTextAsync(Path.Combine(_testFamiliesDirectory, "family-1.json"), family1Json);
        await File.WriteAllTextAsync(Path.Combine(_testFamiliesDirectory, "family-2.json"), family2Json);
        await File.WriteAllTextAsync(_testFamilyOptionsFile, @"{ ""FamilyOptions"": [] }");

        var provider = new JsonBimFamilyDataProvider(_testFamiliesDirectory, _testFamilyOptionsFile);

        // Act
        var result = await provider.LoadAllFamiliesAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, f => f.Id == "family-1");
        Assert.Contains(result, f => f.Id == "family-2");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataDirectory))
        {
            Directory.Delete(_testDataDirectory, true);
        }
    }
}


