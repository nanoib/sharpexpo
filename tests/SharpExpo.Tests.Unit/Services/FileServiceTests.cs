using System.IO;
using System.Text;
using System.Threading.Tasks;
using SharpExpo.UI.Services;
using Xunit;

namespace SharpExpo.Tests.Unit.Services;

/// <summary>
/// Unit tests for the <see cref="FileService"/> class.
/// </summary>
public class FileServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testFilePath;

    public FileServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"test-fileservice-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _testFilePath = Path.Combine(_testDirectory, "test.txt");
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Arrange & Act
        var fileService = new FileService();

        // Assert
        Assert.NotNull(fileService);
    }

    [Fact]
    public async Task ReadAllTextAsync_ReadsFileContent()
    {
        // Arrange
        var fileService = new FileService();
        var expectedContent = "Test file content";
        await File.WriteAllTextAsync(_testFilePath, expectedContent, Encoding.UTF8);

        // Act
        var content = await fileService.ReadAllTextAsync(_testFilePath);

        // Assert
        Assert.Equal(expectedContent, content);
    }

    [Fact]
    public async Task ReadAllTextAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var fileService = new FileService();
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => fileService.ReadAllTextAsync(nonExistentPath));
    }

    [Fact]
    public async Task ReadAllTextAsync_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        var fileService = new FileService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => fileService.ReadAllTextAsync(null!));
    }

    [Fact]
    public async Task WriteAllTextAsync_WritesFileContent()
    {
        // Arrange
        var fileService = new FileService();
        var content = "Test content to write";

        // Act
        await fileService.WriteAllTextAsync(_testFilePath, content);

        // Assert
        Assert.True(File.Exists(_testFilePath));
        var writtenContent = await File.ReadAllTextAsync(_testFilePath, Encoding.UTF8);
        Assert.Equal(content, writtenContent);
    }

    [Fact]
    public async Task WriteAllTextAsync_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var fileService = new FileService();
        var subDirPath = Path.Combine(_testDirectory, "subdir", "test.txt");
        var content = "Test content";

        // Act
        await fileService.WriteAllTextAsync(subDirPath, content);

        // Assert
        Assert.True(File.Exists(subDirPath));
        var writtenContent = await File.ReadAllTextAsync(subDirPath, Encoding.UTF8);
        Assert.Equal(content, writtenContent);
    }

    [Fact]
    public async Task WriteAllTextAsync_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        var fileService = new FileService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => fileService.WriteAllTextAsync(null!, "content"));
    }

    [Fact]
    public async Task WriteAllTextAsync_WithNullContent_ThrowsArgumentNullException()
    {
        // Arrange
        var fileService = new FileService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => fileService.WriteAllTextAsync(_testFilePath, null!));
    }

    [Fact]
    public void FileExists_WithExistingFile_ReturnsTrue()
    {
        // Arrange
        var fileService = new FileService();
        File.WriteAllText(_testFilePath, "test");

        // Act
        var exists = fileService.FileExists(_testFilePath);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void FileExists_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        var fileService = new FileService();
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act
        var exists = fileService.FileExists(nonExistentPath);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void FileExists_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        var fileService = new FileService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => fileService.FileExists(null!));
    }

    [Fact]
    public void GetFullPath_WithRelativePath_ReturnsAbsolutePath()
    {
        // Arrange
        var fileService = new FileService();
        var relativePath = "test.txt";

        // Act
        var fullPath = fileService.GetFullPath(relativePath);

        // Assert
        Assert.True(Path.IsPathRooted(fullPath));
        Assert.Contains("test.txt", fullPath);
    }

    [Fact]
    public void GetFullPath_WithAbsolutePath_ReturnsSamePath()
    {
        // Arrange
        var fileService = new FileService();
        var absolutePath = Path.GetFullPath(_testFilePath);

        // Act
        var fullPath = fileService.GetFullPath(absolutePath);

        // Assert
        Assert.Equal(absolutePath, fullPath);
    }

    [Fact]
    public void GetFullPath_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        var fileService = new FileService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => fileService.GetFullPath(null!));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}

