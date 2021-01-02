using OneSTools.FileDatabase.LowLevel.Pages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OneSTools.FileDatabase.LowLevel.Files
{

    internal abstract class InnerFile
    {
        protected readonly FileDatabaseStream _stream;
        private uint _rootPageNumber;
        private readonly List<uint> _dataPages = new List<uint>();
        protected int _dataPage = -1;
        private long _position = 0;
        protected ulong _dataStreamPosition = 0;

        public RootPage RootPage { get; private set; }
        public List<IndexPage> IndexPages { get; private set; }
        public bool EndOfDataStream => _dataStreamPosition >= RootPage.DataLength;

        public InnerFile(FileDatabaseStream stream, uint rootPageNumber)
        {
            _stream = stream;
            _rootPageNumber = rootPageNumber;

            ReadRootPage();

            if (RootPage.Type == InnerFileType.Full)
            {
                ReadIndexPages();

                IndexPages.ForEach(c => _dataPages.AddRange(c.PageNumbers));
            }
            else
                _dataPages.AddRange(RootPage.PageNumbers);
        }

        public bool HasData()
            => RootPage.DataLength > 0;

        public void GoToDataStartingPosition()
        {
            if (!HasData())
                throw new Exception("The file doesn't contain data");

            _dataPage = -1;

            GoToNextDataPage();
        }

        protected byte[] ReadBytes(int count)
        {
            _stream.Position = _position;

            if (_dataPage == -1)
                GoToNextDataPage();

            var buffer = new byte[count];

            // Loop till read requested quantity
            var read = 0;

            while (read < count)
            {
                read += _stream.ReadBytes(buffer, read, count - read);

                if (read < count)
                    GoToNextDataPage();
            }

            if (PageReadingCompleted())
                GoToNextDataPage();

            _position = _stream.Position;

            return buffer;
        }

        protected void SkipBytes(long count)
        {
            _stream.Position = _position;

            if (_dataPage == -1)
                GoToNextDataPage();

            // Loop till read requested quantity
            long skipped = 0;

            while (skipped < count)
            {
                skipped += _stream.SkipBytes(count - skipped);

                if (skipped < count)
                    GoToNextDataPage();
            }

            if (PageReadingCompleted())
                GoToNextDataPage();

            _position = _stream.Position;
        }

        private void ReadRootPage()
        {
            _stream.CurrentPage = _rootPageNumber;

            RootPage = new RootPage();
            RootPage.Read(_stream);

            _position = _stream.Position;
        }

        private void ReadIndexPages()
        {
            IndexPages = new List<IndexPage>();

            foreach (var pageNumber in RootPage.PageNumbers)
            {
                if (pageNumber == 0)
                    break;
                else
                {
                    _stream.CurrentPage = pageNumber;

                    var indexPage = new IndexPage();
                    indexPage.Read(_stream);

                    IndexPages.Add(indexPage);
                }
            }

            _position = _stream.Position;
        }

        private bool PageReadingCompleted()
            => _dataPages[_dataPage] != _stream.CurrentPage;

        private void GoToNextDataPage()
        {
            _dataPage++;

            if (_dataPages.Count <= _dataPage)
                throw new Exception("There are no more data pages in the root page list");
            else
            {
                var dataPage = _dataPages[_dataPage];

                _stream.CurrentPage = dataPage;
            }

            _position = _stream.Position;
        }
    }
}
