using SharpExpo.Contracts.DTOs;
using SharpExpo.Contracts.Models;

namespace SharpExpo.Family.Mappers;

/// <summary>
/// Provides mapping functionality between DTOs and domain models.
/// This interface abstracts mapping operations to enable dependency injection and testability.
/// </summary>
public interface IDtoMapper
{
    /// <summary>
    /// Maps a <see cref="BimFamilyDto"/> to a <see cref="BimFamily"/>.
    /// </summary>
    /// <param name="dto">The DTO to map.</param>
    /// <returns>The mapped <see cref="BimFamily"/> instance, or <see langword="null"/> if the DTO is <see langword="null"/>.</returns>
    BimFamily? MapToBimFamily(BimFamilyDto? dto);

    /// <summary>
    /// Maps a <see cref="FamilyOptionItemDto"/> to a <see cref="FamilyOption"/>.
    /// </summary>
    /// <param name="dto">The DTO to map.</param>
    /// <returns>The mapped <see cref="FamilyOption"/> instance, or <see langword="null"/> if the DTO is invalid.</returns>
    FamilyOption? MapToFamilyOption(FamilyOptionItemDto dto);

    /// <summary>
    /// Maps a <see cref="OptionPropertyDto"/> to an <see cref="OptionProperty"/>.
    /// </summary>
    /// <param name="dto">The DTO to map.</param>
    /// <returns>The mapped <see cref="OptionProperty"/> instance, or <see langword="null"/> if the DTO is invalid.</returns>
    OptionProperty? MapToOptionProperty(OptionPropertyDto dto);

    /// <summary>
    /// Parses a string value type representation to an <see cref="OptionValueType"/>.
    /// </summary>
    /// <param name="valueTypeString">The string representation of the value type.</param>
    /// <returns>The parsed <see cref="OptionValueType"/>, or <see langword="null"/> if the string is invalid.</returns>
    OptionValueType? ParseValueType(string? valueTypeString);
}

