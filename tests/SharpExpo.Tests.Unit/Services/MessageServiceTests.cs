using SharpExpo.UI.Services;
using Xunit;

namespace SharpExpo.Tests.Unit.Services;

/// <summary>
/// Unit tests for the <see cref="MessageService"/> class.
/// Note: These tests verify that methods don't throw exceptions, but cannot test actual MessageBox display
/// without a WPF application context.
/// </summary>
public class MessageServiceTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Arrange & Act
        var messageService = new MessageService();

        // Assert
        Assert.NotNull(messageService);
    }

    [Fact]
    public void ShowInformation_WithValidMessage_DoesNotThrow()
    {
        // Arrange
        var messageService = new MessageService();
        var message = "Test information message";
        var title = "Test Title";

        // Act & Assert
        // Note: Without WPF application context, this will not display a message box,
        // but it should not throw an exception
        var exception = Record.Exception(() => messageService.ShowInformation(message, title));
        Assert.Null(exception);
    }

    [Fact]
    public void ShowInformation_WithDefaultTitle_DoesNotThrow()
    {
        // Arrange
        var messageService = new MessageService();
        var message = "Test information message";

        // Act & Assert
        var exception = Record.Exception(() => messageService.ShowInformation(message));
        Assert.Null(exception);
    }

    [Fact]
    public void ShowInformation_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var messageService = new MessageService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => messageService.ShowInformation(null!));
    }

    [Fact]
    public void ShowInformation_WithNullTitle_ThrowsArgumentNullException()
    {
        // Arrange
        var messageService = new MessageService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => messageService.ShowInformation("message", null!));
    }

    [Fact]
    public void ShowWarning_WithValidMessage_DoesNotThrow()
    {
        // Arrange
        var messageService = new MessageService();
        var message = "Test warning message";
        var title = "Test Title";

        // Act & Assert
        var exception = Record.Exception(() => messageService.ShowWarning(message, title));
        Assert.Null(exception);
    }

    [Fact]
    public void ShowWarning_WithDefaultTitle_DoesNotThrow()
    {
        // Arrange
        var messageService = new MessageService();
        var message = "Test warning message";

        // Act & Assert
        var exception = Record.Exception(() => messageService.ShowWarning(message));
        Assert.Null(exception);
    }

    [Fact]
    public void ShowWarning_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var messageService = new MessageService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => messageService.ShowWarning(null!));
    }

    [Fact]
    public void ShowWarning_WithNullTitle_ThrowsArgumentNullException()
    {
        // Arrange
        var messageService = new MessageService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => messageService.ShowWarning("message", null!));
    }

    [Fact]
    public void ShowError_WithValidMessage_DoesNotThrow()
    {
        // Arrange
        var messageService = new MessageService();
        var message = "Test error message";
        var title = "Test Title";

        // Act & Assert
        var exception = Record.Exception(() => messageService.ShowError(message, title));
        Assert.Null(exception);
    }

    [Fact]
    public void ShowError_WithDefaultTitle_DoesNotThrow()
    {
        // Arrange
        var messageService = new MessageService();
        var message = "Test error message";

        // Act & Assert
        var exception = Record.Exception(() => messageService.ShowError(message));
        Assert.Null(exception);
    }

    [Fact]
    public void ShowError_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var messageService = new MessageService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => messageService.ShowError(null!));
    }

    [Fact]
    public void ShowError_WithNullTitle_ThrowsArgumentNullException()
    {
        // Arrange
        var messageService = new MessageService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => messageService.ShowError("message", null!));
    }
}


