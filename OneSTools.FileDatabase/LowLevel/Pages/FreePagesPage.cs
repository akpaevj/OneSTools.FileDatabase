using System;
using System.Buffers.Binary;

namespace OneSTools.FileDatabase.LowLevel.Pages
{
    internal class FreePagesPage
    {
        public uint Type { get; private set; }
        public uint PagesCount { get; private set; }
        public int Version { get; private set; }
        public int[] PageNumbers { get; private set; }

        public void Read(FileDatabaseStream reader)
        {
            var type = BinaryPrimitives.ReadUInt32LittleEndian(reader.ReadBytes(4));

            if (!(type is 0x0000FF1C))
                throw new Exception($"{type} is not a type of \"Free pages\" block");

            Type = type;
            PagesCount = BinaryPrimitives.ReadUInt32LittleEndian(reader.ReadBytes(4));
            Version = BinaryPrimitives.ReadInt32LittleEndian(reader.ReadBytes(4));

            PageNumbers = new int[PagesCount];

            for (int i = 0; i < PagesCount; i++)
            {
                var number = BinaryPrimitives.ReadInt32LittleEndian(reader.ReadBytes(4));

                if (number == 0)
                    break;

                PageNumbers[i] = number;
            }
        }
    }
}
