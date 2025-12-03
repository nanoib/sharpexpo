using System.IO;
using SharpExpo.Contracts;
using SharpExpo.Family;
using SharpExpo.Family.Mappers;
using SharpExpo.UI.Services;
using SharpExpo.UI.ViewModels;
using SharpExpo.UI.Windows.BimPropertiesWindow.ViewModels;

namespace SharpExpo.Tests.Unit.Helpers;

/// <summary>
/// Factory for creating test services and dependencies.
/// Provides helper methods to create services with test implementations.
/// </summary>
/// <remarks>
/// WHY: This class centralizes the creation of test dependencies, making tests easier to maintain
/// and ensuring consistent test setup across all test files.
/// </remarks>
public static class TestServiceFactory
{
    /// <summary>
    /// Creates a test logger instance.
    /// </summary>
    /// <returns>A logger instance for testing.</returns>
    public static ILogger CreateLogger()
    {
        return new Logger(Path.Combine(Path.GetTempPath(), $"test-log-{Guid.NewGuid()}.txt"));
    }

    /// <summary>
    /// Creates a test message service that does not display actual message boxes.
    /// </summary>
    /// <returns>A message service instance for testing.</returns>
    public static IMessageService CreateMessageService()
    {
        return new MessageService();
    }

    /// <summary>
    /// Creates a test file service instance.
    /// </summary>
    /// <returns>A file service instance for testing.</returns>
    public static IFileService CreateFileService()
    {
        return new FileService();
    }

    /// <summary>
    /// Creates a test property filter service instance.
    /// </summary>
    /// <returns>A property filter service instance for testing.</returns>
    public static IPropertyFilterService CreatePropertyFilterService()
    {
        return new PropertyFilterService();
    }

    /// <summary>
    /// Creates a test property save service instance.
    /// </summary>
    /// <param name="dataProvider">The data provider to use.</param>
    /// <returns>A property save service instance for testing.</returns>
    public static IPropertySaveService CreatePropertySaveService(IBimFamilyDataProvider dataProvider)
    {
        var logger = CreateLogger();
        var messageService = CreateMessageService();
        var fileService = CreateFileService();
        return new PropertySaveService(fileService, logger, messageService, dataProvider);
    }

    /// <summary>
    /// Creates a test property view model factory instance.
    /// </summary>
    /// <returns>A property view model factory instance for testing.</returns>
    public static IPropertyViewModelFactory CreatePropertyViewModelFactory()
    {
        return new PropertyViewModelFactory();
    }

    /// <summary>
    /// Creates a test data provider instance.
    /// </summary>
    /// <param name="familiesDirectory">The directory containing family JSON files.</param>
    /// <param name="familyOptionsFilePath">The path to the family-options.json file.</param>
    /// <returns>A data provider instance for testing.</returns>
    public static IBimFamilyDataProvider CreateDataProvider(string familiesDirectory, string familyOptionsFilePath)
    {
        var mapper = new DtoMapper();
        return new JsonBimFamilyDataProvider(familiesDirectory, familyOptionsFilePath, mapper);
    }

    /// <summary>
    /// Creates a test BimPropertiesWindowViewModel instance with all required dependencies.
    /// </summary>
    /// <param name="dataProvider">The data provider to use.</param>
    /// <param name="familyId">The family ID to load.</param>
    /// <param name="familyOptionsFilePath">The path to the family-options.json file.</param>
    /// <param name="familiesDirectory">The directory containing family JSON files.</param>
    /// <returns>A BimPropertiesWindowViewModel instance configured for testing.</returns>
    public static BimPropertiesWindowViewModel CreateMainWindowViewModel(
        IBimFamilyDataProvider dataProvider,
        string familyId,
        string familyOptionsFilePath,
        string familiesDirectory = "")
    {
        var logger = CreateLogger();
        var messageService = CreateMessageService();
        var filterService = CreatePropertyFilterService();
        var saveService = CreatePropertySaveService(dataProvider);
        var viewModelFactory = CreatePropertyViewModelFactory();

        return new BimPropertiesWindowViewModel(
            dataProvider,
            filterService,
            saveService,
            viewModelFactory,
            logger,
            messageService,
            familyId,
            familyOptionsFilePath,
            familiesDirectory);
    }
}

