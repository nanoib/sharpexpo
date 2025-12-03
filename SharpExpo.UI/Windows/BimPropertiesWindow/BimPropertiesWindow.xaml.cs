using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SharpExpo.Contracts;
using SharpExpo.Contracts.DTOs;
using SharpExpo.UI.Services;
using SharpExpo.UI.ViewModels;
using SharpExpo.UI.Windows.BimPropertiesWindow.ViewModels;

namespace SharpExpo.UI.Windows.BimPropertiesWindow;

/// <summary>
/// Interaction logic for BimPropertiesWindow.xaml.
/// </summary>
/// <remarks>
/// WHY: This class handles UI initialization and event handling, delegating business logic to services and ViewModels.
/// It uses dependency injection to resolve services, maintaining separation of concerns.
/// </remarks>
public partial class BimPropertiesWindow : Window
{
    private readonly ILogger _logger;
    private readonly IMessageService _messageService;
    private readonly ICommandLineArgumentsService _commandLineService;
    private readonly IFileService _fileService;
    private bool _isSaving = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="BimPropertiesWindow"/> class.
    /// </summary>
    public BimPropertiesWindow()
    {
        try
        {
            // Initialize services first (before using them)
            _logger = new Logger();
            _messageService = new MessageService();
            _fileService = new FileService();
            _commandLineService = new CommandLineArgumentsService(_logger);

            _logger.Log("Инициализация BimPropertiesWindow...");
            InitializeComponent();
            _logger.Log("InitializeComponent выполнен");

            // Parse command line arguments
            var args = Environment.GetCommandLineArgs();
            _logger.Log($"Аргументы командной строки: {string.Join(" ", args)}");

            var familyOptionsFilePath = _commandLineService.ParseFamilyOptionsPath(args);

            if (string.IsNullOrEmpty(familyOptionsFilePath) || !_fileService.FileExists(familyOptionsFilePath))
            {
                _logger.LogError($"Файл family-options.json не найден!", null);
                _messageService.ShowError(
                    $"Файл family-options.json не найден!\n\n" +
                    $"Использование:\n" +
                    $"  SharpExpo.UI.exe --family-path \"C:\\path\\to\\family-options.json\"\n\n" +
                    $"Или поместите данные в: C:\\repos\\sharpexpo\\families\\<directory>\\family-options.json",
                    "Ошибка");
                return;
            }

            // Get Families directory
            var familiesDirectory = _commandLineService.GetFamiliesDirectory(familyOptionsFilePath);

            if (!Directory.Exists(familiesDirectory))
            {
                _logger.LogError($"Директория Families не найдена! Искали по пути: {familiesDirectory}", null);
                _messageService.ShowError(
                    $"Директория Families не найдена!\n\nИскали по пути:\n{familiesDirectory}\n\n" +
                    $"Убедитесь, что рядом с файлом family-options.json находится директория Families с JSON файлами семейств.",
                    "Ошибка");
                return;
            }

            _logger.Log($"Используется директория Families: {familiesDirectory}");
            _logger.Log($"Используется файл family-options.json: {familyOptionsFilePath}");

            // Get ID of first family from Families directory
            var familyId = LoadFirstFamilyId(familiesDirectory);
            if (string.IsNullOrEmpty(familyId))
            {
                return; // Error already logged and shown
            }

            // Configure services with paths
            var serviceProvider = ServiceLocator.ConfigureServices(familiesDirectory, familyOptionsFilePath);

            // Create ViewModel using DI
            // Note: We need to pass familyId, familyOptionsFilePath, and familiesDirectory to the constructor
            // Since these are runtime values, we create the ViewModel manually with resolved services
            var dataProvider = serviceProvider.GetRequiredService<IBimFamilyDataProvider>();
            var filterService = serviceProvider.GetRequiredService<IPropertyFilterService>();
            var saveService = serviceProvider.GetRequiredService<IPropertySaveService>();
            var viewModelFactory = serviceProvider.GetRequiredService<IPropertyViewModelFactory>();

            var viewModel = new BimPropertiesWindowViewModel(
                dataProvider,
                filterService,
                saveService,
                viewModelFactory,
                _logger,
                _messageService,
                familyId,
                familyOptionsFilePath,
                familiesDirectory);

            DataContext = viewModel;
            _logger.Log("DataContext установлен");

            // Load data on startup
            Loaded += (s, e) =>
            {
                try
                {
                    _logger.Log("Окно загружено, начинаем загрузку данных...");
                    if (viewModel.LoadCommand.CanExecute(null))
                    {
                        viewModel.LoadCommand.Execute(null);
                        _logger.Log("Команда загрузки данных выполнена");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Ошибка при загрузке данных в Loaded event", ex);
                    _messageService.ShowError(
                        $"Ошибка при загрузке данных: {ex.Message}\n\nДетали в логе: {_logger.LogFilePath}",
                        "Ошибка");
                }
            };

            _logger.Log("BimPropertiesWindow инициализирован успешно");
        }
        catch (Exception ex)
        {
            _logger?.LogError("Критическая ошибка при инициализации BimPropertiesWindow", ex);
            _messageService?.ShowError(
                $"Критическая ошибка при инициализации окна: {ex.Message}\n\nДетали в логе: {_logger?.LogFilePath ?? "неизвестно"}",
                "Критическая ошибка");
            throw;
        }
    }

    /// <summary>
    /// Loads the ID of the first family from the Families directory.
    /// </summary>
    /// <param name="familiesDirectory">The directory containing family JSON files.</param>
    /// <returns>The family ID, or <see langword="null"/> if not found or an error occurred.</returns>
    /// <remarks>
    /// WHY: This method extracts the family ID loading logic to keep the constructor cleaner.
    /// </remarks>
    private string? LoadFirstFamilyId(string familiesDirectory)
    {
        var familyFiles = Directory.GetFiles(familiesDirectory, "*.json")
            .OrderBy(f => f)
            .ToList();

        if (familyFiles.Count == 0)
        {
            _logger.LogError($"В директории Families не найдено JSON файлов семейств: {familiesDirectory}", null);
            _messageService.ShowError(
                $"В директории Families не найдено JSON файлов семейств!\n\nПуть: {familiesDirectory}",
                "Ошибка");
            return null;
        }

        var firstFamilyFile = familyFiles[0];
        _logger.Log($"Загружаем семейство из файла: {firstFamilyFile}");

        try
        {
            var familyJson = _fileService.ReadAllTextAsync(firstFamilyFile).GetAwaiter().GetResult();
            var familyDto = System.Text.Json.JsonSerializer.Deserialize<BimFamilyDto>(
                familyJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (familyDto == null || string.IsNullOrEmpty(familyDto.Id))
            {
                _logger.LogError($"Не удалось загрузить ID семейства из файла: {firstFamilyFile}", null);
                _messageService.ShowError(
                    $"Не удалось загрузить ID семейства из файла!\n\nПуть: {firstFamilyFile}",
                    "Ошибка");
                return null;
            }

            _logger.Log($"ID семейства для отображения: {familyDto.Id}");
            return familyDto.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при загрузке семейства из файла: {firstFamilyFile}", ex);
            _messageService.ShowError(
                $"Ошибка при загрузке семейства: {ex.Message}\n\nДетали в логе: {_logger.LogFilePath}",
                "Ошибка");
            return null;
        }
    }

    /// <summary>
    /// Handles the ClearSearch button click event.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is BimPropertiesWindowViewModel viewModel)
        {
            viewModel.SearchText = string.Empty;
        }
    }

    /// <summary>
    /// Handles the BimPropertiesWindow closing event.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void BimPropertiesWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _logger.Log("Окно закрывается");
    }

