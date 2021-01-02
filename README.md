# OneSTools.FileDatabase
[![Nuget](https://img.shields.io/nuget/v/OneSTools.FileDatabase)](https://www.nuget.org/packages/OneSTools.FileDatabase)  
Библиотека для чтения данных файловых информационных баз 1С. Поддерживается работа только с актуальной версией формата файлов (8.3.8) и только операции чтения  
Кроме обычных информационных баз, так-же дает возможность прочитать данные базы хранилища конфигурации.

Пример работы с библиотекой:  

```csharp
using var database = new FileDatabaseConnection("..\\1Cv8.1CD");
database.Open();

var table = database.Tables.FirstOrDefault(c => c.Name == "_Document38");

if (table != null)
{
    foreach (var values in table.Rows)
    {
        for (int i = 0; i < table.Fields.Count; i++)
        {
            var field = table.Fields[i];
            var value = values[i];

            // Or another one what you need
            if (field.Type == FieldType.Numeric)
            {
                var typedValue = (decimal?)value;
            }
            if (field.Type == FieldType.NChar
                || field.Type == FieldType.NText
                || field.Type == FieldType.NVarChar)
            {
                var typedValue = (string)value;
            }
        }
    }
}
```
Подключение представлено объектом **FileDatabaseConnection**, который предоставляет информацию об открытой базе и в числе прочего содержит коллекцию **Tables**  
Каждый элемент коллекции **Tables** представляет информацию о схеме таблицы и дает доступ к данным таблицы через свойство **Rows** (доступно использованеи **LINQ**)
