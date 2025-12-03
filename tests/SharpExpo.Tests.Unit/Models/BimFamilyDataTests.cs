using SharpExpo.Contracts.Models;

namespace SharpExpo.Tests.Unit.Models;

public class BimFamilyDataTests
{
    [Fact]
    public void GetOrderedOptionsByCategory_ReturnsPropertiesInCategoryOrder()
    {
        // Arrange
        var family = new BimFamily
        {
            Id = "test-id",
            Name = "Test Family",
            FamilyOptionIds = new List<string> { "option1" },
            CategoryOrder = new List<string> { "Category2", "Category1" }
        };

        var familyOption = new FamilyOption
        {
            Id = "option1",
            OptionProperties = new List<OptionProperty>
            {
                new OptionProperty
                {
                    Id = "prop1",
                    PropertyName = "Property 1",
                    ValueType = OptionValueType.String,
                    StringValue = "Value1",
                    CategoryName = "Category1"
                },
                new OptionProperty
                {
                    Id = "prop2",
                    PropertyName = "Property 2",
                    ValueType = OptionValueType.String,
                    StringValue = "Value2",
                    CategoryName = "Category2"
                }
            }
        };

        var familyData = new BimFamilyData
        {
            Family = family,
            FamilyOptions = new Dictionary<string, FamilyOption>
            {
                { "option1", familyOption }
            }
        };

        // Act
        var result = familyData.GetOrderedOptionsByCategory();

        // Assert
        Assert.Equal(2, result.Count);
        var categories = result.Keys.ToList();
        Assert.Equal("Category2", categories[0]); // First in CategoryOrder
        Assert.Equal("Category1", categories[1]); // Second in CategoryOrder
    }

    [Fact]
    public void GetOrderedOptionsByCategory_WithMissingCategoryInOrder_AddsAtEnd()
    {
        // Arrange
        var family = new BimFamily
        {
            Id = "test-id",
            Name = "Test Family",
            FamilyOptionIds = new List<string> { "option1" },
            CategoryOrder = new List<string> { "Category1" }
        };

        var familyOption = new FamilyOption
        {
            Id = "option1",
            OptionProperties = new List<OptionProperty>
            {
                new OptionProperty
                {
                    Id = "prop1",
                    PropertyName = "Property 1",
                    ValueType = OptionValueType.String,
                    StringValue = "Value1",
                    CategoryName = "Category1"
                },
                new OptionProperty
                {
                    Id = "prop2",
                    PropertyName = "Property 2",
                    ValueType = OptionValueType.String,
                    StringValue = "Value2",
                    CategoryName = "Category2" // Not in CategoryOrder
                }
            }
        };

        var familyData = new BimFamilyData
        {
            Family = family,
            FamilyOptions = new Dictionary<string, FamilyOption>
            {
                { "option1", familyOption }
            }
        };

        // Act
        var result = familyData.GetOrderedOptionsByCategory();

        // Assert
        Assert.Equal(2, result.Count);
        var categories = result.Keys.ToList();
        Assert.Equal("Category1", categories[0]); // From CategoryOrder
        Assert.Equal("Category2", categories[1]); // Added at end
    }

    [Fact]
    public void Validate_WithValidData_ReturnsTrue()
    {
        // Arrange
        var family = new BimFamily
        {
            Id = "test-id",
            Name = "Test Family",
            FamilyOptionIds = new List<string> { "option1" },
            CategoryOrder = new List<string>()
        };

        var familyOption = new FamilyOption
        {
            Id = "option1",
            OptionProperties = new List<OptionProperty>
            {
                new OptionProperty
                {
                    Id = "prop1",
                    PropertyName = "Property 1",
                    ValueType = OptionValueType.String,
                    StringValue = "Value1",
                    CategoryName = "Category1"
                }
            }
        };

        var familyData = new BimFamilyData
        {
            Family = family,
            FamilyOptions = new Dictionary<string, FamilyOption>
            {
                { "option1", familyOption }
            }
        };

        // Act
        var result = familyData.Validate();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Validate_WithMissingFamilyOption_ReturnsFalse()
    {
        // Arrange
        var family = new BimFamily
        {
            Id = "test-id",
            Name = "Test Family",
            FamilyOptionIds = new List<string> { "option1", "option2" },
            CategoryOrder = new List<string>()
        };

        var familyData = new BimFamilyData
        {
            Family = family,
            FamilyOptions = new Dictionary<string, FamilyOption>
            {
                { "option1", new FamilyOption { Id = "option1", OptionProperties = new List<OptionProperty>() } }
                // option2 is missing
            }
        };

        // Act
        var result = familyData.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithInvalidFamilyOption_ReturnsFalse()
    {
        // Arrange
        var family = new BimFamily
        {
            Id = "test-id",
            Name = "Test Family",
            FamilyOptionIds = new List<string> { "option1" },
            CategoryOrder = new List<string>()
        };

        var invalidFamilyOption = new FamilyOption
        {
            Id = "option1",
            OptionProperties = new List<OptionProperty>() // Empty - invalid
        };

        var familyData = new BimFamilyData
        {
            Family = family,
            FamilyOptions = new Dictionary<string, FamilyOption>
            {
                { "option1", invalidFamilyOption }
            }
        };

        // Act
        var result = familyData.Validate();

        // Assert
        Assert.False(result);
    }
}

