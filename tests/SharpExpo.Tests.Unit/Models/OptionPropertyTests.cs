using SharpExpo.Contracts.Models;

namespace SharpExpo.Tests.Unit.Models;

public class OptionPropertyTests
{
    [Fact]
    public void ValidateValue_StringType_WithValue_ReturnsTrue()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.String,
            StringValue = "Test Value"
        };

        // Act
        var result = optionProperty.ValidateValue();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateValue_StringType_WithEmptyString_ReturnsTrue()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.String,
            StringValue = string.Empty
        };

        // Act
        var result = optionProperty.ValidateValue();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateValue_StringType_WithNull_ReturnsFalse()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.String,
            StringValue = null
        };

        // Act
        var result = optionProperty.ValidateValue();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateValue_DoubleType_WithValue_ReturnsTrue()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.Double,
            DoubleValue = 123.45
        };

        // Act
        var result = optionProperty.ValidateValue();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateValue_DoubleType_WithNull_ReturnsFalse()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.Double,
            DoubleValue = null
        };

        // Act
        var result = optionProperty.ValidateValue();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateValue_EnumerationType_WithValue_ReturnsTrue()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.Enumeration,
            EnumValue = "Value1"
        };

        // Act
        var result = optionProperty.ValidateValue();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateValue_EnumerationType_WithEmptyString_ReturnsTrue()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.Enumeration,
            EnumValue = string.Empty
        };

        // Act
        var result = optionProperty.ValidateValue();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateValue_EnumerationType_WithNull_ReturnsFalse()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.Enumeration,
            EnumValue = null
        };

        // Act
        var result = optionProperty.ValidateValue();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetDisplayValue_StringType_ReturnsStringValue()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.String,
            StringValue = "Test"
        };

        // Act
        var result = optionProperty.GetDisplayValue();

        // Assert
        Assert.Equal("Test", result);
    }

    [Fact]
    public void GetDisplayValue_StringType_WithNull_ReturnsEmptyString()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.String,
            StringValue = null
        };

        // Act
        var result = optionProperty.GetDisplayValue();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetDisplayValue_DoubleType_ReturnsFormattedValue()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.Double,
            DoubleValue = 123.456
        };

        // Act
        var result = optionProperty.GetDisplayValue();

        // Assert
        // Проверяем, что значение отформатировано с двумя знаками после запятой
        // (может быть точка или запятая в зависимости от локали)
        Assert.True(result == "123.46" || result == "123,46" || result.Contains("123") && result.Contains("46"));
    }

    [Fact]
    public void GetDisplayValue_DoubleType_WithNull_ReturnsEmptyString()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.Double,
            DoubleValue = null
        };

        // Act
        var result = optionProperty.GetDisplayValue();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetDisplayValue_EnumerationType_ReturnsEnumValue()
    {
        // Arrange
        var optionProperty = new OptionProperty
        {
            ValueType = OptionValueType.Enumeration,
            EnumValue = "Value1"
        };

        // Act
        var result = optionProperty.GetDisplayValue();

        // Assert
        Assert.Equal("Value1", result);
    }
}

