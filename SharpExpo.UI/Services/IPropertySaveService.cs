using SharpExpo.Contracts.DTOs;
using SharpExpo.Contracts.Models;
using SharpExpo.UI.ViewModels;

namespace SharpExpo.UI.Services;

/// <summary>
/// Provides property value saving functionality.
/// This interface abstracts property saving logic to enable dependency injection and testability.
/// </summary>
public interface IPropertySaveService
{
    /// <summary>
    /// Saves a property value to the family-options.json file.
    /// </summary>
    /// <param name="propertyRow">The property row view model containing the property to save.</param>
    /// <param name="newValue">The new value to save.</param>
    /// <param name="familyOptionsFilePath">The path to the family-options.json file.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SavePropertyValueAsync(PropertyRowViewModel propertyRow, string newValue, string familyOptionsFilePath);
}


