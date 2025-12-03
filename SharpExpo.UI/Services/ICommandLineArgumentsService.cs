namespace SharpExpo.UI.Services;

/// <summary>
/// Provides command line argument parsing functionality.
/// This interface abstracts command line parsing to enable dependency injection and testability.
/// </summary>
public interface ICommandLineArgumentsService
{
    /// <summary>
    /// Parses command line arguments and returns the path to the family-options.json file.
    /// </summary>
    /// <param name="args">The command line arguments to parse.</param>
    /// <returns>The path to the family-options.json file, or <see langword="null"/> if not found.</returns>
    string? ParseFamilyOptionsPath(string[] args);

    /// <summary>
    /// Gets the Families directory path based on the family-options.json file path.
    /// </summary>
    /// <param name="familyOptionsPath">The path to the family-options.json file.</param>
    /// <returns>The path to the Families directory.</returns>
    string GetFamiliesDirectory(string familyOptionsPath);
}


