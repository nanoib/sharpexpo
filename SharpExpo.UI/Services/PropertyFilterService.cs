using SharpExpo.UI.ViewModels;

namespace SharpExpo.UI.Services;

/// <summary>
/// Default implementation of <see cref="IPropertyFilterService"/> that filters properties based on search text and category expansion.
/// </summary>
/// <remarks>
/// WHY: This class implements IPropertyFilterService to separate filtering logic from the ViewModel.
/// This follows the Single Responsibility Principle and makes the filtering logic testable independently.
/// </remarks>
public class PropertyFilterService : IPropertyFilterService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyFilterService"/> class.
    /// </summary>
    public PropertyFilterService()
    {
    }

    /// <inheritdoc/>
    public List<PropertyRowViewModel> FilterProperties(List<PropertyRowViewModel> allProperties, string searchText)
    {
        ArgumentNullException.ThrowIfNull(allProperties);

        if (string.IsNullOrWhiteSpace(searchText))
        {
            // If search is empty, show all properties respecting category expansion
            return FilterByCategoryExpansion(allProperties);
        }

        // Filter by search text
        var searchLower = searchText.ToLowerInvariant();
        var filtered = new List<PropertyRowViewModel>();

        foreach (var prop in allProperties)
        {
            bool matches = false;
            if (prop.IsSectionHeader)
            {
                matches = prop.SectionName?.ToLowerInvariant().Contains(searchLower) ?? false;
            }
            else
            {
                matches = (prop.PropertyName?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                         (prop.PropertyValue?.ToLowerInvariant().Contains(searchLower) ?? false);
            }

            if (matches)
            {
                filtered.Add(prop);
            }
        }

        return filtered;
    }

    /// <summary>
    /// Filters properties based on category expansion state.
    /// </summary>
    /// <param name="allProperties">The complete list of properties.</param>
    /// <returns>A filtered list showing only expanded categories and their properties.</returns>
    /// <remarks>
    /// WHY: This method handles the case when no search is active, showing only properties from expanded categories.
    /// This provides a clean UI experience where users can collapse categories they're not interested in.
    /// </remarks>
    private static List<PropertyRowViewModel> FilterByCategoryExpansion(List<PropertyRowViewModel> allProperties)
    {
        var filtered = new List<PropertyRowViewModel>();

        foreach (var prop in allProperties)
        {
            if (prop.IsSectionHeader)
            {
                filtered.Add(prop);
            }
            else
            {
                // Find parent category
                var categoryHeader = FindCategoryHeader(prop, allProperties);
                if (categoryHeader != null && categoryHeader.IsExpanded)
                {
                    filtered.Add(prop);
                }
            }
        }

        return filtered;
    }

    /// <summary>
    /// Finds the category header for a given property.
    /// </summary>
    /// <param name="property">The property to find the category header for.</param>
    /// <param name="allProperties">The complete list of properties.</param>
    /// <returns>The category header, or <see langword="null"/> if not found.</returns>
    private static PropertyRowViewModel? FindCategoryHeader(PropertyRowViewModel property, List<PropertyRowViewModel> allProperties)
    {
        var propertyIndex = allProperties.IndexOf(property);
        if (propertyIndex < 0)
        {
            return null;
        }

        // Search backwards for category header
        for (int i = propertyIndex - 1; i >= 0; i--)
        {
            if (allProperties[i].IsSectionHeader)
            {
                return allProperties[i];
            }
        }

        return null;
    }
}




