// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com
namespace CsvHelper
{
    using System;
    using System.IO;

    using CsvHelper.Configuration;
    using CsvHelper.Interfaces;

    /// <summary>
    ///     Defines methods used to serialize data into a CSV file.
    /// </summary>
    public class CsvSerializer : ICsvSerializer
    {
        private readonly CsvConfiguration configuration;

        private bool disposed;

        private TextWriter writer;

        /// <summary>
        ///     Creates a new serializer using the given <see cref="TextWriter" />.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter" /> to write the CSV file data to.</param>
        public CsvSerializer(TextWriter writer)
            : this(writer, new CsvConfiguration())
        {
        }

        /// <summary>
        ///     Creates a new serializer using the given <see cref="TextWriter" />
        ///     and <see cref="CsvConfiguration" />.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter" /> to write the CSV file data to.</param>
        /// <param name="configuration">The configuration.</param>
        public CsvSerializer(TextWriter writer, CsvConfiguration configuration)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.writer = writer;
            this.configuration = configuration;
        }

        /// <summary>
        ///     Gets the configuration.
        /// </summary>
        public CsvConfiguration Configuration
        {
            get
            {
                return this.configuration;
            }
        }

        /// <summary>
        ///     Writes a record to the CSV file.
        /// </summary>
        /// <param name="record">The record to write.</param>
        public void Write(string[] record)
        {
            this.CheckDisposed();

            var recordString = string.Join(this.configuration.Delimiter, record);
            this.writer.WriteLine(recordString);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if the instance needs to be disposed of.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.writer != null)
                {
                    this.writer.Dispose();
                }
            }

            this.disposed = true;
            this.writer = null;
        }

        /// <summary>
        ///     Checks if the instance has been disposed of.
        /// </summary>
        /// <exception cref="ObjectDisposedException" />
        protected virtual void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().ToString());
            }
        }
    }
}