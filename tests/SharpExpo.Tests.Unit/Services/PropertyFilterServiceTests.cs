using SharpExpo.UI.Services;
using SharpExpo.UI.ViewModels;
using System.Windows.Input;
using Xunit;

namespace SharpExpo.Tests.Unit.Services;

/// <summary>
/// Unit tests for the <see cref="PropertyFilterService"/> class.
/// </summary>
public class PropertyFilterServiceTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Arrange & Act
        var filterService = new PropertyFilterService();

        // Assert
        Assert.NotNull(filterService);
    }

    [Fact]
    public void FilterProperties_WithNullList_ThrowsArgumentNullException()
    {
        // Arrange
        var filterService = new PropertyFilterService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => filterService.FilterProperties(null!, "search"));
    }

    [Fact]
    public void FilterProperties_WithEmptySearchText_ReturnsAllExpandedProperties()
    {
        // Arrange
        var filterService = new PropertyFilterService();
        var category1 = CreateCategoryHeader("Category1", true);
        var category2 = CreateCategoryHeader("Category2", false);
        var prop1 = CreatePropertyRow("Property1", "Value1", category1);
        var prop2 = CreatePropertyRow("Property2", "Value2", category2);
        var allProperties = new List<PropertyRowViewModel> { category1, prop1, category2, prop2 };

        // Act
        var filtered = filterService.FilterProperties(allProperties, string.Empty);

        // Assert
        Assert.Contains(category1, filtered);
        Assert.Contains(prop1, filtered); // Category1 is expanded
        Assert.Contains(category2, filtered);
        Assert.DoesNotContain(prop2, filtered); // Category2 is collapsed
    }

    [Fact]
    public void FilterProperties_WithSearchText_MatchesPropertyName()
    {
        // Arrange
        var filterService = new PropertyFilterService();
        var category = CreateCategoryHeader("Category1", true);
        var prop1 = CreatePropertyRow("Property1", "Value1", category);
        var prop2 = CreatePropertyRow("Property2", "Value2", category);
        var allProperties = new List<PropertyRowViewModel> { category, prop1, prop2 };

        // Act
        var filtered = filterService.FilterProperties(allProperties, "Property1");

        // Assert
        Assert.Contains(category, filtered);
        Assert.Contains(prop1, filtered);
        Assert.DoesNotContain(prop2, filtered);
    }

    [Fact]
    public void FilterProperties_WithSearchText_MatchesPropertyValue()
    {
        // Arrange
        var filterService = new PropertyFilterService();
        var category = CreateCategoryHeader("Category1", true);
        var prop1 = CreatePropertyRow("Property1", "Value1", category);
        var prop2 = CreatePropertyRow("Property2", "Value2", category);
        var allProperties = new List<PropertyRowViewModel> { category, prop1, prop2 };

        // Act
        var filtered = filterService.FilterProperties(allProperties, "Value1");

        // Assert
        Assert.Contains(category, filtered);
        Assert.Contains(prop1, filtered);
        Assert.DoesNotContain(prop2, filtered);
    }

    [Fact]
    public void FilterProperties_WithSearchText_MatchesCategoryName()
    {
        // Arrange
        var filterService = new PropertyFilterService();
        var category1 = CreateCategoryHeader("Category1", true);
        var category2 = CreateCategoryHeader("Category2", false);
        var prop1 = CreatePropertyRow("Property1", "Value1", category1);
        var prop2 = CreatePropertyRow("Property2", "Value2", category2);
        var allProperties = new List<PropertyRowViewModel> { category1, prop1, category2, prop2 };

        // Act
        var filtered = filterService.FilterProperties(allProperties, "Category1");

        // Assert
        Assert.Contains(category1, filtered);
        Assert.Contains(prop1, filtered); // Property in matching category is included
        Assert.Contains(category2, filtered);
        Assert.DoesNotContain(prop2, filtered);
    }

    [Fact]
    public void FilterProperties_WithSearchText_CaseInsensitive()
    {
        // Arrange
        var filterService = new PropertyFilterService();
        var category = CreateCategoryHeader("Category1", true);
        var prop1 = CreatePropertyRow("Property1", "Value1", category);
        var allProperties = new List<PropertyRowViewModel> { category, prop1 };

        // Act
        var filtered = filterService.FilterProperties(allProperties, "PROPERTY1");

        // Assert
        Assert.Contains(prop1, filtered);
    }

    [Fact]
    public void FilterProperties_WithSearchText_PartialMatch()
    {
        // Arrange
        var filterService = new PropertyFilterService();
        var category = CreateCategoryHeader("Category1", true);
        var prop1 = CreatePropertyRow("MyProperty1", "Value1", category);
        var prop2 = CreatePropertyRow("MyProperty2", "Value2", category);
        var allProperties = new List<PropertyRowViewModel> { category, prop1, prop2 };

        // Act
        var filtered = filterService.FilterProperties(allProperties, "Property1");

        // Assert
        Assert.Contains(prop1, filtered);
        Assert.DoesNotContain(prop2, filtered);
    }

    [Fact]
    public void FilterProperties_WithWhitespaceSearchText_ReturnsAllExpandedProperties()
    {
        // Arrange
        var filterService = new PropertyFilterService();
        var category1 = CreateCategoryHeader("Category1", true);
        var category2 = CreateCategoryHeader("Category2", false);
        var prop1 = CreatePropertyRow("Property1", "Value1", category1);
        var prop2 = CreatePropertyRow("Property2", "Value2", category2);
        var allProperties = new List<PropertyRowViewModel> { category1, prop1, category2, prop2 };

        // Act
        var filtered = filterService.FilterProperties(allProperties, "   ");

        // Assert
        Assert.Contains(category1, filtered);
        Assert.Contains(prop1, filtered);
        Assert.Contains(category2, filtered);
        Assert.DoesNotContain(prop2, filtered);
    }

    private static PropertyRowViewModel CreateCategoryHeader(string categoryName, bool isExpanded)
    {
        var toggleCommand = new RelayCommand(() => { });
        return new PropertyRowViewModel
        {
            IsSectionHeader = true,
            SectionName = categoryName,
            IsExpanded = isExpanded,
            ToggleExpandCommand = toggleCommand
        };
    }

    private static PropertyRowViewModel CreatePropertyRow(string propertyName, string propertyValue, PropertyRowViewModel category)
    {
        return new PropertyRowViewModel
        {
            IsSectionHeader = false,
            PropertyName = propertyName,
            PropertyValue = propertyValue
        };
    }

    private class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute();
    }
}

