using SharpExpo.Contracts.DTOs;
using SharpExpo.Contracts.Models;
using SharpExpo.Family.Mappers;
using Xunit;

namespace SharpExpo.Tests.Unit.Mappers;

/// <summary>
/// Unit tests for the <see cref="DtoMapper"/> class.
/// </summary>
public class DtoMapperTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Arrange & Act
        var mapper = new DtoMapper();

        // Assert
        Assert.NotNull(mapper);
    }

    [Fact]
    public void MapToBimFamily_WithValidDto_ReturnsBimFamily()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new BimFamilyDto
        {
            Id = "family1",
            Name = "Test Family",
            FamilyOptionIds = new List<string> { "option1", "option2" },
            CategoryOrder = new List<string> { "Category1", "Category2" }
        };

        // Act
        var result = mapper.MapToBimFamily(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("family1", result!.Id);
        Assert.Equal("Test Family", result.Name);
        Assert.Equal(2, result.FamilyOptionIds.Count);
        Assert.Equal(2, result.CategoryOrder.Count);
    }

    [Fact]
    public void MapToBimFamily_WithNullDto_ReturnsNull()
    {
        // Arrange
        var mapper = new DtoMapper();

        // Act
        var result = mapper.MapToBimFamily(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MapToFamilyOption_WithValidDto_ReturnsFamilyOption()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new FamilyOptionItemDto
        {
            Id = "option1",
            OptionProperties = new List<OptionPropertyDto>
            {
                new OptionPropertyDto
                {
                    Id = "prop1",
                    PropertyName = "Property 1",
                    ValueType = "String",
                    Value = "Value1"
                }
            }
        };

        // Act
        var result = mapper.MapToFamilyOption(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("option1", result!.Id);
        Assert.Single(result.OptionProperties);
        Assert.Equal("prop1", result.OptionProperties[0].Id);
    }

    [Fact]
    public void MapToFamilyOption_WithEmptyId_ReturnsNull()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new FamilyOptionItemDto
        {
            Id = string.Empty,
            OptionProperties = new List<OptionPropertyDto>()
        };

        // Act
        var result = mapper.MapToFamilyOption(dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MapToFamilyOption_WithNullProperties_ReturnsFamilyOptionWithEmptyList()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new FamilyOptionItemDto
        {
            Id = "option1",
            OptionProperties = null
        };

        // Act
        var result = mapper.MapToFamilyOption(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result!.OptionProperties);
    }

    [Fact]
    public void MapToOptionProperty_WithStringProperty_ReturnsOptionProperty()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new OptionPropertyDto
        {
            Id = "prop1",
            PropertyName = "Test Property",
            Description = "Test Description",
            ValueType = "String",
            CategoryName = "Test Category",
            Value = "Test Value"
        };

        // Act
        var result = mapper.MapToOptionProperty(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("prop1", result!.Id);
        Assert.Equal("Test Property", result.PropertyName);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(OptionValueType.String, result.ValueType);
        Assert.Equal("Test Category", result.CategoryName);
        Assert.Equal("Test Value", result.StringValue);
    }

    [Fact]
    public void MapToOptionProperty_WithDoubleProperty_ReturnsOptionProperty()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new OptionPropertyDto
        {
            Id = "prop1",
            PropertyName = "Test Property",
            ValueType = "Double",
            Value = 123.45
        };

        // Act
        var result = mapper.MapToOptionProperty(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OptionValueType.Double, result!.ValueType);
        Assert.NotNull(result.DoubleValue);
        Assert.Equal(123.45, result.DoubleValue.Value, 2);
    }

    [Fact]
    public void MapToOptionProperty_WithEnumerationProperty_ReturnsOptionProperty()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new OptionPropertyDto
        {
            Id = "prop1",
            PropertyName = "Test Property",
            ValueType = "Enumeration",
            Value = "Option1"
        };

        // Act
        var result = mapper.MapToOptionProperty(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OptionValueType.Enumeration, result!.ValueType);
        Assert.Equal("Option1", result.EnumValue);
    }

    [Fact]
    public void MapToOptionProperty_WithNullValue_ConvertsToStringEmpty()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new OptionPropertyDto
        {
            Id = "prop1",
            PropertyName = "Test Property",
            ValueType = "String",
            Value = null
        };

        // Act
        var result = mapper.MapToOptionProperty(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result!.StringValue);
    }

    [Fact]
    public void MapToOptionProperty_WithEmptyId_ReturnsNull()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new OptionPropertyDto
        {
            Id = string.Empty,
            PropertyName = "Test Property",
            ValueType = "String"
        };

        // Act
        var result = mapper.MapToOptionProperty(dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MapToOptionProperty_WithEmptyPropertyName_ReturnsNull()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new OptionPropertyDto
        {
            Id = "prop1",
            PropertyName = string.Empty,
            ValueType = "String"
        };

        // Act
        var result = mapper.MapToOptionProperty(dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MapToOptionProperty_WithInvalidValueType_ReturnsNull()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new OptionPropertyDto
        {
            Id = "prop1",
            PropertyName = "Test Property",
            ValueType = "InvalidType"
        };

        // Act
        var result = mapper.MapToOptionProperty(dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseValueType_WithString_ReturnsStringType()
    {
        // Arrange
        var mapper = new DtoMapper();

        // Act
        var result = mapper.ParseValueType("String");

        // Assert
        Assert.Equal(OptionValueType.String, result);
    }

    [Fact]
    public void ParseValueType_WithDouble_ReturnsDoubleType()
    {
        // Arrange
        var mapper = new DtoMapper();

        // Act
        var result = mapper.ParseValueType("Double");

        // Assert
        Assert.Equal(OptionValueType.Double, result);
    }

    [Fact]
    public void ParseValueType_WithEnumeration_ReturnsEnumerationType()
    {
        // Arrange
        var mapper = new DtoMapper();

        // Act
        var result = mapper.ParseValueType("Enumeration");

        // Assert
        Assert.Equal(OptionValueType.Enumeration, result);
    }

    [Fact]
    public void ParseValueType_WithRussianString_ReturnsStringType()
    {
        // Arrange
        var mapper = new DtoMapper();

        // Act
        var result = mapper.ParseValueType("строка");

        // Assert
        Assert.Equal(OptionValueType.String, result);
    }

    [Fact]
    public void ParseValueType_WithRussianNumber_ReturnsDoubleType()
    {
        // Arrange
        var mapper = new DtoMapper();

        // Act
        var result = mapper.ParseValueType("число");

        // Assert
        Assert.Equal(OptionValueType.Double, result);
    }

    [Fact]
    public void ParseValueType_WithNull_ReturnsStringType()
    {
        // Arrange
        var mapper = new DtoMapper();

        // Act
        var result = mapper.ParseValueType(null);

        // Assert
        Assert.Equal(OptionValueType.String, result);
    }

    [Fact]
    public void ParseValueType_WithEmptyString_ReturnsStringType()
    {
        // Arrange
        var mapper = new DtoMapper();

        // Act
        var result = mapper.ParseValueType(string.Empty);

        // Assert
        Assert.Equal(OptionValueType.String, result);
    }

    [Fact]
    public void ParseValueType_WithInvalidString_ReturnsNull()
    {
        // Arrange
        var mapper = new DtoMapper();

        // Act
        var result = mapper.ParseValueType("InvalidType");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MapToOptionProperty_WithDoubleAsString_ParsesCorrectly()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new OptionPropertyDto
        {
            Id = "prop1",
            PropertyName = "Test Property",
            ValueType = "Double",
            Value = "123.45"
        };

        // Act
        var result = mapper.MapToOptionProperty(dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.DoubleValue);
        Assert.Equal(123.45, result.DoubleValue.Value, 2);
    }

    [Fact]
    public void MapToOptionProperty_WithIntValue_ConvertsToDouble()
    {
        // Arrange
        var mapper = new DtoMapper();
        var dto = new OptionPropertyDto
        {
            Id = "prop1",
            PropertyName = "Test Property",
            ValueType = "Double",
            Value = 100
        };

        // Act
        var result = mapper.MapToOptionProperty(dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.DoubleValue);
        Assert.Equal(100.0, result.DoubleValue.Value);
    }
}




