using SharpExpo.UI.ViewModels;

namespace SharpExpo.UI.Services;

/// <summary>
/// Provides property filtering functionality for the properties view.
/// This interface abstracts filtering logic to enable dependency injection and testability.
/// </summary>
public interface IPropertyFilterService
{
    /// <summary>
    /// Filters properties based on the search text and category expansion state.
    /// </summary>
    /// <param name="allProperties">The complete list of properties to filter.</param>
    /// <param name="searchText">The search text to filter by. If empty or whitespace, shows all properties respecting category expansion.</param>
    /// <returns>A filtered list of properties that match the search criteria or are visible based on category expansion.</returns>
    List<PropertyRowViewModel> FilterProperties(List<PropertyRowViewModel> allProperties, string searchText);
}

