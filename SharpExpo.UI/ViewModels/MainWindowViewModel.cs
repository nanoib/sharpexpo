using System.Collections.ObjectModel;
using System.Windows.Input;
using SharpExpo.Contracts;
using SharpExpo.Contracts.Models;
using SharpExpo.UI.Commands;

namespace SharpExpo.UI.ViewModels;

/// <summary>
/// ViewModel для главного окна
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private readonly IBimDataProvider _dataProvider;
    private string _searchText = string.Empty;
    private ObservableCollection<PropertyRowViewModel> _properties = new();
    private List<PropertyRowViewModel> _allProperties = new();

    public MainWindowViewModel(IBimDataProvider dataProvider)
    {
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
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
            var components = await _dataProvider.LoadComponentsAsync();
            var allProperties = new List<PropertyRowViewModel>();

            foreach (var component in components)
            {
                foreach (var section in component.Sections)
                {
                    // Добавляем заголовок секции
                    allProperties.Add(new PropertyRowViewModel
                    {
                        IsSectionHeader = true,
                        SectionName = section.Name,
                        IsExpanded = section.IsExpanded
                    });

                    // Добавляем свойства секции
                    foreach (var property in section.Properties)
                    {
                        allProperties.Add(new PropertyRowViewModel
                        {
                            IsSectionHeader = false,
                            PropertyName = property.Name,
                            PropertyValue = property.Value ?? string.Empty,
                            IsLocked = property.IsLocked,
                            HasCategory = !string.IsNullOrEmpty(property.Category),
                            HasDropdown = property.HasDropdown
                        });
                    }
                }
            }

            _allProperties = allProperties;
            Properties = new ObservableCollection<PropertyRowViewModel>(allProperties);
            FilterProperties();
        }
        catch (Exception ex)
        {
            // В реальном приложении здесь должна быть обработка ошибок
            System.Windows.MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void FilterProperties()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // Если поиск пуст, показываем все свойства
            Properties.Clear();
            foreach (var prop in _allProperties)
            {
                Properties.Add(prop);
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
            }
            else
            {
                matches = (prop.PropertyName?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                         (prop.PropertyValue?.ToLowerInvariant().Contains(searchLower) ?? false);
            }
            
            if (matches)
            {
                Properties.Add(prop);
            }
        }
    }
}

