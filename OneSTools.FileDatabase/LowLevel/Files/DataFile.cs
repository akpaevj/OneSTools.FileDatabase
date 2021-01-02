using System;

namespace OneSTools.FileDatabase.LowLevel.Files
{
    internal class DataFile : InnerFile
    {
        private readonly int _rowSize;
        private readonly bool _hasShortVersion;

        public DataFile(FileDatabaseStream stream, uint rootPageNumber, int rowSize, bool hasShortVersion) : base(stream, rootPageNumber)
        {
            _rowSize = rowSize;
            _hasShortVersion = hasShortVersion;
        }

        public void GoToRow(int number)
        {
            GoToDataStartingPosition();

            SkipBytes(_rowSize * number);
        }

        public byte[] ReadRow()
        {
            while (!EndOfDataStream)
            {
                var data = ReadBytes(_rowSize);

                // it's not a free row
                if (data[0] == 0)
                {
                    _dataStreamPosition += Convert.ToUInt64(_rowSize);

                    if (_hasShortVersion)
                        return data[9..];
                    else
                        return data[1..];
                }
            }

            return null;
        }
    }
}
