# Документация проекта SharpExpo

## Структура документации

- [**Структура данных**](data-structure.md) - Подробное описание структуры данных проекта, включая все модели, DTO классы, процесс загрузки и отображения данных

## Быстрый старт

### Основные концепции

Проект SharpExpo использует иерархическую структуру данных для представления BIM-компонентов:

1. **BimFamily** - BIM-семейство (компонент)
2. **FamilyOption** - опция семейства (группа свойств)
3. **OptionProperty** - свойство опции (конкретное свойство)

### Расположение файлов

- **Модели данных:** `SharpExpo.Contracts/Models/`
- **DTO классы:** `SharpExpo.Contracts/DTOs/`
- **Интерфейсы:** `SharpExpo.Contracts/`
- **Реализация:** `SharpExpo.Family/`
- **UI:** `SharpExpo.UI/`

### JSON файлы

- **Семейства:** `SharpExpo.Family/Families/{Id}.json`
- **Опции:** `SharpExpo.Family/family-options.json`

### Запуск UI

1. Установите .NET SDK 8.0 и убедитесь, что `dotnet` доступен в PATH.
2. В корне репозитория выполните восстановление зависимостей:  
   `dotnet restore SharpExpo.sln`
3. Запустите клиент:  
   `dotnet run --project SharpExpo.UI/SharpExpo.UI.csproj`
4. При необходимости передайте путь к конкретному `family-options.json`:  
   `dotnet run --project SharpExpo.UI/SharpExpo.UI.csproj -- --family-path "C:\path\to\family-options.json"`

Без параметра `--family-path` приложение автоматически ищет первую по алфавиту директорию в `C:\repos\sharpexpo\families` и использует файл `family-options.json` внутри неё.

## Дополнительная информация

Для подробного описания структуры данных и работы с ними см. [data-structure.md](data-structure.md)

