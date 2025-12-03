using SharpExpo.Contracts.Models;

namespace SharpExpo.Tests.Unit.Models;

public class FamilyOptionTests
{
    [Fact]
    public void Validate_WithValidData_ReturnsTrue()
    {
        // Arrange
        var familyOption = new FamilyOption
        {
            Id = "test-id",
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

        // Act
        var result = familyOption.Validate();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Validate_WithEmptyId_ReturnsFalse()
    {
        // Arrange
        var familyOption = new FamilyOption
        {
            Id = string.Empty,
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

        // Act
        var result = familyOption.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithNullOptionProperties_ReturnsFalse()
    {
        // Arrange
        var familyOption = new FamilyOption
        {
            Id = "test-id",
            OptionProperties = null!
        };

        // Act
        var result = familyOption.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithEmptyOptionProperties_ReturnsFalse()
    {
        // Arrange
        var familyOption = new FamilyOption
        {
            Id = "test-id",
            OptionProperties = new List<OptionProperty>()
        };

        // Act
        var result = familyOption.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithInvalidOptionProperty_ReturnsFalse()
    {
        // Arrange
        var familyOption = new FamilyOption
        {
            Id = "test-id",
            OptionProperties = new List<OptionProperty>
            {
                new OptionProperty
                {
                    Id = "prop1",
                    PropertyName = "Property 1",
                    ValueType = OptionValueType.String,
                    StringValue = null, // Invalid - null value for String type
                    CategoryName = "Category1"
                }
            }
        };

        // Act
        var result = familyOption.Validate();

        // Assert
        Assert.False(result);
    }
}


