using OneSTools.BracketsFile;
using System.Collections.Generic;
using System.IO;
using System;
using System.Collections.Immutable;
using OneSTools.FileDatabase.LowLevel;
using System.Collections.ObjectModel;
using System.Linq;

namespace OneSTools.FileDatabase.HighLevel
{
    public class Table
    {
        internal Dictionary<string, int> FieldNameNumberCache { get; private set; } = new();
        
        internal int MaxRowSize { get; private set; }
        internal uint DataFilePage { get; private set; }
        internal uint UnlimitedLengthDataFilePage { get; private set; }
        internal uint IndexFilePage { get; private set; }

        /// <summary>
        /// Internal name of the table
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Collection of table fields
        /// </summary>
        public Field[] Fields { get; private set; }
        /// <summary>
        /// Collection of table indexes
        /// </summary>
        public Index[] Indexes { get; private set; }
        public bool RecordLock { get; }
        /// <summary>
        /// Collection of table rows
        /// </summary>
        public IReadOnlyList<TableRow> Rows { get; private set; }

        internal Table(FileDatabaseStream stream, BracketsNode node)
        {
            Name = node[0];

            for (var i = 2; i < node.Count; i++)
            {
                var currentNode = node[i];
                string nodeName = currentNode[0];

                switch (nodeName)
                {
                    case "Fields":
                        ReadFields(currentNode);
                        break;
                    case "Indexes":
                        ReadIndexes(currentNode);
                        break;
                    case "Recordlock":
                        RecordLock = currentNode[1];
                        break;
                    case "Files":
                        DataFilePage = currentNode[1];
                        UnlimitedLengthDataFilePage = currentNode[2];
                        IndexFilePage = currentNode[3];
                        break;
                    default:
                        throw new Exception($"{nodeName} is unknown table description node");
                }
            }

            Rows = new TableRows(stream, this);
        }

        private void ReadFields(BracketsNode node)
        {
            var fields = new List<Field>();

            for (var i = 1; i < node.Count; i++)
            {
                var field = new Field();
                field.Read(node[i]);

                if (field.Type == FieldType.RowVersion)
                {
                    fields.Insert(0, field);
                }
                else
                    fields.Add(field);

                MaxRowSize += field.MaxSize;
            }

            // add "free row" mark length
            MaxRowSize++;

            // add "short version" data length
            if (fields.Count > 0 && fields[0].Type == FieldType.RowVersion && RecordLock)
                MaxRowSize += 8;

            Fields = fields.ToArray();
            FieldNameNumberCache = Fields.Select((e, i) => new KeyValuePair<string, int>(e.Name, i)).ToDictionary(e => e.Key, e => e.Value);
        }

        private void ReadIndexes(BracketsNode node)
        {
            var indexes = new List<Index>();

            if (node.Count > 1)
            {
                for (var i = 1; i < node.Count; i++)
                {
                    var index = new Index();
                    index.Read(node[i]);

                    indexes.Add(index);
                }
            }

            Indexes = indexes.ToArray();
        }

        public override string ToString()
            => Name;
    }
}
