using Microsoft.Extensions.DependencyInjection;
using SharpExpo.Contracts;
using SharpExpo.Family;
using SharpExpo.Family.Mappers;
using SharpExpo.UI.ViewModels;

namespace SharpExpo.UI.Services;

/// <summary>
/// Provides dependency injection container setup and service location.
/// This class configures all services and their dependencies for the application.
/// </summary>
/// <remarks>
/// WHY: This class centralizes dependency injection configuration, making it easy to manage service lifetimes
/// and dependencies. It follows the Service Locator pattern for WPF applications where constructor injection
/// is not always available (e.g., in code-behind).
/// </remarks>
public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// Gets the service provider instance.
    /// </summary>
    /// <value>The service provider that resolves service dependencies.</value>
    /// <exception cref="InvalidOperationException">Thrown when the service provider has not been initialized.</exception>
    public static IServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("ServiceProvider has not been initialized. Call ConfigureServices first.");
            }
            return _serviceProvider;
        }
    }

    /// <summary>
    /// Configures the dependency injection container with all required services.
    /// </summary>
    /// <param name="familiesDirectory">The directory containing family JSON files.</param>
    /// <param name="familyOptionsFilePath">The path to the family-options.json file.</param>
    /// <returns>The configured service provider.</returns>
    /// <remarks>
    /// WHY: This method sets up all services with appropriate lifetimes:
    /// - Singleton: Services that should have one instance for the entire application (Logger, MessageService)
    /// - Transient: Services that should be created fresh each time (ViewModels, Factories)
    /// - Scoped: Services that should have one instance per scope (not used in this WPF app, but available)
    /// </remarks>
    public static IServiceProvider ConfigureServices(string familiesDirectory, string familyOptionsFilePath)
    {
        var services = new ServiceCollection();

        // Core services (Singleton - one instance for the app)
        services.AddSingleton<ILogger>(sp => new Logger());
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<IFileService, FileService>();

        // Mappers (Singleton - stateless, can be shared)
        services.AddSingleton<IDtoMapper, DtoMapper>();

        // Data providers (Singleton - can cache data)
        services.AddSingleton<IBimFamilyDataProvider>(sp =>
        {
            var mapper = sp.GetRequiredService<IDtoMapper>();
            return new JsonBimFamilyDataProvider(familiesDirectory, familyOptionsFilePath, mapper);
        });

        // Business services (Transient - fresh instance each time)
        services.AddTransient<ICommandLineArgumentsService, CommandLineArgumentsService>();
        services.AddTransient<IPropertyFilterService, PropertyFilterService>();
        services.AddTransient<IPropertySaveService, PropertySaveService>();
        services.AddTransient<IPropertyViewModelFactory, PropertyViewModelFactory>();

        // ViewModels (Transient - fresh instance each time)
        services.AddTransient<MainWindowViewModel>();

        _serviceProvider = services.BuildServiceProvider();
        return _serviceProvider;
    }

    /// <summary>
    /// Gets a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <returns>The service instance of type <typeparamref name="T"/>.</returns>
    public static T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }
}

