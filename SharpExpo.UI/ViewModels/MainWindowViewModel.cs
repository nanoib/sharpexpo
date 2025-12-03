using System.Collections.ObjectModel;
using System.Windows.Input;
using SharpExpo.Contracts;
using SharpExpo.Contracts.Models;
using SharpExpo.UI.Commands;
using SharpExpo.UI.Services;

namespace SharpExpo.UI.ViewModels;

/// <summary>
/// ViewModel для главного окна
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private readonly IBimFamilyDataProvider _dataProvider;
    private string _searchText = string.Empty;
    private ObservableCollection<PropertyRowViewModel> _properties = new();
    private List<PropertyRowViewModel> _allProperties = new();
    private string _currentFamilyId = string.Empty;

    public MainWindowViewModel(IBimFamilyDataProvider dataProvider, string familyId)
    {
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        _currentFamilyId = familyId ?? throw new ArgumentNullException(nameof(familyId));
        LoadCommand = new RelayCommand(async () => await LoadDataAsync());
    }

    /// <summary>
    /// Текст поиска
    /// </summary>
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
    /// Коллекция свойств для отображения
    /// </summary>
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
    /// Команда загрузки данных
    /// </summary>
    public ICommand LoadCommand { get; }

    private async Task LoadDataAsync()
    {
        try
        {
            Logger.Log($"Начало загрузки данных для семейства: {_currentFamilyId}");
            var familyData = await _dataProvider.LoadFamilyDataAsync(_currentFamilyId);
            
            if (familyData == null)
            {
                Logger.LogError($"Семейство не найдено: {_currentFamilyId}", null);
                System.Windows.MessageBox.Show(
                    $"Семейство с ID {_currentFamilyId} не найдено.",
                    "Ошибка",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (!familyData.Validate())
            {
                Logger.LogError("Данные семейства не прошли валидацию", null);
                System.Windows.MessageBox.Show(
                    "Данные семейства не прошли валидацию.",
                    "Ошибка",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            Logger.Log($"Загружено семейство: {familyData.Family.Name} (ID: {familyData.Family.Id})");
            
            var allProperties = new List<PropertyRowViewModel>();
            var orderedOptions = familyData.GetOrderedOptionsByCategory();

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

                // Добавляем заголовок категории
                var categoryHeader = new PropertyRowViewModel
                {
                    IsSectionHeader = true,
                    SectionName = categoryName,
                    IsExpanded = true,
                    ToggleExpandCommand = new RelayCommand(() => ToggleCategory(categoryName))
                };
                allProperties.Add(categoryHeader);

                Logger.Log($"Добавлена категория: {categoryName}, свойств: {optionProperties.Count}");

                // Добавляем свойства категории
                foreach (var optionProperty in optionProperties)
                {
                    var displayValue = optionProperty.GetDisplayValue();
                    allProperties.Add(new PropertyRowViewModel
                    {
                        IsSectionHeader = false,
                        PropertyName = optionProperty.PropertyName,
                        PropertyValue = displayValue,
                        Description = optionProperty.Description,
                        IsLocked = true, // Пока все свойства только для чтения
                        HasCategory = !string.IsNullOrEmpty(optionProperty.CategoryName),
                        HasDropdown = optionProperty.ValueType == OptionValueType.Enumeration
                    });
                }
            }

            // Добавляем категории, которых нет в CategoryOrder (если такие есть)
            foreach (var categoryPair in orderedOptions)
            {
                if (!familyData.Family.CategoryOrder.Contains(categoryPair.Key))
                {
                    var optionProperties = categoryPair.Value;
                    if (optionProperties.Count == 0)
                    {
                        continue;
                    }

                    // Добавляем заголовок категории
                    var categoryHeader2 = new PropertyRowViewModel
                    {
                        IsSectionHeader = true,
                        SectionName = categoryPair.Key,
                        IsExpanded = true,
                        ToggleExpandCommand = new RelayCommand(() => ToggleCategory(categoryPair.Key))
                    };
                    allProperties.Add(categoryHeader2);

                    // Добавляем свойства категории
                    foreach (var optionProperty in optionProperties)
                    {
                        var displayValue = optionProperty.GetDisplayValue();
                        allProperties.Add(new PropertyRowViewModel
                        {
                            IsSectionHeader = false,
                            PropertyName = optionProperty.PropertyName,
                            PropertyValue = displayValue,
                            Description = optionProperty.Description,
                            IsLocked = true,
                            HasCategory = !string.IsNullOrEmpty(optionProperty.CategoryName),
                            HasDropdown = optionProperty.ValueType == OptionValueType.Enumeration
                        });
                    }
                }
            }

            Logger.Log($"Всего создано строк свойств: {allProperties.Count}");
            
            _allProperties = allProperties;
            Properties = new ObservableCollection<PropertyRowViewModel>(allProperties);
            FilterProperties();
            
            Logger.Log($"Отображается строк в DataGrid: {Properties.Count}");
        }
        catch (Exception ex)
        {
            Logger.LogError("Ошибка загрузки данных", ex);
            System.Windows.MessageBox.Show(
                $"Ошибка загрузки данных: {ex.Message}\n\nДетали в логе: {Logger.LogFilePath}",
                "Ошибка",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void FilterProperties()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // Если поиск пуст, показываем все свойства с учетом развернутости категорий
            Properties.Clear();
            foreach (var prop in _allProperties)
            {
                if (prop.IsSectionHeader)
                {
                    Properties.Add(prop);
                }
                else
                {
                    // Находим родительскую категорию
                    var categoryHeader = FindCategoryHeader(prop);
                    if (categoryHeader != null && categoryHeader.IsExpanded)
                    {
                        Properties.Add(prop);
                    }
                }
            }
            return;
        }

        var searchLower = SearchText.ToLowerInvariant();
        Properties.Clear();
        
        foreach (var prop in _allProperties)
        {
            bool matches = false;
            if (prop.IsSectionHeader)
            {
                matches = prop.SectionName?.ToLowerInvariant().Contains(searchLower) ?? false;
                if (matches)
                {
                    Properties.Add(prop);
                }
            }
            else
            {
                matches = (prop.PropertyName?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                         (prop.PropertyValue?.ToLowerInvariant().Contains(searchLower) ?? false);
                
                if (matches)
                {
                    // При поиске показываем все совпадения, независимо от развернутости
                    Properties.Add(prop);
                }
            }
        }
    }

    private PropertyRowViewModel? FindCategoryHeader(PropertyRowViewModel property)
    {
        var propertyIndex = _allProperties.IndexOf(property);
        if (propertyIndex < 0) return null;
        
        // Ищем назад до заголовка категории
        for (int i = propertyIndex - 1; i >= 0; i--)
        {
            if (_allProperties[i].IsSectionHeader)
            {
                return _allProperties[i];
            }
        }
        
        return null;
    }

    private void ToggleCategory(string categoryName)
    {
        // Находим заголовок категории и переключаем его состояние
        var categoryHeader = _allProperties.FirstOrDefault(p => p.IsSectionHeader && p.SectionName == categoryName);
        if (categoryHeader != null)
        {
            categoryHeader.IsExpanded = !categoryHeader.IsExpanded;
            
            // Обновляем видимость свойств этой категории
            var categoryIndex = _allProperties.IndexOf(categoryHeader);
            var propertiesToToggle = new List<PropertyRowViewModel>();
            
            // Собираем все свойства этой категории (до следующего заголовка или конца списка)
            for (int i = categoryIndex + 1; i < _allProperties.Count; i++)
            {
                var prop = _allProperties[i];
                if (prop.IsSectionHeader)
                {
                    break; // Дошли до следующей категории
                }
                propertiesToToggle.Add(prop);
            }
            
            // Обновляем коллекцию Properties
            FilterProperties();
        }
    }
}
