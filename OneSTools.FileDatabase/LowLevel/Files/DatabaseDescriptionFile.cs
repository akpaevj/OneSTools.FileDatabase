using System;
using System.Buffers.Binary;
using System.Text;
using System.Linq;

namespace OneSTools.FileDatabase.LowLevel.Files
{
    internal class DatabaseDescriptionFile: UnlimitedLengthDataFile
    {
        private int _currentTableStartBlock = -1;

        public string Language { get; private set; }
        public uint TablesCount { get; private set; }
        public uint[] TablesStartBlocks { get; private set; }

        public DatabaseDescriptionFile(FileDatabaseStream stream) : base(stream, 2)
        {
            GoToDataStartingPosition();

            SkipFirstBlock();

            var ddfHeaderData = ReadBlockChain();

            Language = Encoding.UTF8.GetString(ddfHeaderData[..(Array.IndexOf(ddfHeaderData, (byte)0))]);
            TablesCount = BinaryPrimitives.ReadUInt32LittleEndian(ddfHeaderData[32..36]);
            TablesStartBlocks = new uint[TablesCount];

            for (int i = 0; i < TablesCount; i++)
            {
                var startIndex = 36 + i * 4;
                var number = BinaryPrimitives.ReadUInt32LittleEndian(ddfHeaderData[startIndex..(startIndex + 4)]);

                if (number == 0)
                    break;

                TablesStartBlocks[i] = number;
            }
        }

        public byte[] ReadTableDefinitionData()
        {
            GoToNextTableStartBlock();

            return ReadBlockChain();
        }

        private void GoToNextTableStartBlock()
        {
            _currentTableStartBlock++;

            if (TablesStartBlocks.Length <= _currentTableStartBlock)
                throw new Exception("There are no more data pages in the root page list");
            else
                GoToBlock(TablesStartBlocks[_currentTableStartBlock]);
        }
    }
}
