using System.Collections.ObjectModel;
using System.IO;
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
    private string _familyOptionsFilePath = string.Empty;
    private string _familiesDirectory = string.Empty;

    public MainWindowViewModel(IBimFamilyDataProvider dataProvider, string familyId, string familyOptionsFilePath, string familiesDirectory = "")
    {
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        _currentFamilyId = familyId ?? throw new ArgumentNullException(nameof(familyId));
        _familyOptionsFilePath = Path.GetFullPath(familyOptionsFilePath ?? throw new ArgumentNullException(nameof(familyOptionsFilePath)));
        _familiesDirectory = string.IsNullOrEmpty(familiesDirectory) ? string.Empty : Path.GetFullPath(familiesDirectory);
        
        Logger.Log($"MainWindowViewModel инициализирован:");
        Logger.Log($"  FamilyId: {_currentFamilyId}");
        Logger.Log($"  FamilyOptionsFilePath (абсолютный): {_familyOptionsFilePath}");
        Logger.Log($"  FamiliesDirectory (абсолютный): {_familiesDirectory}");
        
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
            Logger.Log($"Путь к файлу family-options.json при загрузке: {_familyOptionsFilePath}");
            Logger.Log($"Файл существует: {File.Exists(_familyOptionsFilePath)}");
            
            if (File.Exists(_familyOptionsFilePath))
            {
                var fileInfo = new FileInfo(_familyOptionsFilePath);
                fileInfo.Refresh();
                Logger.Log($"Информация о файле: размер={fileInfo.Length} байт, последнее изменение={fileInfo.LastWriteTime}");
            }
            
            var familyData = await _dataProvider.LoadFamilyDataAsync(_currentFamilyId);
            
            if (familyData == null)
            {
                Logger.LogError($"Семейство не найдено: {_currentFamilyId}", null);
                // Показываем MessageBox только если есть UI контекст (не в тестах)
                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show(
                            $"Семейство с ID {_currentFamilyId} не найдено.",
                            "Ошибка",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Warning);
                    });
                }
                return;
            }

            if (!familyData.Validate())
            {
                Logger.LogError("Данные семейства не прошли валидацию", null);
                // Показываем MessageBox только если есть UI контекст (не в тестах)
                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show(
                            "Данные семейства не прошли валидацию.",
                            "Ошибка",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);
                    });
                }
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
                    // Находим FamilyOptionId для этого свойства
                    var familyOptionId = FindFamilyOptionId(optionProperty, familyData);
                    allProperties.Add(new PropertyRowViewModel
                    {
                        IsSectionHeader = false,
                        PropertyName = optionProperty.PropertyName,
                        PropertyValue = displayValue,
                        Description = optionProperty.Description,
                        IsLocked = optionProperty.ValueType == OptionValueType.Enumeration, // Enumeration нельзя редактировать
                        HasCategory = !string.IsNullOrEmpty(optionProperty.CategoryName),
                        HasDropdown = optionProperty.ValueType == OptionValueType.Enumeration,
                        OriginalProperty = optionProperty,
                        FamilyOptionId = familyOptionId,
                        ValueType = optionProperty.ValueType
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
                        // Находим FamilyOptionId для этого свойства
                        var familyOptionId = FindFamilyOptionId(optionProperty, familyData);
                        allProperties.Add(new PropertyRowViewModel
                        {
                            IsSectionHeader = false,
                            PropertyName = optionProperty.PropertyName,
                            PropertyValue = displayValue,
                            Description = optionProperty.Description,
                            IsLocked = optionProperty.ValueType == OptionValueType.Enumeration, // Enumeration нельзя редактировать
                            HasCategory = !string.IsNullOrEmpty(optionProperty.CategoryName),
                            HasDropdown = optionProperty.ValueType == OptionValueType.Enumeration,
                            OriginalProperty = optionProperty,
                            FamilyOptionId = familyOptionId,
                            ValueType = optionProperty.ValueType
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
            OnPropertyChanged(nameof(Properties)); // Уведомляем об изменении для обновления треугольника
            
            // Обновляем коллекцию Properties
            FilterProperties();
        }
    }

    private string? FindFamilyOptionId(OptionProperty optionProperty, BimFamilyData familyData)
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
    /// Сохраняет изменение значения свойства в JSON файл
    /// </summary>
    public async Task SavePropertyValueAsync(PropertyRowViewModel propertyRow, string newValue)
    {
        Logger.Log($"=== SavePropertyValueAsync ВЫЗВАН ===");
        Logger.Log($"PropertyName: {propertyRow.PropertyName}");
        Logger.Log($"NewValue: '{newValue}'");
        Logger.Log($"OriginalProperty: {(propertyRow.OriginalProperty != null ? "не null" : "null")}");
        Logger.Log($"FamilyOptionId: '{propertyRow.FamilyOptionId}'");
        
        if (propertyRow.OriginalProperty == null || string.IsNullOrEmpty(propertyRow.FamilyOptionId))
        {
            Logger.LogError("Не удалось сохранить: отсутствует OriginalProperty или FamilyOptionId", null);
            return;
        }

        try
        {
            Logger.Log($"Начало сохранения: PropertyId={propertyRow.OriginalProperty.Id}, FamilyOptionId={propertyRow.FamilyOptionId}, NewValue={newValue}");
            Logger.Log($"Путь к файлу: {_familyOptionsFilePath}");
            Logger.Log($"Файл существует: {File.Exists(_familyOptionsFilePath)}");
            
            // Загружаем текущий JSON
            // Убеждаемся, что используем абсолютный путь
            var loadPath = Path.GetFullPath(_familyOptionsFilePath);
            Logger.Log($"Загрузка JSON из: {loadPath}");
            Logger.Log($"Файл существует перед загрузкой: {File.Exists(loadPath)}");
            
            if (!File.Exists(loadPath))
            {
                Logger.LogError($"Файл не найден для загрузки: {loadPath}", null);
                return;
            }
            
            var fileInfoBefore = new FileInfo(loadPath);
            fileInfoBefore.Refresh();
            Logger.Log($"Информация о файле перед загрузкой: размер={fileInfoBefore.Length} байт, последнее изменение={fileInfoBefore.LastWriteTime}");
            
            var jsonContent = await File.ReadAllTextAsync(loadPath);
            Logger.Log($"Размер загруженного JSON: {jsonContent.Length} байт");
            var optionsDto = System.Text.Json.JsonSerializer.Deserialize<SharpExpo.Contracts.DTOs.FamilyOptionsCollectionDto>(
                jsonContent,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (optionsDto == null)
            {
                Logger.LogError("Не удалось десериализовать family-options.json", null);
                return;
            }

            // Находим нужный FamilyOption и OptionProperty
            var familyOption = optionsDto.FamilyOptions.FirstOrDefault(fo => fo.Id == propertyRow.FamilyOptionId);
            if (familyOption == null)
            {
                Logger.LogError($"FamilyOption с ID {propertyRow.FamilyOptionId} не найден", null);
                return;
            }

            var optionPropertyDto = familyOption.OptionProperties.FirstOrDefault(op => op.Id == propertyRow.OriginalProperty.Id);
            if (optionPropertyDto == null)
            {
                Logger.LogError($"OptionProperty с ID {propertyRow.OriginalProperty.Id} не найден", null);
                return;
            }

            // Обновляем значение в зависимости от типа
            if (propertyRow.ValueType == OptionValueType.String)
            {
                var oldValue = optionPropertyDto.Value?.ToString() ?? "null";
                optionPropertyDto.Value = newValue;
                propertyRow.OriginalProperty.StringValue = newValue;
                // СРАЗУ обновляем отображаемое значение
                propertyRow.PropertyValue = newValue;
                Logger.Log($"Значение обновлено в DTO и ViewModel: '{oldValue}' -> '{newValue}'");
                
                // Проверяем, что значение действительно обновилось
                if (optionPropertyDto.Value?.ToString() != newValue)
                {
                    Logger.LogError($"ОШИБКА: Значение в DTO не обновилось! Ожидалось: '{newValue}', Фактически: '{optionPropertyDto.Value}'", null);
                }
            }
            else if (propertyRow.ValueType == OptionValueType.Double)
            {
                if (double.TryParse(newValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var doubleValue))
                {
                    optionPropertyDto.Value = doubleValue;
                    propertyRow.OriginalProperty.DoubleValue = doubleValue;
                }
                else
                {
                    Logger.LogError($"Не удалось преобразовать '{newValue}' в число", null);
                    // Показываем MessageBox только если есть UI контекст (не в тестах)
                    if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            System.Windows.MessageBox.Show(
                                $"Неверный формат числа: {newValue}",
                                "Ошибка",
                                System.Windows.MessageBoxButton.OK,
                                System.Windows.MessageBoxImage.Error);
                        });
                    }
                    return;
                }
            }

            // Сохраняем обратно в JSON (используем PascalCase, как в исходном файле)
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null, // Используем исходные имена свойств (PascalCase)
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Для сохранения кириллицы
            };
            // Проверяем, что значение действительно в DTO перед сериализацией
            var checkProperty = optionsDto.FamilyOptions
                .FirstOrDefault(fo => fo.Id == propertyRow.FamilyOptionId)?
                .OptionProperties.FirstOrDefault(op => op.Id == propertyRow.OriginalProperty.Id);
            
            if (checkProperty != null)
            {
                Logger.Log($"Перед сериализацией: значение в DTO = '{checkProperty.Value}' (ожидалось: '{newValue}')");
            }
            
            var updatedJson = System.Text.Json.JsonSerializer.Serialize(optionsDto, jsonOptions);
            
            // Проверяем, что новое значение есть в сериализованном JSON
            if (!updatedJson.Contains(newValue))
            {
                Logger.LogError($"ОШИБКА: Новое значение '{newValue}' не найдено в сериализованном JSON!", null);
            }
            else
            {
                Logger.Log($"✓ Новое значение '{newValue}' найдено в сериализованном JSON");
            }

            // Используем уже нормализованный путь (он был нормализован в конструкторе)
            // Убеждаемся, что путь абсолютный и совпадает с путем загрузки
            var absolutePath = Path.GetFullPath(_familyOptionsFilePath);
            
            // Проверяем, что путь сохранения совпадает с путем загрузки
            if (absolutePath != loadPath)
            {
                Logger.LogError($"ОШИБКА: Путь сохранения не совпадает с путем загрузки! Сохранение: {absolutePath}, Загрузка: {loadPath}", null);
                // Используем путь загрузки для сохранения
                absolutePath = loadPath;
            }
            else
            {
                Logger.Log($"✓ Путь сохранения совпадает с путем загрузки: {absolutePath}");
            }
            
            Logger.Log($"=== СОХРАНЕНИЕ ЗНАЧЕНИЯ ===");
            Logger.Log($"Свойство: {propertyRow.PropertyName} (Id: {propertyRow.OriginalProperty.Id})");
            Logger.Log($"FamilyOptionId: {propertyRow.FamilyOptionId}");
            Logger.Log($"Путь к файлу (абсолютный): {absolutePath}");
            Logger.Log($"Файл существует перед сохранением: {File.Exists(absolutePath)}");
            Logger.Log($"Новое значение: '{newValue}'");
            Logger.Log($"Старое значение: '{optionPropertyDto.Value}'");
            
            // Сохраняем файл с принудительной записью на диск
            // Используем FileStream для гарантированной записи
            Logger.Log($"Начинаем запись файла: {absolutePath}");
            Logger.Log($"Размер JSON для записи: {updatedJson.Length} байт");
            
            // Записываем файл с принудительной записью на диск
            // Используем File.WriteAllTextAsync для простоты и надежности
            await File.WriteAllTextAsync(absolutePath, updatedJson, System.Text.Encoding.UTF8);
            
            // Принудительно сбрасываем буферы файловой системы через FileStream
            try
            {
                using (var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                {
                    fileStream.Flush(true); // Принудительная запись на диск
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Не удалось выполнить Flush файла (не критично): {ex.Message}", ex);
            }
            
            Logger.Log($"Файл записан через FileStream");
            
            // Обновляем информацию о файле
            var fileInfo = new FileInfo(absolutePath);
            fileInfo.Refresh();
            
            Logger.Log($"Файл записан, размер на диске: {fileInfo.Length} байт, последнее изменение: {fileInfo.LastWriteTime}");
            
            // Дополнительная проверка: читаем файл сразу после записи
            await Task.Delay(100); // Небольшая задержка для гарантии записи на диск
            var verificationContent = await File.ReadAllTextAsync(absolutePath);
            if (verificationContent.Contains(newValue))
            {
                Logger.Log($"✓ Подтверждение: новое значение '{newValue}' найдено в файле после записи");
            }
            else
            {
                Logger.LogError($"✗ ОШИБКА: новое значение '{newValue}' НЕ найдено в файле после записи!", null);
                var previewLength = Math.Min(500, verificationContent.Length);
                if (previewLength > 0)
                {
                    Logger.LogError($"Содержимое файла (первые {previewLength} символов): {verificationContent.Substring(0, previewLength)}", null);
                }
            }
            
            // Очищаем кэш провайдера данных, чтобы при следующей загрузке использовались обновленные данные
            _dataProvider.ClearCache();
            
            // Проверяем, что файл действительно сохранен и содержит правильное значение
            if (File.Exists(absolutePath))
            {
                // Небольшая задержка для гарантии записи на диск
                await Task.Delay(100);
                
                var savedContent = await File.ReadAllTextAsync(absolutePath);
                Logger.Log($"Файл сохранен, размер при чтении: {savedContent.Length} байт");
                
                // Проверяем, что значение действительно сохранено
                var verificationDto = System.Text.Json.JsonSerializer.Deserialize<SharpExpo.Contracts.DTOs.FamilyOptionsCollectionDto>(
                    savedContent,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (verificationDto != null)
                {
                    var savedProp = verificationDto.FamilyOptions
                        .SelectMany(fo => fo.OptionProperties)
                        .FirstOrDefault(op => op.Id == propertyRow.OriginalProperty.Id);
                    
                    if (savedProp != null)
                    {
                        Logger.Log($"✓ Проверка сохранения: значение в файле = '{savedProp.Value}' (ожидалось: '{newValue}')");
                        
                        if (savedProp.Value?.ToString() != newValue)
                        {
                            Logger.LogError($"⚠ ВНИМАНИЕ: Сохраненное значение не совпадает с ожидаемым!", null);
                            // Показываем MessageBox только если есть UI контекст (не в тестах)
                            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    System.Windows.MessageBox.Show(
                                        $"ВНИМАНИЕ: Сохраненное значение не совпадает с ожидаемым!\n\n" +
                                        $"Ожидалось: {newValue}\n" +
                                        $"Сохранено: {savedProp.Value}\n\n" +
                                        $"Проверьте лог-файл: {Logger.LogFilePath}",
                                        "Предупреждение",
                                        System.Windows.MessageBoxButton.OK,
                                        System.Windows.MessageBoxImage.Warning);
                                });
                            }
                        }
                        else
                        {
                            Logger.Log($"✓ Значение успешно сохранено и проверено!");
                        }
                    }
                    else
                    {
                        Logger.LogError($"⚠ Свойство {propertyRow.OriginalProperty.Id} не найдено в сохраненном файле!", null);
                    }
                }
            }
            else
            {
                Logger.LogError($"✗ Файл не найден после сохранения: {absolutePath}", null);
                // Показываем MessageBox только если есть UI контекст (не в тестах)
                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show(
                            $"ОШИБКА: Файл не найден после сохранения!\n\n" +
                            $"Путь: {absolutePath}\n\n" +
                            $"Проверьте лог-файл: {Logger.LogFilePath}",
                            "Ошибка сохранения",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);
                    });
                }
            }
            
            // Обновляем отображаемое значение (если еще не обновлено выше)
            if (propertyRow.PropertyValue != newValue && propertyRow.ValueType == OptionValueType.String)
            {
                propertyRow.PropertyValue = newValue;
            }
            else if (propertyRow.ValueType != OptionValueType.String)
            {
                propertyRow.PropertyValue = propertyRow.OriginalProperty.GetDisplayValue();
            }
            
            Logger.Log($"=== СОХРАНЕНИЕ ЗАВЕРШЕНО ===");
            Logger.Log($"Значение свойства '{propertyRow.PropertyName}' успешно обновлено на '{newValue}'");
            Logger.Log($"Отображаемое значение в ViewModel: '{propertyRow.PropertyValue}'");
            Logger.Log($"Значение в OriginalProperty: '{propertyRow.OriginalProperty.GetDisplayValue()}'");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка при сохранении значения свойства", ex);
            // Показываем MessageBox только если есть UI контекст (не в тестах)
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show(
                        $"Ошибка при сохранении: {ex.Message}",
                        "Ошибка",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                });
            }
        }
    }
}
