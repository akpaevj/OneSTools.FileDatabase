using System;
using System.IO;

namespace OneSTools.FileDatabase.LowLevel
{
    internal class FileDatabaseStream : IDisposable
    {
        private readonly BinaryReader reader;

        public long Position
        {
            get => reader.BaseStream.Position;
            set => reader.BaseStream.Position = value;
        }
        public uint PageSize { get; set; }
        public uint CurrentPage
        {
            get => (uint)(Position / PageSize);
            set => reader.BaseStream.Position = PageSize * value;
        }
        public uint PositionOnPage => (uint)(Position - (CurrentPage * PageSize));

        public FileDatabaseStream(Stream stream)
        {
            reader = new BinaryReader(stream);
        }

        public byte[] ReadBytes(int count)
            => reader.ReadBytes(count);

        public int ReadBytes(byte[] buffer, int offset, int count)
        {
            var forReading = MaxBytesCanTake(count);

            return reader.Read(buffer, offset, (int)forReading);
        }

        public long SkipBytes(long count)
        {
            var forSkip = MaxBytesCanTake(count);

            reader.BaseStream.Position += forSkip;

            return forSkip;
        }

        private long MaxBytesCanTake(long requestedCount)
            => Math.Min(((CurrentPage + 1) * PageSize) - Position, requestedCount);

        public void Dispose()
        {
            reader?.Dispose();
        }
    }
}
