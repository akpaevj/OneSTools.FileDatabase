using System;
using OneSTools.BracketsFile;

namespace OneSTools.FileDatabase.HighLevel
{
    public class Field
    {
        /// <summary>
        /// Real size of the value (bytes)
        /// </summary>
        internal int MaxSize { get; private set; }
        /// <summary>
        /// Internal name of the field
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Type of the field's value
        /// </summary>
        public FieldType Type { get; private set; }
        /// <summary>
        /// If the flag is True a value can be null
        /// </summary>
        public bool Nullable { get; private set; }
        /// <summary>
        /// Length of the value
        /// </summary>
        public int Length { get; private set; }
        /// <summary>
        /// "Numeric" value only - position of a point in a value
        /// </summary>
        public int Precision { get; private set; }
        public string CaseSensitive { get; private set; }

        internal void Read(BracketsNode node)
        {
            Name = node[0];

            var type = (string)node[1];

            Type = type switch
            {
                "B" => FieldType.Binary,
                "L" => FieldType.Logical,
                "N" => FieldType.Numeric,
                "NC" => FieldType.NChar,
                "NVC" => FieldType.NVarChar,
                "RV" => FieldType.RowVersion,
                "NT" => FieldType.NText,
                "I" => FieldType.Image,
                "DT" => FieldType.DateTime,
                _ => throw new Exception($"{type} is unknown field type"),
            };

            Nullable = node[2];
            Length = node[3];
            Precision = node[4];
            CaseSensitive = node[5];

            CalculateRealSize();
        }

        public override string ToString()
            => $"{Name} ({Type})";

        private void CalculateRealSize()
        {
            MaxSize = Type switch
            {
                FieldType.Binary => Length,
                FieldType.Logical => 1,
                FieldType.Numeric => (Length + 1) / 2 + (Length + 1) % 2,
                FieldType.NChar => Length * 2,
                FieldType.NVarChar => Length * 2 + 2,
                FieldType.RowVersion => 16,
                FieldType.NText => 8,
                FieldType.Image => 8,
                FieldType.DateTime => 7,
                _ => throw new NotImplementedException($"There is no alghorithm to calculate size of \"{Type}\" type"),
            };

            if (Nullable)
                MaxSize++;
        }
    }
}
