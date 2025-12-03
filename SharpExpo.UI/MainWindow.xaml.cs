using System.IO;
using System.Reflection;
using System.Windows;
using SharpExpo.Family;
using SharpExpo.UI.ViewModels;

namespace SharpExpo.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Получаем путь к JSON файлу
        // Сначала ищем в выходной директории UI проекта
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        var jsonPath = Path.Combine(assemblyDirectory ?? string.Empty, "data.json");
        
        // Если файл не найден, ищем в директории Family проекта
        if (!File.Exists(jsonPath))
        {
            var solutionDirectory = Directory.GetParent(assemblyDirectory ?? string.Empty)?.Parent?.FullName;
            if (!string.IsNullOrEmpty(solutionDirectory))
            {
                var familyJsonPath = Path.Combine(solutionDirectory, "SharpExpo.Family", "data.json");
                if (File.Exists(familyJsonPath))
                {
                    jsonPath = familyJsonPath;
                }
            }
        }
        
        // Создаем провайдер данных и ViewModel
        var dataProvider = new JsonBimDataProvider(jsonPath);
        var viewModel = new MainWindowViewModel(dataProvider);
        
        DataContext = viewModel;
        
        // Загружаем данные при старте
        Loaded += (s, e) =>
        {
            if (viewModel.LoadCommand.CanExecute(null))
            {
                viewModel.LoadCommand.Execute(null);
            }
        };
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SearchText = string.Empty;
        }
    }
}
