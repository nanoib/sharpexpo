using System.Windows.Input;
using SharpExpo.Contracts.Models;
using SharpExpo.UI.ViewModels;

namespace SharpExpo.UI.Services;

/// <summary>
/// Default implementation of <see cref="IPropertyViewModelFactory"/> that creates property row view models.
/// </summary>
/// <remarks>
/// WHY: This class implements IPropertyViewModelFactory to separate view model creation logic from the main ViewModel.
/// This follows the Single Responsibility Principle and makes the creation logic testable independently.
/// </remarks>
public class PropertyViewModelFactory : IPropertyViewModelFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyViewModelFactory"/> class.
    /// </summary>
    public PropertyViewModelFactory()
    {
    }

    /// <inheritdoc/>
    public PropertyRowViewModel CreateCategoryHeader(string categoryName, System.Windows.Input.ICommand toggleCommand)
    {
        ArgumentNullException.ThrowIfNull(categoryName);
        ArgumentNullException.ThrowIfNull(toggleCommand);

        return new PropertyRowViewModel
        {
            IsSectionHeader = true,
            SectionName = categoryName,
            IsExpanded = true,
            ToggleExpandCommand = toggleCommand
        };
    }

    /// <inheritdoc/>
    public PropertyRowViewModel CreatePropertyRow(OptionProperty optionProperty, string familyOptionId)
    {
        ArgumentNullException.ThrowIfNull(optionProperty);
        ArgumentNullException.ThrowIfNull(familyOptionId);

        var displayValue = optionProperty.GetDisplayValue();

        return new PropertyRowViewModel
        {
            IsSectionHeader = false,
            PropertyName = optionProperty.PropertyName,
            PropertyValue = displayValue,
            Description = optionProperty.Description,
            IsLocked = optionProperty.ValueType == OptionValueType.Enumeration, // Enumeration cannot be edited
            HasCategory = !string.IsNullOrEmpty(optionProperty.CategoryName),
            HasDropdown = optionProperty.ValueType == OptionValueType.Enumeration,
            OriginalProperty = optionProperty,
            FamilyOptionId = familyOptionId,
            ValueType = optionProperty.ValueType
        };
    }
}

