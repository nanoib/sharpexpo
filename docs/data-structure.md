# Структура данных проекта SharpExpo

## Обзор

Проект SharpExpo использует иерархическую структуру данных для представления BIM-компонентов и их свойств. Данные организованы в три основных уровня:

1. **BimFamily** - BIM-семейство (компонент)
2. **FamilyOption** - опция семейства (группа свойств)
3. **OptionProperty** - свойство опции (конкретное свойство)

## Основные сущности

### BimFamily (BIM-семейство)

**Класс:** [`SharpExpo.Contracts.Models.BimFamily`](../SharpExpo.Contracts/Models/BimFamily.cs)

BimFamily представляет собой BIM-компонент со всеми его опциями и настройками отображения.

#### Свойства:

- **Id** (`string`) - Уникальный идентификатор BIM-семейства
- **Name** (`string`) - Имя BIM-семейства, отображаемое в системе
- **FamilyOptionIds** (`List<string>`) - Список идентификаторов опций семейства, входящих в это BIM-семейство
- **CategoryOrder** (`List<string>`) - Порядок категорий для отображения в панели BIM-свойств. Определяет последовательность группировки свойств в интерфейсе

#### Методы:

- **Validate()** - Проверяет валидность BIM-семейства (наличие Id, Name и хотя бы одной опции)

#### Хранение:

Каждое BIM-семейство хранится в отдельном JSON файле в директории `Families/`. Имя файла соответствует Id семейства: `{Id}.json`

**Пример файла:** [`Families/9e1f8475-2aff-4293-9999-d0596e958d85.json`](../SharpExpo.Family/Families/9e1f8475-2aff-4293-9999-d0596e958d85.json)

---

### FamilyOption (Опция семейства)

**Класс:** [`SharpExpo.Contracts.Models.FamilyOption`](../SharpExpo.Contracts/Models/FamilyOption.cs)

FamilyOption представляет собой неразделимый, но пополняемый список свойств (OptionProperty). Опции семейства могут использоваться в разных BIM-семействах, что обеспечивает переиспользование данных.

#### Свойства:

- **Id** (`string`) - Уникальный идентификатор опции семейства
- **OptionProperties** (`List<OptionProperty>`) - Список свойств опций, входящих в это семейство

#### Методы:

- **Validate()** - Проверяет валидность опции семейства:
  - Наличие Id
  - Наличие хотя бы одного OptionProperty
  - Валидность всех входящих OptionProperty

#### Хранение:

Все опции семейства хранятся в одном JSON файле: `family-options.json`

**Пример файла:** [`family-options.json`](../SharpExpo.Family/family-options.json)

---

### OptionProperty (Свойство опции)

**Класс:** [`SharpExpo.Contracts.Models.OptionProperty`](../SharpExpo.Contracts/Models/OptionProperty.cs)

OptionProperty представляет собой конкретное свойство BIM-компонента с его значением и метаданными.

#### Свойства:

