# Тесты проекта SharpExpo

## Структура тестов

Проект `SharpExpo.Tests.Unit` содержит unit-тесты для основных компонентов проекта.

## Запуск тестов

```bash
# Запустить все тесты
dotnet test

# Запустить тесты конкретного проекта
dotnet test tests/SharpExpo.Tests.Unit/SharpExpo.Tests.Unit.csproj

# Запустить тесты с подробным выводом
dotnet test --verbosity normal
```

## Покрытие тестами

### Модели данных (`Models/`)

#### OptionPropertyTests
- Валидация значений для всех типов (String, Double, Enumeration)
- Получение строкового представления значений
- Обработка null значений

#### FamilyOptionTests
- Валидация опций семейства
- Проверка обязательных полей
- Валидация вложенных OptionProperty

#### BimFamilyTests
- Валидация BIM-семейств
- Проверка обязательных полей (Id, Name, FamilyOptionIds)

#### BimFamilyDataTests
- Сортировка свойств по категориям согласно CategoryOrder
- Обработка категорий, отсутствующих в CategoryOrder
- Валидация полных данных семейства
- Проверка соответствия FamilyOptionIds и FamilyOptions

### Провайдеры данных (`Providers/`)

#### JsonBimFamilyDataProviderTests
- Загрузка данных семейства из JSON
- Обработка несуществующих семейств
- Преобразование null значений в пустые строки
- Загрузка списка всех семейств
- Обработка различных типов значений

## Статистика

- **Всего тестов:** 32
- **Покрытие:** Основные модели данных и провайдер данных

## Добавление новых тестов

При добавлении новых компонентов рекомендуется создавать соответствующие тесты:

1. Создайте файл тестов в соответствующей папке (`Models/`, `Providers/`, и т.д.)
2. Назовите класс тестов по шаблону `{ClassName}Tests`
3. Используйте xUnit атрибуты `[Fact]` для тестов
4. Следуйте паттерну AAA (Arrange-Act-Assert)

## Пример теста

```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var instance = new MyClass();

    // Act
    var result = instance.Method();

    // Assert
    Assert.NotNull(result);
}
```

