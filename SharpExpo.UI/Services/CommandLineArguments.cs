using System.IO;
using System.Linq;

namespace SharpExpo.UI.Services;

/// <summary>
/// Обработчик аргументов командной строки
/// </summary>
public static class CommandLineArguments
{
    private const string FamilyPathFlag = "--family-path";
    private const string DefaultFamiliesBasePath = @"C:\repos\sharpexpo\families";
    private const string FamilyOptionsFileName = "family-options.json";

    /// <summary>
    /// Парсит аргументы командной строки и возвращает путь к файлу family-options.json
    /// </summary>
    /// <param name="args">Аргументы командной строки</param>
    /// <returns>Путь к файлу family-options.json или null, если не найден</returns>
    public static string? ParseFamilyOptionsPath(string[] args)
    {
        // Ищем флаг --family-path
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == FamilyPathFlag && i + 1 < args.Length)
            {
                var path = args[i + 1];
                Logger.Log($"Найден параметр --family-path: {path}");
                
                var normalizedPath = Path.GetFullPath(path);
                if (File.Exists(normalizedPath))
                {
                    Logger.Log($"Файл найден (абсолютный путь): {normalizedPath}");
                    return normalizedPath;
                }
                else
                {
                    Logger.LogError($"Файл не найден по указанному пути: {normalizedPath}", null);
                    return null;
                }
            }
        }

        // Если флаг не указан, ищем первую директорию по алфавиту
        Logger.Log($"Параметр --family-path не указан, ищем первую директорию в {DefaultFamiliesBasePath}");
        
        if (!Directory.Exists(DefaultFamiliesBasePath))
        {
            Logger.LogError($"Базовая директория не найдена: {DefaultFamiliesBasePath}", null);
            return null;
        }

        // Получаем все поддиректории и сортируем по алфавиту
        var directories = Directory.GetDirectories(DefaultFamiliesBasePath)
            .OrderBy(d => d)
            .ToList();

        if (directories.Count == 0)
        {
            Logger.LogError($"В директории {DefaultFamiliesBasePath} не найдено поддиректорий", null);
            return null;
        }

        // Берем первую директорию по алфавиту
        var firstDirectory = directories[0];
        Logger.Log($"Первая директория по алфавиту: {firstDirectory}");

        var familyOptionsPath = Path.Combine(firstDirectory, FamilyOptionsFileName);
        var absolutePath = Path.GetFullPath(familyOptionsPath);
        
        if (File.Exists(absolutePath))
        {
            Logger.Log($"Файл найден (абсолютный путь): {absolutePath}");
            return absolutePath;
        }
        else
        {
            Logger.LogError($"Файл {FamilyOptionsFileName} не найден в директории: {firstDirectory}", null);
            return null;
        }
    }

    /// <summary>
    /// Получает путь к директории Families на основе пути к файлу family-options.json
    /// </summary>
    /// <param name="familyOptionsPath">Путь к файлу family-options.json</param>
    /// <returns>Путь к директории Families</returns>
    public static string GetFamiliesDirectory(string familyOptionsPath)
    {
        var directory = Path.GetDirectoryName(familyOptionsPath);
        return Path.Combine(directory ?? string.Empty, "Families");
    }
}

