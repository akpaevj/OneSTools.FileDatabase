using System;
using System.Buffers.Binary;
using System.Text;

namespace OneSTools.FileDatabase.LowLevel.Pages
{
    internal class HeaderPage
    {
        public string Signature { get; private set; }
        public string Version { get; private set; }
        public uint PagesCount { get; private set; }
        public uint PageSize { get; private set; }

        public void Read(FileDatabaseStream reader)
        {
            var signature = Encoding.UTF8.GetString(reader.ReadBytes(8));
            if (signature != "1CDBMSV8")
                throw new Exception("This file is not a 1C database");

            var versionData = reader.ReadBytes(4);
            var version = $"{versionData[0]}.{versionData[1]}.{versionData[2]}.{versionData[3]}";
            if (version != "8.3.8.0")
                throw new Exception($"{version} is unknown version");

            Signature = signature;
            Version = version;
            PagesCount = BinaryPrimitives.ReadUInt32LittleEndian(reader.ReadBytes(4));

            // skip 4 bytes of unknown value
            reader.ReadBytes(4);

            PageSize = BinaryPrimitives.ReadUInt32LittleEndian(reader.ReadBytes(4));
        }
    }
}
