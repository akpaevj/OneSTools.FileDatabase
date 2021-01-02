using OneSTools.FileDatabase.LowLevel.Files;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace OneSTools.FileDatabase.LowLevel.Pages
{
    internal class RootPage
    {
        public InnerFileType Type { get; private set; }
        public string Version { get; private set; }
        public ulong DataLength { get; private set; }
        public uint[] PageNumbers { get; private set; }

        public void Read(FileDatabaseStream reader)
        {
            Type = (InnerFileType)BinaryPrimitives.ReadUInt32LittleEndian(reader.ReadBytes(4));

            var v1 = BinaryPrimitives.ReadUInt32LittleEndian(reader.ReadBytes(4));
            var v2 = BinaryPrimitives.ReadUInt32LittleEndian(reader.ReadBytes(4));
            var v3 = BinaryPrimitives.ReadUInt32LittleEndian(reader.ReadBytes(4));

            Version = $"{v1}.{v2}.{v3}";

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
