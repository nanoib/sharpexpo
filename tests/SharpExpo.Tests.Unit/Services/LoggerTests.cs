using System.IO;
using SharpExpo.UI.Services;
using Xunit;

namespace SharpExpo.Tests.Unit.Services;

/// <summary>
/// Unit tests for the <see cref="Logger"/> class.
/// </summary>
public class LoggerTests : IDisposable
{
    private readonly string _testLogFilePath;

    public LoggerTests()
    {
        _testLogFilePath = Path.Combine(Path.GetTempPath(), $"test-log-{Guid.NewGuid()}.txt");
    }

    [Fact]
    public void Constructor_WithCustomPath_SetsLogFilePath()
    {
        // Arrange & Act
        var logger = new Logger(_testLogFilePath);

        // Assert
        Assert.Equal(_testLogFilePath, logger.LogFilePath);
        // File is created when first log entry is written, not in constructor
        // Directory is created in constructor though
        Assert.True(Directory.Exists(Path.GetDirectoryName(_testLogFilePath)));
    }

    [Fact]
    public void Constructor_WithNullPath_UsesDefaultPath()
    {
        // Arrange & Act
        var logger = new Logger(null);

        // Assert
        Assert.NotNull(logger.LogFilePath);
        Assert.Contains("SharpExpo", logger.LogFilePath);
        Assert.Contains("log.txt", logger.LogFilePath);
    }

    [Fact]
    public void Constructor_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), $"test-log-dir-{Guid.NewGuid()}");
        var logPath = Path.Combine(testDir, "subdir", "log.txt");

        // Act
        var logger = new Logger(logPath);

        // Assert
        Assert.True(Directory.Exists(Path.GetDirectoryName(logPath)));
        Assert.Equal(logPath, logger.LogFilePath);

        // Cleanup
        if (Directory.Exists(testDir))
        {
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void Log_WritesMessageToFile()
    {
        // Arrange
        var logger = new Logger(_testLogFilePath);
        var message = "Test log message";

        // Act
        logger.Log(message);

        // Assert
        Assert.True(File.Exists(_testLogFilePath));
        var content = File.ReadAllText(_testLogFilePath);
        Assert.Contains(message, content);
        Assert.Contains(DateTime.Now.ToString("yyyy-MM-dd"), content);
    }

    [Fact]
    public void Log_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = new Logger(_testLogFilePath);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => logger.Log(null!));
    }

    [Fact]
    public void Log_AppendsMultipleMessages()
    {
        // Arrange
        var logger = new Logger(_testLogFilePath);
        var message1 = "First message";
        var message2 = "Second message";

        // Act
        logger.Log(message1);
        logger.Log(message2);

        // Assert
        var content = File.ReadAllText(_testLogFilePath);
        Assert.Contains(message1, content);
        Assert.Contains(message2, content);
        var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 2);
    }

    [Fact]
    public void LogError_WritesErrorMessageToFile()
    {
        // Arrange
        var logger = new Logger(_testLogFilePath);
        var errorMessage = "Test error message";

        // Act
        logger.LogError(errorMessage);

        // Assert
        var content = File.ReadAllText(_testLogFilePath);
        Assert.Contains("[ERROR]", content);
        Assert.Contains(errorMessage, content);
    }

    [Fact]
    public void LogError_WithException_IncludesExceptionDetails()
    {
        // Arrange
        var logger = new Logger(_testLogFilePath);
        var errorMessage = "Test error message";
        var exception = new InvalidOperationException("Test exception");

        // Act
        logger.LogError(errorMessage, exception);

        // Assert
        var content = File.ReadAllText(_testLogFilePath);
        Assert.Contains("[ERROR]", content);
        Assert.Contains(errorMessage, content);
        Assert.Contains("InvalidOperationException", content);
        Assert.Contains("Test exception", content);
    }

    [Fact]
    public void LogError_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = new Logger(_testLogFilePath);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => logger.LogError(null!));
    }

    [Fact]
    public void LogError_WithNullException_OnlyLogsMessage()
    {
        // Arrange
        var logger = new Logger(_testLogFilePath);
        var errorMessage = "Test error message";

        // Act
        logger.LogError(errorMessage, null);

        // Assert
        var content = File.ReadAllText(_testLogFilePath);
        Assert.Contains("[ERROR]", content);
        Assert.Contains(errorMessage, content);
        Assert.DoesNotContain("Exception:", content);
    }

    public void Dispose()
    {
        if (File.Exists(_testLogFilePath))
        {
            File.Delete(_testLogFilePath);
        }
    }
}

