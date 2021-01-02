using OneSTools.BracketsFile;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OneSTools.FileDatabase.HighLevel
{
    public class Index
    {
        /// <summary>
        /// Internal name of the index
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Collection of the index fields
        /// </summary>
        public ReadOnlyCollection<IndexField> Fields { get; private set; } = null;

        internal void Read(BracketsNode node)
        {
            var fields = new List<IndexField>();

            Name = (string)node[0];

            for (int i = 2; i < node.Count; i++)
            {
                var indexField = new IndexField();
                indexField.Read(node[i]);

                fields.Add(indexField);
            }

            Fields = fields.AsReadOnly();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
