namespace SharpExpo.UI.Services;

/// <summary>
/// Provides file system operations.
/// This interface abstracts file operations to enable dependency injection and testability.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Reads all text from a file asynchronously.
    /// </summary>
    /// <param name="path">The path to the file to read.</param>
    /// <returns>A task that represents the asynchronous read operation. The task result contains the contents of the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied.</exception>
    Task<string> ReadAllTextAsync(string path);

    /// <summary>
    /// Writes all text to a file asynchronously.
    /// </summary>
    /// <param name="path">The path to the file to write.</param>
    /// <param name="contents">The contents to write to the file.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
    Task WriteAllTextAsync(string path, string contents);

    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><see langword="true"/> if the file exists; otherwise, <see langword="false"/>.</returns>
    bool FileExists(string path);

    /// <summary>
    /// Gets the full path for the specified path.
    /// </summary>
    /// <param name="path">The relative or absolute path to normalize.</param>
    /// <returns>The normalized absolute path.</returns>
    string GetFullPath(string path);
}


