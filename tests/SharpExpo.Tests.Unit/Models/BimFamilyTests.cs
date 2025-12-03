using SharpExpo.Contracts.Models;

namespace SharpExpo.Tests.Unit.Models;

public class BimFamilyTests
{
    [Fact]
    public void Validate_WithValidData_ReturnsTrue()
    {
        // Arrange
        var bimFamily = new BimFamily
        {
            Id = "test-id",
            Name = "Test Family",
            FamilyOptionIds = new List<string> { "option1", "option2" },
            CategoryOrder = new List<string> { "Category1", "Category2" }
        };

        // Act
        var result = bimFamily.Validate();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Validate_WithEmptyId_ReturnsFalse()
    {
        // Arrange
        var bimFamily = new BimFamily
        {
            Id = string.Empty,
            Name = "Test Family",
            FamilyOptionIds = new List<string> { "option1" },
            CategoryOrder = new List<string>()
        };

        // Act
        var result = bimFamily.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsFalse()
    {
        // Arrange
        var bimFamily = new BimFamily
        {
            Id = "test-id",
            Name = string.Empty,
            FamilyOptionIds = new List<string> { "option1" },
            CategoryOrder = new List<string>()
        };

        // Act
        var result = bimFamily.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithNullFamilyOptionIds_ReturnsFalse()
    {
        // Arrange
        var bimFamily = new BimFamily
        {
            Id = "test-id",
            Name = "Test Family",
            FamilyOptionIds = null!,
            CategoryOrder = new List<string>()
        };

        // Act
        var result = bimFamily.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithEmptyFamilyOptionIds_ReturnsFalse()
    {
        // Arrange
        var bimFamily = new BimFamily
        {
            Id = "test-id",
            Name = "Test Family",
            FamilyOptionIds = new List<string>(),
            CategoryOrder = new List<string>()
        };

        // Act
        var result = bimFamily.Validate();

        // Assert
        Assert.False(result);
    }
}

