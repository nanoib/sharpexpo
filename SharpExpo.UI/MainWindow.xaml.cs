using System.IO;
using System.Linq;
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
            
            // Получаем аргументы командной строки
            var args = Environment.GetCommandLineArgs();
            Logger.Log($"Аргументы командной строки: {string.Join(" ", args)}");
            
            // Парсим путь к файлу family-options.json
            var familyOptionsFilePath = CommandLineArguments.ParseFamilyOptionsPath(args);
            
            if (string.IsNullOrEmpty(familyOptionsFilePath) || !File.Exists(familyOptionsFilePath))
            {
                Logger.LogError($"Файл family-options.json не найден!", null);
                MessageBox.Show(
                    $"Файл family-options.json не найден!\n\n" +
                    $"Использование:\n" +
                    $"  SharpExpo.UI.exe --family-path \"C:\\path\\to\\family-options.json\"\n\n" +
                    $"Или поместите данные в: C:\\repos\\sharpexpo\\families\\<directory>\\family-options.json",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            
            // Получаем директорию Families на основе пути к файлу family-options.json
            var familiesDirectory = CommandLineArguments.GetFamiliesDirectory(familyOptionsFilePath);
            
            if (!Directory.Exists(familiesDirectory))
            {
                Logger.LogError($"Директория Families не найдена! Искали по пути: {familiesDirectory}");
                MessageBox.Show(
                    $"Директория Families не найдена!\n\nИскали по пути:\n{familiesDirectory}\n\n" +
                    $"Убедитесь, что рядом с файлом family-options.json находится директория Families с JSON файлами семейств.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            
            Logger.Log($"Используется директория Families: {familiesDirectory}");
            Logger.Log($"Используется файл family-options.json: {familyOptionsFilePath}");
            
            // Получаем ID первого семейства из директории Families
            var familyFiles = Directory.GetFiles(familiesDirectory, "*.json")
                .OrderBy(f => f)
                .ToList();
            
            if (familyFiles.Count == 0)
            {
                Logger.LogError($"В директории Families не найдено JSON файлов семейств: {familiesDirectory}", null);
                MessageBox.Show(
                    $"В директории Families не найдено JSON файлов семейств!\n\nПуть: {familiesDirectory}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            
            // Загружаем первое семейство для получения его ID
            var firstFamilyFile = familyFiles[0];
            Logger.Log($"Загружаем семейство из файла: {firstFamilyFile}");
            
            try
            {
                var familyJson = File.ReadAllText(firstFamilyFile);
                var familyDto = System.Text.Json.JsonSerializer.Deserialize<SharpExpo.Contracts.DTOs.BimFamilyDto>(
                    familyJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (familyDto == null || string.IsNullOrEmpty(familyDto.Id))
                {
                    Logger.LogError($"Не удалось загрузить ID семейства из файла: {firstFamilyFile}", null);
                    MessageBox.Show(
                        $"Не удалось загрузить ID семейства из файла!\n\nПуть: {firstFamilyFile}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                
                var familyId = familyDto.Id;
                Logger.Log($"ID семейства для отображения: {familyId}");
                
                // Создаем провайдер данных и ViewModel
                var dataProvider = new JsonBimFamilyDataProvider(familiesDirectory, familyOptionsFilePath);
                var viewModel = new MainWindowViewModel(dataProvider, familyId, familyOptionsFilePath, familiesDirectory);
                
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
                Logger.LogError($"Ошибка при загрузке семейства из файла: {firstFamilyFile}", ex);
                MessageBox.Show(
                    $"Ошибка при загрузке семейства: {ex.Message}\n\nДетали в логе: {Logger.LogFilePath}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

    private void TriangleIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBlock textBlock && 
            textBlock.DataContext is PropertyRowViewModel viewModel &&
            viewModel.IsSectionHeader &&
            viewModel.ToggleExpandCommand != null &&
            viewModel.ToggleExpandCommand.CanExecute(null))
        {
            viewModel.ToggleExpandCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void PropertiesDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.DataGrid dataGrid && dataGrid.SelectedCells.Count > 0)
        {
            var selectedCell = dataGrid.SelectedCells[0];
            // Если выделена ячейка во втором столбце, переключаем выделение на первый столбец
            if (selectedCell.Column != null && selectedCell.Column.DisplayIndex == 1)
            {
                var row = selectedCell.Item;
                if (row != null)
                {
                    dataGrid.SelectedCells.Clear();
                    dataGrid.CurrentCell = new System.Windows.Controls.DataGridCellInfo(row, dataGrid.Columns[0]);
                    dataGrid.SelectedCells.Add(new System.Windows.Controls.DataGridCellInfo(row, dataGrid.Columns[0]));
                }
            }
        }
    }

    private bool _isSaving = false; // Флаг для предотвращения двойного сохранения
    
    private async void PropertyValueTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter && sender is System.Windows.Controls.TextBox textBox)
        {
            e.Handled = true;
            
            // Предотвращаем двойное сохранение
            if (_isSaving)
            {
                Logger.Log("Сохранение уже выполняется, пропускаем...");
                return;
            }
            
            _isSaving = true;
            try
            {
                // Обновляем привязку вручную перед сохранением
                var bindingExpression = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
                bindingExpression?.UpdateSource();
                
                // Сохраняем значение
                await SavePropertyValue(textBox);
                
                // Перемещаем фокус после сохранения
                textBox.MoveFocus(new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
            }
            finally
            {
                _isSaving = false;
            }
        }
    }

    private async void PropertyValueTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            // Предотвращаем двойное сохранение (если уже сохраняем через KeyDown)
            if (_isSaving)
            {
                Logger.Log("Сохранение уже выполняется через KeyDown, пропускаем LostFocus...");
                return;
            }
            
            _isSaving = true;
            try
            {
                // Обновляем привязку вручную перед сохранением
                var bindingExpression = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
                bindingExpression?.UpdateSource();
                
                await SavePropertyValue(textBox);
            }
            finally
            {
                _isSaving = false;
            }
        }
    }

    private async Task SavePropertyValue(System.Windows.Controls.TextBox textBox)
    {
        if (textBox.DataContext is PropertyRowViewModel propertyRow && 
            DataContext is MainWindowViewModel viewModel)
        {
            // Используем значение из TextBox напрямую, так как привязка может еще не обновиться
            var newValue = textBox.Text;
            var oldValue = propertyRow.PropertyValue;
            
            Logger.Log($"SavePropertyValue вызван: OldValue='{oldValue}', NewValue='{newValue}'");
            Logger.Log($"TextBox.Text='{textBox.Text}', PropertyRow.PropertyValue='{propertyRow.PropertyValue}'");
            
            // ВСЕГДА сохраняем значение из TextBox, даже если PropertyValue уже обновлен
            // Это гарантирует, что файл будет обновлен с актуальным значением из UI
            Logger.Log($"Сохраняем значение из TextBox: '{newValue}'");
            await viewModel.SavePropertyValueAsync(propertyRow, newValue);
            
            // После сохранения обновляем привязку, чтобы UI синхронизировался
            var bindingExpression = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            bindingExpression?.UpdateTarget();
        }
        else
        {
            Logger.LogError("SavePropertyValue: не удалось получить propertyRow или viewModel", null);
        }
    }
}
