using System.Buffers.Binary;
using System.Collections.Generic;

namespace OneSTools.FileDatabase.LowLevel.Pages
{
    internal class IndexPage
    {
        public ulong DataLength { get; private set; }
        public uint[] PageNumbers { get; private set; }

        public virtual void Read(FileDatabaseStream reader)
        {
            DataLength = BinaryPrimitives.ReadUInt64LittleEndian(reader.ReadBytes(8));

            var pageNumbers = new List<uint>();

            while (true)
            {
                var pageNumber = BinaryPrimitives.ReadUInt32LittleEndian(reader.ReadBytes(4));

                if (pageNumber == 0)
                    break;

                pageNumbers.Add(pageNumber);
            }

            PageNumbers = pageNumbers.ToArray();
        }
    }
}
