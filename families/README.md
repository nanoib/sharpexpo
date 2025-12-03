# Данные BIM-компонентов

Эта папка содержит данные для различных BIM-компонентов. Каждый компонент хранится в отдельной поддиректории.

## Структура

```
families/
  ├── 1/                          # Компонент 1
  │   ├── family-options.json    # Опции семейства для этого компонента
  │   └── Families/              # JSON файлы конкретных семейств
  │       └── {FamilyId}.json
  ├── 2/                          # Компонент 2
  │   ├── family-options.json
  │   └── Families/
  │       └── {FamilyId}.json
  └── ...
```

## Использование

### Запуск с указанием пути

```bash
SharpExpo.UI.exe --family-path "C:\repos\sharpexpo\families\1\family-options.json"
```

### Запуск без параметров

Если параметр `--family-path` не указан, приложение автоматически найдет первую директорию по алфавиту в `C:\repos\sharpexpo\families\` и использует файл `family-options.json` из этой директории.

## Формат файлов

- **family-options.json** - содержит все опции семейства (FamilyOption) с их свойствами (OptionProperty)
- **Families/{FamilyId}.json** - содержит описание конкретного BIM-семейства (BimFamily)

Подробнее о структуре данных см. [документацию](../docs/data-structure.md).

## JSON Schema

Схемы для валидации JSON файлов находятся в проекте `SharpExpo.Family`:
- `family-options.schema.json` - схема для family-options.json
- `bim-family.schema.json` - схема для файлов семейств


