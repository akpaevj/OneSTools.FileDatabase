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
            var filePath = "/home/usr1cv8/crserver/test/1cv8ddb.1CD";

            using var database = new FileDatabaseConnection(filePath);
            database.Open();

            foreach(var table in database.Tables)
            {
                Console.WriteLine($"Table \"{table}\":");

                // list the table fields
                Console.WriteLine("\tFields:");

                foreach (var field in table.Fields)
                    Console.WriteLine($"\t\tField \"{field}\"");

                if (table.Indexes.Length > 0)
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
        }
    }
}