    /// <summary>
    /// Handles the triangle icon mouse down event for category expansion.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
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

    /// <summary>
    /// Handles the PropertiesDataGrid selection changed event.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    /// <remarks>
    /// WHY: This method ensures that when a cell in the second column is selected, the selection moves to the first column.
    /// This provides a better UX by keeping focus on the property name column.
    /// </remarks>
    private void PropertiesDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.DataGrid dataGrid && dataGrid.SelectedCells.Count > 0)
        {
            var selectedCell = dataGrid.SelectedCells[0];
            // If second column is selected, switch to first column
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

    /// <summary>
    /// Handles the PropertyValueTextBox key down event for saving on Enter.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private async void PropertyValueTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter && sender is System.Windows.Controls.TextBox textBox)
        {
            e.Handled = true;

            // Prevent double saving
            if (_isSaving)
            {
                _logger.Log("Сохранение уже выполняется, пропускаем...");
                return;
            }

            _isSaving = true;
            try
            {
                // Update binding manually before saving
                var bindingExpression = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
                bindingExpression?.UpdateSource();

                // Save value
                await SavePropertyValue(textBox);

                // Move focus after saving
                textBox.MoveFocus(new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
            }
            finally
            {
                _isSaving = false;
            }
        }
    }

    /// <summary>
    /// Handles the PropertyValueTextBox lost focus event for saving on focus loss.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private async void PropertyValueTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            // Prevent double saving (if already saving through KeyDown)
            if (_isSaving)
            {
                _logger.Log("Сохранение уже выполняется через KeyDown, пропускаем LostFocus...");
                return;
            }

            _isSaving = true;
            try
            {
                // Update binding manually before saving
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

    /// <summary>
    /// Saves the property value from the text box.
    /// </summary>
    /// <param name="textBox">The text box containing the value to save.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <remarks>
    /// WHY: This method handles saving property values, using the value directly from the TextBox to ensure
    /// the most up-to-date value is saved, even if the binding hasn't updated yet.
    /// </remarks>
    private async Task SavePropertyValue(System.Windows.Controls.TextBox textBox)
    {
        if (textBox.DataContext is PropertyRowViewModel propertyRow &&
            DataContext is BimPropertiesWindowViewModel viewModel)
        {
            // Use value from TextBox directly, as binding may not have updated yet
            var newValue = textBox.Text;
            var oldValue = propertyRow.PropertyValue;

            _logger.Log($"SavePropertyValue вызван: OldValue='{oldValue}', NewValue='{newValue}'");
            _logger.Log($"TextBox.Text='{textBox.Text}', PropertyRow.PropertyValue='{propertyRow.PropertyValue}'");

            // ALWAYS save value from TextBox, even if PropertyValue is already updated
            // This ensures the file is updated with the actual value from the UI
            _logger.Log($"Сохраняем значение из TextBox: '{newValue}'");
            await viewModel.SavePropertyValueAsync(propertyRow, newValue);

            // After saving, update binding to sync UI
            var bindingExpression = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            bindingExpression?.UpdateTarget();
        }
        else
        {
            _logger.LogError("SavePropertyValue: не удалось получить propertyRow или viewModel", null);
        }
    }
}


