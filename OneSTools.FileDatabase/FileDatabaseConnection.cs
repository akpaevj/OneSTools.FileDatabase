using OneSTools.FileDatabase.HighLevel;
using System;
using System.Collections.Generic;
using System.IO;
using OneSTools.FileDatabase.LowLevel;
using OneSTools.FileDatabase.LowLevel.Pages;
using OneSTools.FileDatabase.LowLevel.Files;
using OneSTools.BracketsFile;
using System.Text;

namespace OneSTools.FileDatabase
{
    /// <summary>
    /// Provides properties and methods for reading 1C file database data
    /// </summary>
    public class FileDatabaseConnection(string path) : IDisposable
    {
        private FileDatabaseStream _stream;
        private HeaderPage _headerPage;
        private FreePagesPage _freePagesPage;
        private DatabaseDescriptionFile _databaseDescriptionFile;

        /// <summary>
        /// The flag of database opening
        /// </summary>
        public bool Opened { get; private set; }
        /// <summary>
        /// Version of the database file
        /// </summary>
        public string Version => _headerPage?.Version;
        /// <summary>
        /// Locale of the database
        /// </summary>
        public string Language => _databaseDescriptionFile?.Language;
        /// <summary>
        /// A collection of database tables
        /// </summary>
        public Tables Tables { get; private set; }
        
        public Table this[string tableName] => Tables[tableName];

        /// <summary>
        /// Open a database file
        /// </summary>
        public void Open()
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Cannot find a database file", path);

            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _stream = new FileDatabaseStream(stream);

            ReadStructure();

            Opened = true;
        }

        private void ReadStructure()
        {
            _headerPage = new HeaderPage();
            _headerPage.Read(_stream);

            _stream.PageSize = _headerPage.PageSize;

            _stream.CurrentPage = 1;
            _freePagesPage = new FreePagesPage();
            _freePagesPage.Read(_stream);

            _databaseDescriptionFile = new DatabaseDescriptionFile(_stream);

            var tables = new List<Table>();

            for (var i = 0; i < _databaseDescriptionFile.TablesCount; i++)
            {
                var tableDefinitionData = _databaseDescriptionFile.ReadTableDefinitionData();
                var tableDefinitionStr = Encoding.UTF8.GetString(tableDefinitionData);
                var tableDefinitionNode = BracketsParser.ParseBlock(tableDefinitionStr);

                var table = new Table(_stream, tableDefinitionNode);
                tables.Add(table);
            }

            Tables = new Tables(tables.AsReadOnly()); 
        }

        /// <summary>
        /// Close the database file
        /// </summary>
        public void Close()
        {
            Opened = false;
            _stream?.Dispose();
        }

        public void Dispose()
            => Close();
    }
}
