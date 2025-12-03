using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using SharpExpo.Contracts;
using SharpExpo.Contracts.Models;
using SharpExpo.UI.Commands;
using SharpExpo.UI.Services;
using SharpExpo.UI.ViewModels;

namespace SharpExpo.UI.Windows.BimPropertiesWindow.ViewModels;

/// <summary>
/// View model for the BIM properties window.
/// Manages the display and interaction with BIM family properties.
/// </summary>
/// <remarks>
/// WHY: This class serves as the view model for the BIM properties window, coordinating between the data provider, services, and the UI.
/// It follows the MVVM pattern and delegates specific responsibilities to specialized services to maintain single responsibility.
/// </remarks>
public class BimPropertiesWindowViewModel : ViewModelBase
{
    private readonly IBimFamilyDataProvider _dataProvider;
    private readonly IPropertyFilterService _filterService;
    private readonly IPropertySaveService _saveService;
    private readonly IPropertyViewModelFactory _viewModelFactory;
    private readonly ILogger _logger;
    private readonly IMessageService _messageService;
    private readonly string _familyOptionsFilePath;
    private readonly string _familiesDirectory;

    private string _searchText = string.Empty;
    private ObservableCollection<PropertyRowViewModel> _properties = new();
    private List<PropertyRowViewModel> _allProperties = new();
    private string _currentFamilyId = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="BimPropertiesWindowViewModel"/> class.
    /// </summary>
    /// <param name="dataProvider">The data provider for loading family data.</param>
    /// <param name="filterService">The service for filtering properties.</param>
    /// <param name="saveService">The service for saving property values.</param>
    /// <param name="viewModelFactory">The factory for creating property view models.</param>
    /// <param name="logger">The logger for logging operations.</param>
    /// <param name="messageService">The service for displaying messages to the user.</param>
    /// <param name="familyId">The ID of the family to load.</param>
    /// <param name="familyOptionsFilePath">The path to the family-options.json file.</param>
    /// <param name="familiesDirectory">The directory containing family JSON files. If empty, uses default location.</param>
    public BimPropertiesWindowViewModel(
        IBimFamilyDataProvider dataProvider,
        IPropertyFilterService filterService,
        IPropertySaveService saveService,
        IPropertyViewModelFactory viewModelFactory,
        ILogger logger,
        IMessageService messageService,
        string familyId,
        string familyOptionsFilePath,
        string familiesDirectory = "")
    {
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        _filterService = filterService ?? throw new ArgumentNullException(nameof(filterService));
        _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        _viewModelFactory = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _currentFamilyId = familyId ?? throw new ArgumentNullException(nameof(familyId));
        _familyOptionsFilePath = Path.GetFullPath(familyOptionsFilePath ?? throw new ArgumentNullException(nameof(familyOptionsFilePath)));
        _familiesDirectory = string.IsNullOrEmpty(familiesDirectory) ? string.Empty : Path.GetFullPath(familiesDirectory);

        _logger.Log($"BimPropertiesWindowViewModel initialized:");
        _logger.Log($"  FamilyId: {_currentFamilyId}");
        _logger.Log($"  FamilyOptionsFilePath (absolute): {_familyOptionsFilePath}");
        _logger.Log($"  FamiliesDirectory (absolute): {_familiesDirectory}");

        LoadCommand = new RelayCommand(async () => await LoadDataAsync());
    }

