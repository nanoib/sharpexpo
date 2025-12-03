using SharpExpo.Contracts.DTOs;
using SharpExpo.Contracts.Models;

namespace SharpExpo.Family.Mappers;

/// <summary>
/// Default implementation of <see cref="IDtoMapper"/> that converts DTOs to domain models.
/// </summary>
/// <remarks>
/// WHY: This class implements IDtoMapper to separate data transformation logic from data access logic.
/// This follows the Single Responsibility Principle and makes the code more testable and maintainable.
/// </remarks>
public class DtoMapper : IDtoMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DtoMapper"/> class.
    /// </summary>
    public DtoMapper()
    {
    }

    /// <inheritdoc/>
    public BimFamily? MapToBimFamily(BimFamilyDto? dto)
    {
        if (dto == null)
        {
            return null;
        }

        return new BimFamily
        {
            Id = dto.Id,
            Name = dto.Name,
            FamilyOptionIds = dto.FamilyOptionIds,
            CategoryOrder = dto.CategoryOrder
        };
    }

    /// <inheritdoc/>
    public FamilyOption? MapToFamilyOption(FamilyOptionItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            return null;
        }

        var familyOption = new FamilyOption
        {
            Id = dto.Id,
            OptionProperties = new List<OptionProperty>()
        };

        if (dto.OptionProperties != null)
        {
            foreach (var valueDto in dto.OptionProperties)
            {
                var optionProperty = MapToOptionProperty(valueDto);
                if (optionProperty != null)
                {
                    familyOption.OptionProperties.Add(optionProperty);
                }
            }
        }

        return familyOption;
    }

    /// <inheritdoc/>
    public OptionProperty? MapToOptionProperty(OptionPropertyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.PropertyName))
        {
            return null;
        }

        // Parse value type
        var valueType = ParseValueType(dto.ValueType);
        if (valueType == null)
        {
            return null;
        }

        var optionProperty = new OptionProperty
        {
            Id = dto.Id,
            PropertyName = dto.PropertyName,
            Description = dto.Description,
            ValueType = valueType.Value,
            CategoryName = dto.CategoryName ?? string.Empty
        };

        // Set value based on type
        SetPropertyValue(optionProperty, valueType.Value, dto.Value);

        return optionProperty;
    }

    /// <inheritdoc/>
    public OptionValueType? ParseValueType(string? valueTypeString)
    {
        if (string.IsNullOrWhiteSpace(valueTypeString))
        {
            return OptionValueType.String;
        }

        return valueTypeString.ToLowerInvariant() switch
        {
            "string" or "строка" => OptionValueType.String,
            "double" or "число" or "number" => OptionValueType.Double,
            "enumeration" or "перечисление" or "enum" => OptionValueType.Enumeration,
            _ => null
        };
    }

    /// <summary>
    /// Sets the property value based on its type.
    /// </summary>
    /// <param name="optionProperty">The option property to set the value for.</param>
    /// <param name="valueType">The type of the value.</param>
    /// <param name="value">The value to set (can be various types).</param>
    /// <remarks>
    /// WHY: This method handles type conversion for different value types, supporting multiple numeric formats
    /// and culture-specific parsing to ensure robust data loading from JSON files.
    /// </remarks>
    private static void SetPropertyValue(OptionProperty optionProperty, OptionValueType valueType, object? value)
    {
        switch (valueType)
        {
            case OptionValueType.String:
                // For String type, convert null to empty string
                optionProperty.StringValue = value?.ToString() ?? string.Empty;
                break;

            case OptionValueType.Double:
                if (value != null)
                {
                    // Try different parsing methods
                    double doubleVal = 0;
                    bool parsed = false;

                    // First try as number directly
                    if (value is double d)
                    {
                        doubleVal = d;
                        parsed = true;
                    }
                    else if (value is int i)
                    {
                        doubleVal = i;
                        parsed = true;
                    }
                    else if (value is long l)
                    {
                        doubleVal = l;
                        parsed = true;
                    }
                    else if (value is decimal dec)
                    {
                        doubleVal = (double)dec;
                        parsed = true;
                    }
                    else
                    {
                        // Try parsing string with different cultures
                        var strValue = value.ToString();
                        if (double.TryParse(strValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out doubleVal))
                        {
                            parsed = true;
                        }
                        else if (double.TryParse(strValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out doubleVal))
                        {
                            parsed = true;
                        }
                    }

                    if (parsed)
                    {
                        optionProperty.DoubleValue = doubleVal;
                    }
                }
                break;

            case OptionValueType.Enumeration:
                // For Enumeration type, convert null to empty string
                optionProperty.EnumValue = value?.ToString() ?? string.Empty;
                break;
        }
    }
}

