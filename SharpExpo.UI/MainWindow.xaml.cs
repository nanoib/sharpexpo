using System.IO;
using System.Reflection;
using System.Windows;
using SharpExpo.Family;
using SharpExpo.UI.Services;
using SharpExpo.UI.ViewModels;

namespace SharpExpo.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            Logger.Log("Инициализация MainWindow...");
            InitializeComponent();
            Logger.Log("InitializeComponent выполнен");
            
            // Получаем пути к директориям и файлам
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            Logger.Log($"Директория сборки: {assemblyDirectory}");
            
            // Путь к директории с семействами
            var familiesDirectory = Path.Combine(assemblyDirectory ?? string.Empty, "Families");
            
            // Если директория не найдена, ищем в директории Family проекта
            if (!Directory.Exists(familiesDirectory))
            {
                var solutionDirectory = Directory.GetParent(assemblyDirectory ?? string.Empty)?.Parent?.FullName;
                Logger.Log($"Директория решения: {solutionDirectory}");
                
                if (!string.IsNullOrEmpty(solutionDirectory))
                {
                    var familyProjectFamiliesPath = Path.Combine(solutionDirectory, "SharpExpo.Family", "Families");
                    Logger.Log($"Проверка директории Families в Family проекте: {familyProjectFamiliesPath}, существует: {Directory.Exists(familyProjectFamiliesPath)}");
                    
                    if (Directory.Exists(familyProjectFamiliesPath))
                    {
                        familiesDirectory = familyProjectFamiliesPath;
                    }
                }
            }
            
            // Путь к файлу с опциями семейств
            var familyOptionsFilePath = Path.Combine(assemblyDirectory ?? string.Empty, "family-options.json");
            
            if (!File.Exists(familyOptionsFilePath))
            {
                var solutionDirectory = Directory.GetParent(assemblyDirectory ?? string.Empty)?.Parent?.FullName;
                if (!string.IsNullOrEmpty(solutionDirectory))
                {
                    var familyProjectOptionsPath = Path.Combine(solutionDirectory, "SharpExpo.Family", "family-options.json");
                    Logger.Log($"Проверка файла family-options.json в Family проекте: {familyProjectOptionsPath}, существует: {File.Exists(familyProjectOptionsPath)}");
                    
                    if (File.Exists(familyProjectOptionsPath))
                    {
                        familyOptionsFilePath = familyProjectOptionsPath;
                    }
                }
            }
            
            if (!Directory.Exists(familiesDirectory))
            {
                Logger.LogError($"Директория Families не найдена! Искали по пути: {familiesDirectory}");
                MessageBox.Show(
                    $"Директория Families не найдена!\n\nИскали по пути:\n{familiesDirectory}\n\nПроверьте, что директория Families существует.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            
            if (!File.Exists(familyOptionsFilePath))
            {
                Logger.LogError($"Файл family-options.json не найден! Искали по пути: {familyOptionsFilePath}");
                MessageBox.Show(
                    $"Файл family-options.json не найден!\n\nИскали по пути:\n{familyOptionsFilePath}\n\nПроверьте, что файл существует.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            
            Logger.Log($"Используется директория Families: {familiesDirectory}");
            Logger.Log($"Используется файл family-options.json: {familyOptionsFilePath}");
            
            // ID семейства для отображения (можно сделать настраиваемым)
            var defaultFamilyId = "9e1f8475-2aff-4293-9999-d0596e958d85";
            
            // Создаем провайдер данных и ViewModel
            var dataProvider = new JsonBimFamilyDataProvider(familiesDirectory, familyOptionsFilePath);
            var viewModel = new MainWindowViewModel(dataProvider, defaultFamilyId);
            
            DataContext = viewModel;
            Logger.Log("DataContext установлен");
            
            // Загружаем данные при старте
            Loaded += (s, e) =>
            {
                try
                {
                    Logger.Log("Окно загружено, начинаем загрузку данных...");
                    if (viewModel.LoadCommand.CanExecute(null))
                    {
                        viewModel.LoadCommand.Execute(null);
                        Logger.Log("Команда загрузки данных выполнена");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Ошибка при загрузке данных в Loaded event", ex);
                    MessageBox.Show(
                        $"Ошибка при загрузке данных: {ex.Message}\n\nДетали в логе: {Logger.LogFilePath}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            };
            
            Logger.Log("MainWindow инициализирован успешно");
        }
        catch (Exception ex)
        {
            Logger.LogError("Критическая ошибка при инициализации MainWindow", ex);
            MessageBox.Show(
                $"Критическая ошибка при инициализации окна: {ex.Message}\n\nДетали в логе: {Logger.LogFilePath}",
                "Критическая ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            throw;
        }
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SearchText = string.Empty;
        }
    }

    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Logger.Log("Окно закрывается");
    }
}
