# OneSTools.FileDatabase
[![Nuget](https://img.shields.io/nuget/v/OneSTools.FileDatabase)](https://www.nuget.org/packages/OneSTools.FileDatabase)  
Библиотека для чтения данных файловых информационных баз 1С. Поддерживается работа только с версией формата файлов 8.3.8 и только операции чтения.  
Кроме обычных информационных баз, так-же дает возможность прочитать данные базы хранилища конфигураций.

Пример работы с библиотекой:  

```csharp
using var database = new FileDatabaseConnection("..\\1Cv8.1CD");
database.Open();

foreach(var table in database.Tables)
{
    Console.WriteLine($"Table \"{table}\":");

    // list the table fields
    Console.WriteLine("\tFields:");

    foreach (var field in table.Fields)
        Console.WriteLine($"\t\tField \"{field}\"");

    if (table.Indexes.Count > 0)
    {
        // list the table indexes
        Console.WriteLine("\tIndexes:");

        foreach (var index in table.Indexes)
        {
            Console.WriteLine($"\t\tIndex \"{index}\"");

            // list the index fields
            Console.WriteLine("\t\t\tIndex fields:");

            foreach (var indexField in index.Fields)
                Console.WriteLine($"\t\t\t\tIndex field \"{indexField}\"");

        }
    }

    // list the table data (rows)
    if (table.Rows.Count > 0)
    {
        Console.WriteLine("\tRows:");

        // values is an array of objects, it contains values as in the same order as fields are represented
        foreach (var values in table.Rows)
        {
            Console.WriteLine("\t\tRow");
        }
    }
}
```
Подключение представлено объектом **FileDatabaseConnection**, который предоставляет информацию об открытой базе и в числе прочего содержит коллекцию **Tables**  
Каждый элемент коллекции **Tables** представляет информацию о схеме таблицы и дает доступ к данным таблицы через свойство **Rows** (доступно использование **LINQ**)  
Данные строки - массив **object**, где количество элементов равно количеству полей таблицы и порядок так-же равен порядку полей в коллекции **Fields** таблицы.
