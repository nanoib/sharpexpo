using System.IO;
using System.Text;

namespace SharpExpo.UI.Services;

/// <summary>
/// Default implementation of <see cref="IFileService"/> that performs file system operations.
/// </summary>
/// <remarks>
/// WHY: This class implements IFileService to abstract file operations from business logic.
/// This enables unit testing without actual file system access and allows for future replacement with different storage mechanisms.
/// </remarks>
public class FileService : IFileService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    public FileService()
    {
    }

    /// <inheritdoc/>
    public Task<string> ReadAllTextAsync(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return File.ReadAllTextAsync(path, Encoding.UTF8);
    }

    /// <inheritdoc/>
    public Task WriteAllTextAsync(string path, string contents)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(contents);
        return File.WriteAllTextAsync(path, contents, Encoding.UTF8);
    }

    /// <inheritdoc/>
    public bool FileExists(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return File.Exists(path);
    }

    /// <inheritdoc/>
    public string GetFullPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return Path.GetFullPath(path);
    }
}

