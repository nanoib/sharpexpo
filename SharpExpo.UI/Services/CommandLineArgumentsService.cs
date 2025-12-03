using System.IO;
using System.Linq;

namespace SharpExpo.UI.Services;

/// <summary>
/// Default implementation of <see cref="ICommandLineArgumentsService"/> that parses command line arguments.
/// </summary>
/// <remarks>
/// WHY: This class implements ICommandLineArgumentsService to abstract command line parsing from UI code.
/// This enables unit testing and allows for future extension with additional command line options.
/// </remarks>
public class CommandLineArgumentsService : ICommandLineArgumentsService
{
    private const string FamilyPathFlag = "--family-path";
    private const string DefaultFamiliesBasePath = @"C:\repos\sharpexpo\families";
    private const string FamilyOptionsFileName = "family-options.json";

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineArgumentsService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging operations.</param>
    public CommandLineArgumentsService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public string? ParseFamilyOptionsPath(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        // Look for --family-path flag
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == FamilyPathFlag && i + 1 < args.Length)
            {
                var path = args[i + 1];
                _logger.Log($"Найден параметр --family-path: {path}");

                var normalizedPath = Path.GetFullPath(path);
                if (File.Exists(normalizedPath))
                {
                    _logger.Log($"Файл найден (абсолютный путь): {normalizedPath}");
                    return normalizedPath;
                }
                else
                {
                    _logger.LogError($"Файл не найден по указанному пути: {normalizedPath}", null);
                    return null;
                }
            }
        }

        // If flag not specified, search for first directory alphabetically
        _logger.Log($"Параметр --family-path не указан, ищем первую директорию в {DefaultFamiliesBasePath}");

        if (!Directory.Exists(DefaultFamiliesBasePath))
        {
            _logger.LogError($"Базовая директория не найдена: {DefaultFamiliesBasePath}", null);
            return null;
        }

        // Get all subdirectories and sort alphabetically
        var directories = Directory.GetDirectories(DefaultFamiliesBasePath)
            .OrderBy(d => d)
            .ToList();

        if (directories.Count == 0)
        {
            _logger.LogError($"В директории {DefaultFamiliesBasePath} не найдено поддиректорий", null);
            return null;
        }

        // Take first directory alphabetically
        var firstDirectory = directories[0];
        _logger.Log($"Первая директория по алфавиту: {firstDirectory}");

        var familyOptionsPath = Path.Combine(firstDirectory, FamilyOptionsFileName);
        var absolutePath = Path.GetFullPath(familyOptionsPath);

        if (File.Exists(absolutePath))
        {
            _logger.Log($"Файл найден (абсолютный путь): {absolutePath}");
            return absolutePath;
        }
        else
        {
            _logger.LogError($"Файл {FamilyOptionsFileName} не найден в директории: {firstDirectory}", null);
            return null;
        }
    }

    /// <inheritdoc/>
    public string GetFamiliesDirectory(string familyOptionsPath)
    {
        ArgumentNullException.ThrowIfNull(familyOptionsPath);
        var directory = Path.GetDirectoryName(familyOptionsPath);
        return Path.Combine(directory ?? string.Empty, "Families");
    }
}

