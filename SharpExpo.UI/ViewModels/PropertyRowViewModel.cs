using System.Windows.Input;
using SharpExpo.Contracts.Models;

namespace SharpExpo.UI.ViewModels;

/// <summary>
/// View model for a property row in the DataGrid.
/// Represents either a category header or a property row with its value.
/// </summary>
/// <remarks>
/// WHY: This class serves as the view model for individual rows in the properties DataGrid.
/// It handles both category headers (for grouping) and property rows (for displaying individual properties).
/// The separation of concerns allows the UI to bind to a consistent view model structure.
/// </remarks>
public class PropertyRowViewModel : ViewModelBase
{
    private bool _isExpanded;
    private string? _propertyValue;

    /// <summary>
    /// Gets or sets a value indicating whether this row is a section header (category).
    /// </summary>
    /// <value><see langword="true"/> if this row represents a category header; otherwise, <see langword="false"/>.</value>
    public bool IsSectionHeader { get; set; }

    /// <summary>
    /// Gets or sets the name of the section (for category headers).
    /// </summary>
    /// <value>The category name, or <see langword="null"/> if this is not a category header.</value>
    public string? SectionName { get; set; }

    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    /// <value>The property name, or <see langword="null"/> if this is a category header.</value>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the value of the property as a string.
    /// </summary>
    /// <value>The property value displayed in the UI, or <see langword="null"/> if not set.</value>
    /// <remarks>
    /// WHY: This property notifies the UI when the value changes, ensuring the DataGrid updates correctly.
    /// </remarks>
    public string? PropertyValue
    {
        get => _propertyValue;
        set
        {
            if (_propertyValue != value)
            {
                _propertyValue = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the property is locked (read-only).
    /// </summary>
    /// <value><see langword="true"/> if the property cannot be edited; otherwise, <see langword="false"/>.</value>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property has a category.
    /// </summary>
    /// <value><see langword="true"/> if the property belongs to a category; otherwise, <see langword="false"/>.</value>
    public bool HasCategory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property has a dropdown list.
    /// </summary>
    /// <value><see langword="true"/> if the property has a dropdown (e.g., for enumeration values); otherwise, <see langword="false"/>.</value>
    public bool HasDropdown { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the section is expanded.
    /// </summary>
    /// <value><see langword="true"/> if the category is expanded and its properties are visible; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// WHY: This property controls the visibility of properties within a category, providing a collapsible UI experience.
    /// </remarks>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the description of the property (for tooltips).
    /// </summary>
    /// <value>The property description, or <see langword="null"/> if not provided.</value>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the command to execute when toggling the section expansion state.
    /// </summary>
    /// <value>The toggle command, or <see langword="null"/> if this is not a category header.</value>
    public ICommand? ToggleExpandCommand { get; set; }

    /// <summary>
    /// Gets or sets the reference to the original <see cref="OptionProperty"/> for saving changes.
    /// </summary>
    /// <value>The original option property model, or <see langword="null"/> if this is a category header.</value>
    /// <remarks>
    /// WHY: This property maintains a reference to the domain model, allowing the save service to update the correct property
    /// when the user changes a value in the UI.
    /// </remarks>
    public OptionProperty? OriginalProperty { get; set; }

    /// <summary>
    /// Gets or sets the ID of the FamilyOption this property belongs to.
    /// </summary>
    /// <value>The family option ID, or <see langword="null"/> if not set.</value>
    /// <remarks>
    /// WHY: This property is needed to locate the correct family option in the JSON file when saving property values.
    /// </remarks>
    public string? FamilyOptionId { get; set; }

    /// <summary>
    /// Gets or sets the type of the property value.
    /// </summary>
    /// <value>The value type (String, Double, or Enumeration).</value>
    public OptionValueType ValueType { get; set; }

    /// <summary>
    /// Gets a value indicating whether this property can be edited.
    /// </summary>
    /// <value><see langword="true"/> if the property is editable; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// WHY: This property determines editability based on whether it's a header, locked, or an enumeration type.
    /// Enumeration types cannot be edited directly as they must use predefined values.
    /// </remarks>
    public bool IsEditable => !IsSectionHeader && !IsLocked &&
                              (ValueType == OptionValueType.String || ValueType == OptionValueType.Double);
}
