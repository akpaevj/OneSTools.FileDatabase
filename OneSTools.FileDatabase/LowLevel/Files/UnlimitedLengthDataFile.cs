using System.Buffers.Binary;
using System.Collections.Generic;
using System;

namespace OneSTools.FileDatabase.LowLevel.Files
{

    internal class UnlimitedLengthDataFile : InnerFile
    {
        private const int UNLIMITED_DATA_BLOCK_MAX_SIZE = 256;

        public UnlimitedLengthDataFile(FileDatabaseStream stream, uint rootPageNumber) : base(stream, rootPageNumber)
        {

        }

        public void SkipFirstBlock()
        {
            // It skips the first block, cause it's a first block of the chain of free blocks
            SkipBytes(UNLIMITED_DATA_BLOCK_MAX_SIZE);
        }

        public void GoToBlock(uint blockNumber)
        {
            GoToDataStartingPosition();

            SkipBytes(blockNumber * UNLIMITED_DATA_BLOCK_MAX_SIZE);
        }

        public byte[] ReadBlockChain()
        {
            var buffer = new List<byte>();

            while (true)
            {
                var nextBlock = BinaryPrimitives.ReadUInt32LittleEndian(ReadBytes(4));
                var blockDatalength = BinaryPrimitives.ReadUInt16LittleEndian(ReadBytes(2));

                if (blockDatalength > 0)
                {
                    var blockData = ReadBytes(blockDatalength);
                    buffer.AddRange(blockData);
                }

                if (nextBlock == 0)
                    break;
                else
                    GoToBlock(nextBlock);
            }

            return buffer.ToArray();
        }
    }
}