    /// <summary>
    /// Gets or sets the search text for filtering properties.
    /// </summary>
    /// <value>The search text used to filter displayed properties.</value>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                FilterProperties();
            }
        }
    }

    /// <summary>
    /// Gets the collection of properties displayed in the UI.
    /// </summary>
    /// <value>An observable collection of property row view models.</value>
    public ObservableCollection<PropertyRowViewModel> Properties
    {
        get => _properties;
        set
        {
            _properties = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the command to load family data.
    /// </summary>
    /// <value>The command that triggers loading of family data.</value>
    public ICommand LoadCommand { get; }

    /// <summary>
    /// Loads family data asynchronously and populates the properties collection.
    /// </summary>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    /// WHY: This method orchestrates the data loading process, delegating specific tasks to services.
    /// It handles validation, error reporting, and UI updates in a centralized location.
    /// </remarks>
    private async Task LoadDataAsync()
    {
        try
        {
            _logger.Log($"Начало загрузки данных для семейства: {_currentFamilyId}");
            _logger.Log($"Путь к файлу family-options.json при загрузке: {_familyOptionsFilePath}");

            var familyData = await _dataProvider.LoadFamilyDataAsync(_currentFamilyId);

            if (familyData == null)
            {
                _logger.LogError($"Семейство не найдено: {_currentFamilyId}", null);
                _messageService.ShowWarning(
                    $"Семейство с ID {_currentFamilyId} не найдено.",
                    "Ошибка");
                return;
            }

            if (!familyData.Validate())
            {
                _logger.LogError("Данные семейства не прошли валидацию", null);
                _messageService.ShowError(
                    "Данные семейства не прошли валидацию.",
                    "Ошибка");
                return;
            }

            _logger.Log($"Загружено семейство: {familyData.Family.Name} (ID: {familyData.Family.Id})");

            var allProperties = CreatePropertyViewModels(familyData);
            _logger.Log($"Всего создано строк свойств: {allProperties.Count}");

            _allProperties = allProperties;
            Properties = new ObservableCollection<PropertyRowViewModel>(allProperties);
            FilterProperties();

            _logger.Log($"Отображается строк в DataGrid: {Properties.Count}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Ошибка загрузки данных", ex);
            _messageService.ShowError(
                $"Ошибка загрузки данных: {ex.Message}\n\nДетали в логе: {_logger.LogFilePath}",
                "Ошибка");
        }
    }

    /// <summary>
    /// Creates property view models from family data.
    /// </summary>
    /// <param name="familyData">The family data to create view models from.</param>
    /// <returns>A list of property row view models organized by category.</returns>
    /// <remarks>
    /// WHY: This method handles the creation of view models from domain models, organizing them by category
    /// according to the family's category order. This separation keeps the ViewModel focused on coordination.
    /// </remarks>
    private List<PropertyRowViewModel> CreatePropertyViewModels(BimFamilyData familyData)
    {
        var allProperties = new List<PropertyRowViewModel>();
        var orderedOptions = familyData.GetOrderedOptionsByCategory();

        // Add categories in the specified order
        foreach (var categoryName in familyData.Family.CategoryOrder)
        {
            if (!orderedOptions.ContainsKey(categoryName))
            {
                continue;
            }

            var optionProperties = orderedOptions[categoryName];
            if (optionProperties.Count == 0)
            {
                continue;
            }

            AddCategoryWithProperties(allProperties, categoryName, optionProperties, familyData);
        }

        // Add categories not in CategoryOrder (if any)
        foreach (var categoryPair in orderedOptions)
        {
            if (!familyData.Family.CategoryOrder.Contains(categoryPair.Key))
            {
                var optionProperties = categoryPair.Value;
                if (optionProperties.Count == 0)
                {
                    continue;
                }

                AddCategoryWithProperties(allProperties, categoryPair.Key, optionProperties, familyData);
            }
        }

        return allProperties;
    }

    /// <summary>
    /// Adds a category header and its properties to the properties list.
    /// </summary>
    /// <param name="allProperties">The list to add to.</param>
    /// <param name="categoryName">The name of the category.</param>
    /// <param name="optionProperties">The properties in the category.</param>
    /// <param name="familyData">The family data to find family option IDs from.</param>
    private void AddCategoryWithProperties(
        List<PropertyRowViewModel> allProperties,
        string categoryName,
        List<OptionProperty> optionProperties,
        BimFamilyData familyData)
    {
        var toggleCommand = new RelayCommand(() => ToggleCategory(categoryName));
        var categoryHeader = _viewModelFactory.CreateCategoryHeader(categoryName, toggleCommand);
        allProperties.Add(categoryHeader);

        _logger.Log($"Добавлена категория: {categoryName}, свойств: {optionProperties.Count}");

        foreach (var optionProperty in optionProperties)
        {
            var familyOptionId = FindFamilyOptionId(optionProperty, familyData);
            var propertyRow = _viewModelFactory.CreatePropertyRow(optionProperty, familyOptionId ?? string.Empty);
            allProperties.Add(propertyRow);
        }
    }

    /// <summary>
    /// Filters properties based on the current search text.
    /// </summary>
    /// <remarks>
    /// WHY: This method delegates filtering to the PropertyFilterService to maintain separation of concerns.
    /// </remarks>
    private void FilterProperties()
    {
        var filtered = _filterService.FilterProperties(_allProperties, _searchText);
        Properties.Clear();
        foreach (var prop in filtered)
        {
            Properties.Add(prop);
        }
    }

    /// <summary>
    /// Toggles the expansion state of a category.
    /// </summary>
    /// <param name="categoryName">The name of the category to toggle.</param>
    /// <remarks>
    /// WHY: This method handles category expansion/collapse, updating the UI accordingly.
    /// </remarks>
    private void ToggleCategory(string categoryName)
    {
        var categoryHeader = _allProperties.FirstOrDefault(p => p.IsSectionHeader && p.SectionName == categoryName);
        if (categoryHeader != null)
        {
            categoryHeader.IsExpanded = !categoryHeader.IsExpanded;
            OnPropertyChanged(nameof(Properties)); // Notify for triangle update
            FilterProperties();
        }
    }

    /// <summary>
    /// Finds the family option ID for a given option property.
    /// </summary>
    /// <param name="optionProperty">The option property to find the family option ID for.</param>
    /// <param name="familyData">The family data to search in.</param>
    /// <returns>The family option ID, or <see langword="null"/> if not found.</returns>
    /// <remarks>
    /// WHY: This method searches through family options to find which family option contains the given property.
    /// This is needed to properly save property values back to the correct family option.
    /// </remarks>
    private static string? FindFamilyOptionId(OptionProperty optionProperty, BimFamilyData familyData)
    {
        foreach (var familyOptionPair in familyData.FamilyOptions)
        {
            if (familyOptionPair.Value.OptionProperties.Any(op => op.Id == optionProperty.Id))
            {
                return familyOptionPair.Key;
            }
        }
        return null;
    }

    /// <summary>
    /// Saves a property value asynchronously.
    /// </summary>
    /// <param name="propertyRow">The property row view model containing the property to save.</param>
    /// <param name="newValue">The new value to save.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <remarks>
    /// WHY: This method delegates saving to the PropertySaveService, maintaining separation of concerns.
    /// </remarks>
    public async Task SavePropertyValueAsync(PropertyRowViewModel propertyRow, string newValue)
    {
        await _saveService.SavePropertyValueAsync(propertyRow, newValue, _familyOptionsFilePath);
    }
}

