using SharpExpo.Contracts.Models;
using SharpExpo.UI.Services;
using SharpExpo.UI.ViewModels;
using System.Windows.Input;
using Xunit;

namespace SharpExpo.Tests.Unit.Services;

/// <summary>
/// Unit tests for the <see cref="PropertyViewModelFactory"/> class.
/// </summary>
public class PropertyViewModelFactoryTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Arrange & Act
        var factory = new PropertyViewModelFactory();

        // Assert
        Assert.NotNull(factory);
    }

    [Fact]
    public void CreateCategoryHeader_WithValidParameters_CreatesCategoryHeader()
    {
        // Arrange
        var factory = new PropertyViewModelFactory();
        var categoryName = "Test Category";
        var toggleCommand = new TestCommand();

        // Act
        var result = factory.CreateCategoryHeader(categoryName, toggleCommand);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSectionHeader);
        Assert.Equal(categoryName, result.SectionName);
        Assert.True(result.IsExpanded);
        Assert.Equal(toggleCommand, result.ToggleExpandCommand);
    }

    [Fact]
    public void CreateCategoryHeader_WithNullCategoryName_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = new PropertyViewModelFactory();
        var toggleCommand = new TestCommand();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.CreateCategoryHeader(null!, toggleCommand));
    }

    [Fact]
    public void CreateCategoryHeader_WithNullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = new PropertyViewModelFactory();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.CreateCategoryHeader("Category", null!));
    }

    [Fact]
    public void CreatePropertyRow_WithStringProperty_CreatesPropertyRow()
    {
        // Arrange
        var factory = new PropertyViewModelFactory();
        var optionProperty = new OptionProperty
        {
            Id = "prop1",
            PropertyName = "Test Property",
            Description = "Test Description",
            ValueType = OptionValueType.String,
            CategoryName = "Test Category",
            StringValue = "Test Value"
        };
        var familyOptionId = "option1";

        // Act
        var result = factory.CreatePropertyRow(optionProperty, familyOptionId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSectionHeader);
        Assert.Equal("Test Property", result.PropertyName);
        Assert.Equal("Test Value", result.PropertyValue);
        Assert.Equal("Test Description", result.Description);
        Assert.False(result.IsLocked);
        Assert.True(result.HasCategory);
        Assert.False(result.HasDropdown);
        Assert.Equal(optionProperty, result.OriginalProperty);
        Assert.Equal(familyOptionId, result.FamilyOptionId);
        Assert.Equal(OptionValueType.String, result.ValueType);
    }

    [Fact]
    public void CreatePropertyRow_WithDoubleProperty_CreatesPropertyRow()
    {
        // Arrange
        var factory = new PropertyViewModelFactory();
        var optionProperty = new OptionProperty
        {
            Id = "prop1",
            PropertyName = "Test Property",
            Description = "Test Description",
            ValueType = OptionValueType.Double,
            CategoryName = "Test Category",
            DoubleValue = 123.45
        };
        var familyOptionId = "option1";

        // Act
        var result = factory.CreatePropertyRow(optionProperty, familyOptionId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSectionHeader);
        Assert.Equal("Test Property", result.PropertyName);
        // GetDisplayValue uses current culture, so format may vary (123.45 or 123,45)
        // We check that it contains the number value
        Assert.True(result.PropertyValue == "123.45" || result.PropertyValue == "123,45" || 
                   result.PropertyValue.Contains("123") && result.PropertyValue.Contains("45"));
        Assert.False(result.IsLocked);
        Assert.Equal(OptionValueType.Double, result.ValueType);
    }

    [Fact]
    public void CreatePropertyRow_WithEnumerationProperty_CreatesLockedPropertyRow()
    {
        // Arrange
        var factory = new PropertyViewModelFactory();
        var optionProperty = new OptionProperty
        {
            Id = "prop1",
            PropertyName = "Test Property",
            Description = "Test Description",
            ValueType = OptionValueType.Enumeration,
            CategoryName = "Test Category",
            EnumValue = "Option1"
        };
        var familyOptionId = "option1";

        // Act
        var result = factory.CreatePropertyRow(optionProperty, familyOptionId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsLocked); // Enumeration should be locked
        Assert.True(result.HasDropdown); // Enumeration should have dropdown
        Assert.Equal(OptionValueType.Enumeration, result.ValueType);
    }

    [Fact]
    public void CreatePropertyRow_WithNullCategoryName_SetsHasCategoryToFalse()
    {
        // Arrange
        var factory = new PropertyViewModelFactory();
        var optionProperty = new OptionProperty
        {
            Id = "prop1",
            PropertyName = "Test Property",
            ValueType = OptionValueType.String,
            CategoryName = null,
            StringValue = "Value"
        };
        var familyOptionId = "option1";

        // Act
        var result = factory.CreatePropertyRow(optionProperty, familyOptionId);

        // Assert
        Assert.False(result.HasCategory);
    }

    [Fact]
    public void CreatePropertyRow_WithEmptyCategoryName_SetsHasCategoryToFalse()
    {
        // Arrange
        var factory = new PropertyViewModelFactory();
        var optionProperty = new OptionProperty
        {
            Id = "prop1",
            PropertyName = "Test Property",
            ValueType = OptionValueType.String,
            CategoryName = string.Empty,
            StringValue = "Value"
        };
        var familyOptionId = "option1";

        // Act
        var result = factory.CreatePropertyRow(optionProperty, familyOptionId);

        // Assert
        Assert.False(result.HasCategory);
    }

    [Fact]
    public void CreatePropertyRow_WithNullOptionProperty_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = new PropertyViewModelFactory();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.CreatePropertyRow(null!, "option1"));
    }

    [Fact]
    public void CreatePropertyRow_WithNullFamilyOptionId_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = new PropertyViewModelFactory();
        var optionProperty = new OptionProperty
        {
            Id = "prop1",
            PropertyName = "Test Property",
            ValueType = OptionValueType.String,
            StringValue = "Value"
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.CreatePropertyRow(optionProperty, null!));
    }

    private class TestCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
        }
    }
}

