using OneSTools.BracketsFile;

namespace OneSTools.FileDatabase.HighLevel
{
    public class IndexField
    {
        /// <summary>
        /// Internal name of the index field
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The length of the value
        /// </summary>
        public int Length { get; private set; }

        internal void Read(BracketsNode node)
        {
            Name = node[0];
            Length = node[1];
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