- **Id** (`string`) - Уникальный идентификатор свойства опции
- **PropertyName** (`string`) - Имя свойства, отображаемое в первом столбце панели BIM-свойств
- **Description** (`string?`) - Пояснение к свойству (внутренняя характеристика, не отображается в UI)
- **ValueType** ([`OptionValueType`](#optionvaluetype-тип-значения)) - Тип свойства, определяющий допустимые значения
- **CategoryName** (`string`) - Имя категории, к которой относится это свойство. Определяет группировку свойств в панели BIM-свойств
- **StringValue** (`string?`) - Текущее значение свойства для типа String
- **DoubleValue** (`double?`) - Текущее значение свойства для типа Double
- **EnumValue** (`string?`) - Текущее значение свойства для типа Enumeration

#### Методы:

- **ValidateValue()** - Валидация значения в соответствии с типом свойства:
  - Для `String`: проверяет, что `StringValue != null`
  - Для `Double`: проверяет, что `DoubleValue.HasValue`
  - Для `Enumeration`: проверяет, что `EnumValue != null`
- **GetDisplayValue()** - Возвращает строковое представление значения для отображения в UI

---

### OptionValueType (Тип значения)

**Enum:** [`SharpExpo.Contracts.Models.OptionValueType`](../SharpExpo.Contracts/Models/OptionValueType.cs)

Определяет тип значения свойства.

#### Значения:

- **String** (`0`) - Строковое значение
- **Double** (`1`) - Число с плавающей точкой
- **Enumeration** (`2`) - Перечисление (enum)

---

### BimFamilyData (Полные данные семейства)

**Класс:** [`SharpExpo.Contracts.Models.BimFamilyData`](../SharpExpo.Contracts/Models/BimFamilyData.cs)

BimFamilyData содержит полные данные BIM-семейства для отображения в панели, включая само семейство и все связанные опции.

#### Свойства:

- **Family** ([`BimFamily`](#bimfamily-bim-семейство)) - BIM-семейство
- **FamilyOptions** (`Dictionary<string, FamilyOption>`) - Словарь опций семейства по их Id

#### Методы:

- **GetOrderedOptionsByCategory()** - Получает все OptionProperty, отсортированные по порядку категорий:
  - Собирает все OptionProperty из всех FamilyOption
  - Группирует их по CategoryName
  - Сортирует согласно CategoryOrder из Family
  - Возвращает Dictionary, где ключ - имя категории, значение - список OptionProperty
- **Validate()** - Валидация данных семейства:
  - Проверяет валидность Family
  - Проверяет, что все FamilyOptionIds имеют соответствующие FamilyOption
  - Проверяет валидность всех FamilyOption

---

## DTO классы для JSON

Для десериализации JSON файлов используются специальные DTO классы:

### BimFamilyDto

**Класс:** [`SharpExpo.Contracts.DTOs.BimFamilyDto`](../SharpExpo.Contracts/DTOs/BimFamilyDto.cs)

DTO для десериализации JSON файла BIM-семейства. Структура полностью соответствует JSON формату.

### FamilyOptionsCollectionDto

**Класс:** [`SharpExpo.Contracts.DTOs.FamilyOptionsCollectionDto`](../SharpExpo.Contracts/DTOs/FamilyOptionDto.cs)

DTO для десериализации JSON файла со всеми опциями семейства. Содержит список `FamilyOptionItemDto`.

### FamilyOptionItemDto

**Класс:** [`SharpExpo.Contracts.DTOs.FamilyOptionItemDto`](../SharpExpo.Contracts/DTOs/FamilyOptionDto.cs)

DTO для одной опции семейства из JSON.

### OptionPropertyDto

**Класс:** [`SharpExpo.Contracts.DTOs.OptionPropertyDto`](../SharpExpo.Contracts/DTOs/FamilyOptionDto.cs)

DTO для свойства опции из JSON. Содержит поле `Value` типа `object?`, которое затем преобразуется в соответствующий тип в зависимости от `ValueType`.

---

## Загрузка и преобразование данных

### JsonBimFamilyDataProvider

**Класс:** [`SharpExpo.Family.JsonBimFamilyDataProvider`](../SharpExpo.Family/JsonBimFamilyDataProvider.cs)

**Интерфейс:** [`SharpExpo.Contracts.IBimFamilyDataProvider`](../SharpExpo.Contracts/IBimFamilyDataProvider.cs)

Провайдер данных, отвечающий за загрузку и преобразование данных из JSON файлов.

#### Основные методы:

- **LoadFamilyDataAsync(string familyId)** - Загружает данные конкретного BIM-семейства:
  1. Загружает кэш опций семейства (если еще не загружен)
  2. Читает JSON файл семейства из директории `Families/{familyId}.json`
  3. Десериализует в `BimFamilyDto`
  4. Преобразует в `BimFamily`
  5. Загружает соответствующие `FamilyOption` из кэша
  6. Возвращает `BimFamilyData`

- **LoadAllFamiliesAsync()** - Загружает список всех доступных BIM-семейств

#### Преобразование данных:

- **ConvertToFamilyOption(FamilyOptionItemDto)** - Преобразует DTO в `FamilyOption`
- **ConvertToOptionProperty(OptionPropertyDto)** - Преобразует DTO в `OptionProperty`:
  - Парсит `ValueType` из строки
  - Устанавливает значение в зависимости от типа:
    - Для `String`: `null` преобразуется в пустую строку
    - Для `Double`: парсит строку в число
    - Для `Enumeration`: `null` преобразуется в пустую строку
- **ParseValueType(string?)** - Парсит строковое представление типа в `OptionValueType`

#### Кэширование:

Опции семейства загружаются один раз при первом обращении и кэшируются в `_cachedFamilyOptions` для повышения производительности.

---

## Отображение в UI

### MainWindowViewModel

**Класс:** [`SharpExpo.UI.ViewModels.MainWindowViewModel`](../SharpExpo.UI/ViewModels/MainWindowViewModel.cs)

ViewModel для главного окна, отвечающая за загрузку и отображение данных.

#### Процесс загрузки данных:

1. Вызывается `LoadDataAsync()` при загрузке окна
2. Загружается `BimFamilyData` через `IBimFamilyDataProvider.LoadFamilyDataAsync()`
3. Выполняется валидация данных
4. Вызывается `GetOrderedOptionsByCategory()` для получения отсортированных данных
5. Создаются `PropertyRowViewModel` для каждой категории и свойства:
   - Заголовки категорий (IsSectionHeader = true)
   - Свойства категорий (IsSectionHeader = false)
6. Данные отображаются в DataGrid в порядке, определенном `CategoryOrder`

#### Фильтрация:

Метод `FilterProperties()` позволяет фильтровать свойства по тексту поиска, проверяя совпадения в названиях категорий, именах свойств и их значениях.

---

## Порядок категорий

Порядок отображения свойств в панели BIM-свойств определяется полем `CategoryOrder` в `BimFamily`. 

**Важно:**
- Категории отображаются в том порядке, в котором они указаны в `CategoryOrder`
- Если в данных есть категории, которых нет в `CategoryOrder`, они добавляются в конец списка
- Категории группируют свойства по их `CategoryName`

---

## Валидация данных

Валидация выполняется на нескольких уровнях:

1. **BimFamily.Validate()** - Проверяет наличие обязательных полей
2. **FamilyOption.Validate()** - Проверяет наличие Id и валидность всех OptionProperty
3. **OptionProperty.ValidateValue()** - Проверяет соответствие значения типу свойства
4. **BimFamilyData.Validate()** - Комплексная проверка всех данных семейства

Если валидация не проходит, приложение показывает сообщение об ошибке и не отображает данные.

---

## Пример структуры данных

### JSON файл семейства (Families/{Id}.json):

```json
{
  "Id": "9e1f8475-2aff-4293-9999-d0596e958d85",
  "Name": "1",
  "FamilyOptionIds": [
    "option-basic-info",
    "option-categorization",
    "option-coordinates"
  ],
  "CategoryOrder": [
    "Наименование",
    "Категоризация",
    "Система координат модели"
  ]
}
```

### JSON файл опций (family-options.json):

```json
{
  "FamilyOptions": [
    {
      "Id": "option-basic-info",
      "OptionProperties": [
        {
          "Id": "opt-val-name",
          "PropertyName": "Наименование",
          "Description": "Основное наименование компонента",
          "ValueType": "String",
          "CategoryName": "Наименование",
          "Value": "1"
        }
      ]
    }
  ]
}
```

---

## Связи между сущностями

```
BimFamily
  ├── FamilyOptionIds (список ID)
  │
  └── FamilyOptions (словарь по ID)
      └── FamilyOption
          └── OptionProperties (список)
              └── OptionProperty
                  ├── ValueType (String/Double/Enumeration)
                  ├── CategoryName (группировка)
                  └── Value (в зависимости от типа)
```

---

## Принципы проектирования

1. **Разделение данных и представления** - DTO классы отделены от моделей
2. **Переиспользование** - FamilyOption могут использоваться в разных BimFamily
3. **Валидация на всех уровнях** - Каждая сущность имеет метод валидации
4. **Типобезопасность** - Использование enum для типов значений
5. **Гибкость** - Порядок категорий настраивается для каждого семейства
6. **Производительность** - Кэширование опций семейства

