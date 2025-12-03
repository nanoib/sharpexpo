using System.Windows.Input;
using SharpExpo.Contracts.Models;
using SharpExpo.UI.ViewModels;

namespace SharpExpo.UI.Services;

/// <summary>
/// Provides factory functionality for creating property row view models.
/// This interface abstracts view model creation to enable dependency injection and testability.
/// </summary>
public interface IPropertyViewModelFactory
{
    /// <summary>
    /// Creates a category header view model.
    /// </summary>
    /// <param name="categoryName">The name of the category.</param>
    /// <param name="toggleCommand">The command to execute when toggling the category expansion.</param>
    /// <returns>A new <see cref="PropertyRowViewModel"/> instance configured as a category header.</returns>
    PropertyRowViewModel CreateCategoryHeader(string categoryName, ICommand toggleCommand);

    /// <summary>
    /// Creates a property row view model from an option property.
    /// </summary>
    /// <param name="optionProperty">The option property to create the view model for.</param>
    /// <param name="familyOptionId">The ID of the family option this property belongs to.</param>
    /// <returns>A new <see cref="PropertyRowViewModel"/> instance configured for the property.</returns>
    PropertyRowViewModel CreatePropertyRow(OptionProperty optionProperty, string familyOptionId);
}

