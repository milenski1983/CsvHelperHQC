﻿namespace CsvHelper.Tests.Mocks
{
    using System;
    using System.Collections.Generic;
    using CsvHelper.Configuration;
    using CsvHelper.Interfaces;

    public class SerializerMock : ICsvSerializer
    {
        private readonly List<string[]> records = new List<string[]>();

        private readonly bool throwExceptionOnWrite;

        public SerializerMock(bool throwExceptionOnWrite = false)
        {
            this.Configuration = new CsvConfiguration();
            this.throwExceptionOnWrite = throwExceptionOnWrite;
        }

        public List<string[]> Records
        {
            get
            {
                return this.records;
            }
        }

        public CsvConfiguration Configuration { get; private set; }

        public void Write(string[] record)
        {
            if (this.throwExceptionOnWrite)
            {
                throw new Exception("Mock Write exception.");
            }

            this.records.Add(record);
        }

        public void Dispose()
        {
        }
    }
}