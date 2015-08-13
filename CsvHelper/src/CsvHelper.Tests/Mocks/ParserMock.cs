// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com
namespace CsvHelper.Tests.Mocks
{
    using System.Collections;
    using System.Collections.Generic;

    using CsvHelper.Configuration;

    public class ParserMock : ICsvParser, IEnumerable<string[]>
    {
        public ParserMock()
        {
            this.Configuration = new CsvConfiguration();
            this.rows = new Queue<string[]>();
        }

        public ParserMock(Queue<string[]> rows)
        {
            this.Configuration = new CsvConfiguration();
            this.rows = rows;
        }

        public void Add(params string[] row)
        {
            this.rows.Enqueue(row);
        }

        public void Dispose()
        {
        }

        public CsvConfiguration Configuration { get; private set; }

        public int FieldCount { get; private set; }

        public long CharPosition { get; private set; }

        public long BytePosition { get; private set; }

        public int Row { get; private set; }

        public string RawRecord { get; private set; }

        public string[] Read()
        {
            this.Row++;
            return this.rows.Dequeue();
        }

        public IEnumerator<string[]> GetEnumerator()
        {
            return this.rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private readonly Queue<string[]> rows;
    }
}