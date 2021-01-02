using OneSTools.FileDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using OneSTools.FileDatabase.HighLevel;

namespace OneSTools.FileDatabaseTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var filePath = @"C:\Users\akpaev.e.ENTERPRISE\Desktop\Новая папка (2)\1Cv8.1CD";
            var filePath2 = @"C:\Users\akpaev.e.ENTERPRISE\Documents\InfoBase10\1Cv8.1CD";

            using var database = new FileDatabaseConnection(filePath2);
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
        }
    }
}
