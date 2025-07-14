using System.Collections.Generic;
using System.Text;
using System;
using OneSTools.FileDatabase.LowLevel;
using OneSTools.FileDatabase.LowLevel.Files;
using System.Buffers.Binary;
using System.Collections;
using System.Globalization;

namespace OneSTools.FileDatabase.HighLevel
{
    internal class TableRows : IReadOnlyList<TableRow>
    {
        private readonly FileDatabaseStream _stream;
        private readonly Table _table;
        private DataFile _dataFile;
        private UnlimitedLengthDataFile _unlimitedLengthDataFile;

        internal TableRows(FileDatabaseStream stream, Table table)
        {
            _stream = stream;
            _table = table;
        }

        public TableRow this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new IndexOutOfRangeException();
                else
                {
                    return Get(index + 1);
                }
            }
        }

        public int Count
        {
            get
            {
                InitializeFiles();

                return (int)(_dataFile?.RootPage.DataLength / Convert.ToUInt64(_table.MaxRowSize));
            }
        }

        public IEnumerator<TableRow> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return Get(i + 1);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void InitializeFiles()
        {
            if (_dataFile == null)
            {
                if (_table.DataFilePage != 0)
                {
                    _dataFile = new DataFile(_stream, _table.DataFilePage, _table.MaxRowSize, _table.Fields.Length > 0 && _table.Fields[0].Type != FieldType.RowVersion && _table.RecordLock);

                    if (_dataFile.HasData())
                    {
                        _dataFile.GoToDataStartingPosition();
                        _dataFile.GoToRow(1);
                    }
                }

                if (_table.UnlimitedLengthDataFilePage != 0)
                {
                    _unlimitedLengthDataFile = new UnlimitedLengthDataFile(_stream, _table.UnlimitedLengthDataFilePage);

                    if (_unlimitedLengthDataFile.HasData())
                        _dataFile.GoToDataStartingPosition();
                }
            }
        }

        private TableRow Get(int rowNumber)
        {
            InitializeFiles();

            _dataFile.GoToRow(rowNumber);

            var values = new object[_table.Fields.Length];

            var rawData = _dataFile.ReadRow();

            if (rawData == null)
                return null;
            
            var currentOffset = 0;

            for (var i = 0; i < _table.Fields.Length; i++)
            {
                var field = _table.Fields[i];

                var fieldData = rawData[currentOffset..(currentOffset + field.MaxSize)];

                currentOffset += field.MaxSize;

                values[i] = field.Type switch
                {
                    FieldType.Binary => ReadBinary(fieldData, field.Nullable),
                    FieldType.Logical => ReadLogical(fieldData, field.Nullable),
                    FieldType.Numeric => ReadNumericValue(fieldData, field.Precision, field.Nullable),
                    FieldType.NChar => ReadNChar(fieldData, field.Nullable),
                    FieldType.NVarChar => ReadNVarChar(fieldData, field.Nullable),
                    FieldType.RowVersion => ReadRowVersion(fieldData, field.Nullable),
                    FieldType.NText => ReadNText(fieldData, field.Nullable),
                    FieldType.Image => ReadImage(fieldData, field.Nullable),
                    FieldType.DateTime => ReadDateTime(fieldData, field.Nullable),
                    _ => throw new Exception($"Reading value for a field with type {field.Type} is not implemented")
                };
            }

            return new TableRow(_table, values);
        }

        private byte[] ReadBinary(byte[] data, bool nullable)
        {
            if (!HasValue(data, nullable))
                return null;
            else
            {
                var valueData = GetValueData(data, nullable);

                return valueData;
            }
        }

        private bool? ReadLogical(byte[] data, bool nullable)
        {
            if (!HasValue(data, nullable))
                return null;
            else
            {
                var valueData = GetValueData(data, nullable);

                return valueData[0] != 0;
            }
        }

        private decimal? ReadNumericValue(byte[] data, int precision, bool nullable)
        {
            if (!HasValue(data, nullable))
                return null;
            else
            {
                var valueData = GetValueData(data, nullable);
                var doubleStr = new StringBuilder();
                
                var negative = ReadTetrad(valueData[0]) == 0;
                if (negative)
                    doubleStr.Append('-');

                doubleStr.Append(ReadTetrad(valueData[0], true));

                for (int i = 1; i < valueData.Length; i++)
                {
                    doubleStr.Append(ReadTetrad(valueData[i]));
                    doubleStr.Append(ReadTetrad(valueData[i], true));
                }

                if (precision > 0)
                    doubleStr.Insert(doubleStr.Length - 1 - precision, '.');

                // remove last zero, I don't know what is it
                doubleStr.Remove(doubleStr.Length - 1, 1);

                return decimal.Parse(doubleStr.ToString(), new NumberFormatInfo() { NumberDecimalSeparator = "." });
            }
        }

        private void AddNumberToNumeric(StringBuilder doubleStr, int value)
        {
            // don't add leading zeros
            if (value != 0)
                doubleStr.Append(value);
            else if (doubleStr.Length > 0)
                doubleStr.Append(value);
        }

        private string ReadNChar(byte[] data, bool nullable)
        {
            if (!HasValue(data, nullable))
                return null;
            else
            {
                var valueData = GetValueData(data, nullable);

                return Encoding.Unicode.GetString(valueData);
            }
        }

        private string ReadNVarChar(byte[] data, bool nullable)
        {
            if (!HasValue(data, nullable))
                return null;
            else
            {
                var valueData = GetValueData(data, nullable);

                var length = BinaryPrimitives.ReadUInt16LittleEndian(valueData);

                if (length == 0)
                    return "";
                else
                    return Encoding.Unicode.GetString(valueData[2..(length * 2 + 2)]);
            }
        }

        private DateTime? ReadDateTime(byte[] data, bool nullable)
        {
            if (!HasValue(data, nullable))
                return null;
            else
            {
                var valueData = GetValueData(data, nullable);

                var year1 = ReadTetrad(valueData[0]);
                var year2 = ReadTetrad(valueData[0], true);
                var year3 = ReadTetrad(valueData[1]);
                var year4 = ReadTetrad(valueData[1], true);
                var year = int.Parse($"{year1}{year2}{year3}{year4}");

                var month1 = ReadTetrad(valueData[2]);
                var month2 = ReadTetrad(valueData[2], true);
                var month = int.Parse($"{month1}{month2}");

                var day1 = ReadTetrad(valueData[3]);
                var day2 = ReadTetrad(valueData[3], true);
                var day = int.Parse($"{day1}{day2}");

                var hour1 = ReadTetrad(valueData[4]);
                var hour2 = ReadTetrad(valueData[4], true);
                var hour = int.Parse($"{hour1}{hour2}");

                var minute1 = ReadTetrad(valueData[5]);
                var minute2 = ReadTetrad(valueData[5], true);
                var minute = int.Parse($"{minute1}{minute2}");

                var second1 = ReadTetrad(valueData[6]);
                var second2 = ReadTetrad(valueData[6], true);
                var second = int.Parse($"{second1}{second2}");

                if (year == 0 && month == 0 && day == 0 && hour == 0 && minute == 0 && second == 0)
                    return DateTime.MinValue;
                else
                    return new DateTime(year, month, day, hour, minute, second);
            }
        }

        private string ReadRowVersion(byte[] data, bool nullable)
        {
            if (!HasValue(data, nullable))
                return null;
            else
            {
                var valueData = GetValueData(data, nullable);

                var v1 = BinaryPrimitives.ReadUInt32LittleEndian(valueData[..5]);
                var v2 = BinaryPrimitives.ReadUInt32LittleEndian(valueData[5..9]);
                var v3 = BinaryPrimitives.ReadUInt32LittleEndian(valueData[9..13]);
                var v4 = BinaryPrimitives.ReadUInt32LittleEndian(valueData[12..]);

                return $"{v1}.{v2}.{v3}.{v4}";
            }
        }

        private string ReadNText(byte[] data, bool nullable)
        {
            var valueData = ReadUnlimitedData(data, nullable);

            if (valueData is null)
                return null;
            else
                return Encoding.Unicode.GetString(valueData);
        }

        private byte[] ReadImage(byte[] data, bool nullable)
        {
            var valueData = ReadUnlimitedData(data, nullable);

            if (valueData is null)
                return null;
            else
                return valueData;
        }

        private byte[] ReadUnlimitedData(byte[] data, bool nullable)
        {
            if (!HasValue(data, nullable))
                return null;

            var valueData = GetValueData(data, nullable);

            var blockNumber = BinaryPrimitives.ReadUInt32LittleEndian(valueData);
            var dataLength = BinaryPrimitives.ReadUInt32LittleEndian(valueData[4..]);

            _unlimitedLengthDataFile.GoToBlock(blockNumber);

            var valueRawData = _unlimitedLengthDataFile.ReadBlockChain();

            return valueRawData[..(int)dataLength];
        }

        private bool HasValue(byte[] data, bool nullable)
        {
            if (nullable)
                return (bool)ReadLogical(data, false);
            else
                return true;
        }

        private static byte[] GetValueData(byte[] data, bool nullable)
            => nullable ? data[1..] : data;

        private static int ReadTetrad(byte b, bool second = false)
        {
            if (second)
                return b & 0b_0000_1111;
            else
                return b >> 4;
        }
    }
}
