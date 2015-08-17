// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com

#if NET_2_0
using CsvHelper.MissingFrom20;
#endif
#if !NET_2_0

#endif

namespace CsvHelper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CsvHelper.Configuration;
    using CsvHelper.Interfaces;

    /// <summary>
    ///     Parses a CSV file.
    /// </summary>
    public class CsvParser : ICsvParser
    {
        private readonly CsvConfiguration configuration;

        private readonly char[] readerBuffer;

        private char c = '\0';

        private int charsRead;

        private char? cPrev;

        private int currentRawRow;

        private int currentRow;

        private bool disposed;

        private bool hasExcelSeparatorBeenRead;

        private bool read;

        private TextReader reader;

        private int readerBufferPosition;

        private string[] record;

        /// <summary>
        ///     Creates a new parser using the given <see cref="TextReader" />.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader" /> with the CSV file data.</param>
        public CsvParser(TextReader reader)
            : this(reader, new CsvConfiguration())
        {
        }

        /// <summary>
        ///     Creates a new parser using the given <see cref="TextReader" />
        ///     and <see cref="CsvConfiguration" />.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader" /> with the CSV file data.</param>
        /// <param name="configuration">The configuration.</param>
        public CsvParser(TextReader reader, CsvConfiguration configuration)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.reader = reader;
            this.configuration = configuration;

            this.readerBuffer = new char[configuration.BufferSize];
        }

        /// <summary>
        ///     Gets the row of the CSV file that the parser is currently on.
        ///     This is the actual file row.
        /// </summary>
        public virtual int RawRow
        {
            get
            {
                return this.currentRawRow;
            }
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
                if (this.reader != null)
                {
                    this.reader.Dispose();
                }
            }

            this.disposed = true;
            this.reader = null;
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

        /// <summary>
        ///     Adds the field to the current record.
        /// </summary>
        /// <param name="recordPosition">The record position to add the field to.</param>
        /// <param name="field">The field to add.</param>
        protected virtual void AddFieldToRecord(ref int recordPosition, string field, ref bool fieldIsBad)
        {
            if (this.record.Length < recordPosition + 1)
            {
                // Resize record if it's too small.
                Array.Resize(ref this.record, recordPosition + 1);

                // Set the field count. If there is a header
                // record, then we can go by the number of
                // headers there is. If there is no header
                // record, then we can go by the first row.
                // Either way, we're using the first row.
                if (this.currentRow == 1)
                {
                    this.FieldCount = this.record.Length;
                }
            }

            if (fieldIsBad && this.configuration.ThrowOnBadData)
            {
                throw new CsvBadDataException(string.Format("Field: '{0}'", field));
            }

            if (fieldIsBad && this.configuration.BadDataCallback != null)
            {
                this.configuration.BadDataCallback(field);
            }

            fieldIsBad = false;

            this.record[recordPosition] = field;
            recordPosition++;
        }

        /// <summary>
        ///     Appends the current buffer data to the field.
        /// </summary>
        /// <param name="field">The field to append the current buffer to.</param>
        /// <param name="fieldStartPosition">The start position in the buffer that the .</param>
        /// <param name="length">The length.</param>
        protected virtual void AppendField(ref string field, int fieldStartPosition, int length)
        {
            field += new string(this.readerBuffer, fieldStartPosition, length);
        }

        /// <summary>
        ///     Updates the byte position using the data from the reader buffer.
        /// </summary>
        /// <param name="fieldStartPosition">The field start position.</param>
        /// <param name="length">The length.</param>
        protected virtual void UpdateBytePosition(int fieldStartPosition, int length)
        {
            if (this.configuration.CountBytes)
            {
                this.BytePosition += this.configuration.Encoding.GetByteCount(
                    this.readerBuffer, 
                    fieldStartPosition, 
                    length);
            }
        }

        /// <summary>
        ///     Reads the next line.
        /// </summary>
        /// <returns>The line separated into fields.</returns>
        protected virtual string[] ReadLine()
        {
            string field = null;
            var fieldStartPosition = this.readerBufferPosition;
            var rawFieldStartPosition = this.readerBufferPosition;
            var inQuotes = false;
            var fieldIsEscaped = false;
            var fieldIsBad = false;
            var inComment = false;
            var inDelimiter = false;
            var inExcelLeadingZerosFormat = false;
            var delimiterPosition = 0;
            var prevCharWasDelimiter = false;
            var recordPosition = 0;
            this.record = new string[this.FieldCount];
            this.RawRecord = string.Empty;
            this.currentRow++;
            this.currentRawRow++;

            while (true)
            {
                if (this.read)
                {
                    this.cPrev = this.c;
                }

                var fieldLength = this.readerBufferPosition - fieldStartPosition;
                this.read = this.GetChar(
                    out this.c, 
                    ref fieldStartPosition, 
                    ref rawFieldStartPosition, 
                    ref field, 
                    ref fieldIsBad, 
                    prevCharWasDelimiter, 
                    ref recordPosition, 
                    ref fieldLength, 
                    inComment, 
                    inDelimiter, 
                    inQuotes, 
                    false);
                if (!this.read)
                {
                    break;
                }

                this.readerBufferPosition++;
                this.CharPosition++;

                // This needs to get in the way and parse things completely different
                // from how a normal CSV field works. This horribly ugly.
                if (this.configuration.UseExcelLeadingZerosFormatForNumerics)
                {
                    if (this.c == '=' && !inExcelLeadingZerosFormat
                        && (prevCharWasDelimiter || this.cPrev == '\r' || this.cPrev == '\n' || this.cPrev == null))
                    {
                        // The start of the leading zeros format has been hit.
                        fieldLength = this.readerBufferPosition - fieldStartPosition;
                        char cNext;
                        this.GetChar(
                            out cNext, 
                            ref fieldStartPosition, 
                            ref rawFieldStartPosition, 
                            ref field, 
                            ref fieldIsBad, 
                            prevCharWasDelimiter, 
                            ref recordPosition, 
                            ref fieldLength, 
                            inComment, 
                            inDelimiter, 
                            inQuotes, 
                            true);
                        if (cNext == '"')
                        {
                            inExcelLeadingZerosFormat = true;
                            continue;
                        }
                    }
                    else if (inExcelLeadingZerosFormat)
                    {
                        if (this.c == '"' && this.cPrev == '=' || char.IsDigit(this.c))
                        {
                            // Inside of the field.
                        }
                        else if (this.c == '"')
                        {
                            // The end of the field has been hit.
                            char cNext;
                            var peekRead = this.GetChar(
                                out cNext, 
                                ref fieldStartPosition, 
                                ref rawFieldStartPosition, 
                                ref field, 
                                ref fieldIsBad, 
                                prevCharWasDelimiter, 
                                ref recordPosition, 
                                ref fieldLength, 
                                inComment, 
                                inDelimiter, 
                                inQuotes, 
                                true);
                            if (cNext == this.configuration.Delimiter[0] || cNext == '\r' || cNext == '\n'
                                || cNext == '\0')
                            {
                                this.AppendField(
                                    ref field, 
                                    fieldStartPosition, 
                                    this.readerBufferPosition - fieldStartPosition);
                                this.UpdateBytePosition(
                                    fieldStartPosition, 
                                    this.readerBufferPosition - fieldStartPosition);
                                field = field.Trim('=', '"');
                                fieldStartPosition = this.readerBufferPosition;

                                if (!peekRead)
                                {
                                    this.AddFieldToRecord(ref recordPosition, field, ref fieldIsBad);
                                }

                                inExcelLeadingZerosFormat = false;
                            }
                        }
                        else
                        {
                            inExcelLeadingZerosFormat = false;
                        }

                        continue;
                    }
                }

                if (this.c == this.configuration.Quote && !this.configuration.IgnoreQuotes)
                {
                    if (!fieldIsEscaped
                        && (prevCharWasDelimiter || this.cPrev == '\r' || this.cPrev == '\n' || this.cPrev == null))
                    {
                        // The field is escaped only if the first char of
                        // the field is a quote.
                        fieldIsEscaped = true;
                    }

                    if (!fieldIsEscaped)
                    {
                        fieldIsBad = true;

                        // If the field isn't escaped, the quote
                        // is like any other char and we should
                        // just ignore it.
                        continue;
                    }

                    inQuotes = !inQuotes;

                    if (fieldStartPosition != this.readerBufferPosition - 1)
                    {
                        // Grab all the field chars before the
                        // quote if there are any.
                        this.AppendField(
                            ref field, 
                            fieldStartPosition, 
                            this.readerBufferPosition - fieldStartPosition - 1);

                        // Include the quote in the byte count.
                        this.UpdateBytePosition(fieldStartPosition, this.readerBufferPosition - fieldStartPosition);
                    }

                    if (this.cPrev != this.configuration.Quote || !inQuotes)
                    {
                        if (inQuotes || this.cPrev == this.configuration.Quote || this.readerBufferPosition == 1)
                        {
                            // The quote will be uncounted and needs to be catered for if:
                            // 1. It's the opening quote
                            // 2. It's the closing quote on an empty field ("")
                            // 3. It's the closing quote and has appeared as the first character in the buffer
                            this.UpdateBytePosition(fieldStartPosition, this.readerBufferPosition - fieldStartPosition);
                        }

                        // Set the new field start position to
                        // the char after the quote.
                        fieldStartPosition = this.readerBufferPosition;
                    }

                    prevCharWasDelimiter = false;

                    continue;
                }

                prevCharWasDelimiter = false;

                if (fieldIsEscaped && inQuotes)
                {
                    if (this.c == '\r' || (this.c == '\n' && this.cPrev != '\r'))
                    {
                        this.currentRawRow++;
                    }

                    // While inside an escaped field,
                    // all chars are ignored.
                    continue;
                }

                if (this.cPrev == this.configuration.Quote && !this.configuration.IgnoreQuotes)
                {
                    if (this.c != this.configuration.Delimiter[0] && this.c != '\r' && this.c != '\n')
                    {
                        fieldIsBad = true;
                    }

                    // If we're not in quotes and the
                    // previous char was a quote, the
                    // field is no longer escaped.
                    fieldIsEscaped = false;
                }

                if (inComment && this.c != '\r' && this.c != '\n')
                {
                    // We are on a commented line.
                    // Ignore the character.
                }
                else if (this.c == this.configuration.Delimiter[0] || inDelimiter)
                {
                    if (!inDelimiter)
                    {
                        // If we hit the delimiter, we are
                        // done reading the field and can
                        // add it to the record.
                        this.AppendField(
                            ref field, 
                            fieldStartPosition, 
                            this.readerBufferPosition - fieldStartPosition - 1);

                        // Include the comma in the byte count.
                        this.UpdateBytePosition(fieldStartPosition, this.readerBufferPosition - fieldStartPosition);
                        this.AddFieldToRecord(ref recordPosition, field, ref fieldIsBad);
                        fieldStartPosition = this.readerBufferPosition;
                        field = null;

                        inDelimiter = true;
                    }

                    if (delimiterPosition == this.configuration.Delimiter.Length - 1)
                    {
                        // We are done reading the delimeter.

                        // Include the delimiter in the byte count.
                        this.UpdateBytePosition(fieldStartPosition, this.readerBufferPosition - fieldStartPosition);
                        inDelimiter = false;
                        prevCharWasDelimiter = true;
                        delimiterPosition = 0;
                        fieldStartPosition = this.readerBufferPosition;
                    }
                    else
                    {
                        delimiterPosition++;
                    }
                }
                else if (this.c == '\r' || this.c == '\n')
                {
                    fieldLength = this.readerBufferPosition - fieldStartPosition - 1;
                    if (this.c == '\r')
                    {
                        char cNext;
                        this.GetChar(
                            out cNext, 
                            ref fieldStartPosition, 
                            ref rawFieldStartPosition, 
                            ref field, 
                            ref fieldIsBad, 
                            prevCharWasDelimiter, 
                            ref recordPosition, 
                            ref fieldLength, 
                            inComment, 
                            inDelimiter, 
                            inQuotes, 
                            true);
                        if (cNext == '\n')
                        {
                            this.readerBufferPosition++;
                            this.CharPosition++;
                        }
                    }

                    if (this.cPrev == '\r' || this.cPrev == '\n' || inComment || this.cPrev == null)
                    {
                        // We have hit a blank line. Ignore it.
                        this.UpdateBytePosition(fieldStartPosition, this.readerBufferPosition - fieldStartPosition);

                        fieldStartPosition = this.readerBufferPosition;
                        inComment = false;

                        if (!this.configuration.IgnoreBlankLines)
                        {
                            break;
                        }

                        // If blank lines are being ignored, we need to add
                        // to the row count because we're skipping the row
                        // and it won't get added normally.
                        this.currentRow++;

                        continue;
                    }

                    // If we hit the end of the record, add 
                    // the current field and return the record.
                    this.AppendField(ref field, fieldStartPosition, fieldLength);

                    // Include the \r or \n in the byte count.
                    this.UpdateBytePosition(fieldStartPosition, this.readerBufferPosition - fieldStartPosition);
                    this.AddFieldToRecord(ref recordPosition, field, ref fieldIsBad);
                    break;
                }
                else if (this.configuration.AllowComments && this.c == this.configuration.Comment
                         && (this.cPrev == '\r' || this.cPrev == '\n' || this.cPrev == null))
                {
                    inComment = true;
                }
            }

            if (this.record != null)
            {
                this.RawRecord += new string(
                    this.readerBuffer, 
                    rawFieldStartPosition, 
                    this.readerBufferPosition - rawFieldStartPosition);
            }

            return this.record;
        }

        /// <summary>
        ///     Gets the current character from the buffer while
        ///     advancing the buffer if it ran out.
        /// </summary>
        /// <param name="ch">The char that gets the read char set to.</param>
        /// <param name="fieldStartPosition">The start position of the current field.</param>
        /// <param name="rawFieldStartPosition">The start position of the raw field.</param>
        /// <param name="field">The field.</param>
        /// <param name="prevCharWasDelimiter">A value indicating if the previous char read was a delimiter.</param>
        /// <param name="recordPosition">The position in the record we are currently at.</param>
        /// <param name="fieldLength">The length of the field in the buffer.</param>
        /// <param name="inComment">A value indicating if the row is current a comment row.</param>
        /// <param name="isPeek">
        ///     A value indicating if this call is a peek. If true and the end of the record was found
        ///     no record handling will be done.
        /// </param>
        /// <returns>A value indicating if read a char was read. True if a char was read, otherwise false.</returns>
        protected bool GetChar(
            out char ch, 
            ref int fieldStartPosition, 
            ref int rawFieldStartPosition, 
            ref string field, 
            ref bool fieldIsBad, 
            bool prevCharWasDelimiter, 
            ref int recordPosition, 
            ref int fieldLength, 
            bool inComment, 
            bool inDelimiter, 
            bool inQuotes, 
            bool isPeek)
        {
            if (this.readerBufferPosition == this.charsRead)
            {
                // We need to read more of the stream.
                if (!inDelimiter)
                {
                    // The buffer ran out. Take the current
                    // text and add it to the field.
                    this.AppendField(ref field, fieldStartPosition, fieldLength);
                }

                this.UpdateBytePosition(fieldStartPosition, this.readerBufferPosition - fieldStartPosition);
                fieldLength = 0;

                this.RawRecord += new string(
                    this.readerBuffer, 
                    rawFieldStartPosition, 
                    this.readerBufferPosition - rawFieldStartPosition);

                this.charsRead = this.reader.Read(this.readerBuffer, 0, this.readerBuffer.Length);
                this.readerBufferPosition = 0;
                fieldStartPosition = 0;
                rawFieldStartPosition = 0;

                if (this.charsRead == 0)
                {
                    // The end of the stream has been reached.
                    if (isPeek)
                    {
                        // Don't do any record handling because we're just looking ahead
                        // and not actually getting the next char to use.
                        ch = '\0';
                        return false;
                    }

                    if ((this.c != '\r' && this.c != '\n' && this.c != '\0' && !inComment) || inQuotes)
                    {
                        // If we're in quotes and have reached the end of the file, record the
                        // rest of the record and field.
                        if (prevCharWasDelimiter)
                        {
                            // Handle an empty field at the end of the row.
                            field = string.Empty;
                        }

                        this.AddFieldToRecord(ref recordPosition, field, ref fieldIsBad);
                    }
                    else
                    {
                        this.RawRecord = null;
                        this.record = null;
                    }

                    ch = '\0';
                    return false;
                }
            }

            ch = this.readerBuffer[this.readerBufferPosition];
            return true;
        }

        /// <summary>
        ///     Reads the Excel seperator and sets it to the delimiter.
        /// </summary>
        protected virtual void ReadExcelSeparator()
        {
            // sep=delimiter
            var sepLine = this.reader.ReadLine();
            if (sepLine != null)
            {
                this.configuration.Delimiter = sepLine.Substring(4);
            }

            this.hasExcelSeparatorBeenRead = true;
        }

        /// <summary>
        ///     Gets the configuration.
        /// </summary>
        public virtual CsvConfiguration Configuration
        {
            get
            {
                return this.configuration;
            }
        }

        /// <summary>
        ///     Gets the field count.
        /// </summary>
        public virtual int FieldCount { get; protected set; }

        /// <summary>
        ///     Gets the character position that the parser is currently on.
        /// </summary>
        public virtual long CharPosition { get; protected set; }

        /// <summary>
        ///     Gets the byte position that the parser is currently on.
        /// </summary>
        public virtual long BytePosition { get; protected set; }

        /// <summary>
        ///     Gets the row of the CSV file that the parser is currently on.
        ///     This is the logical CSV row.
        /// </summary>
        public virtual int Row
        {
            get
            {
                return this.currentRow;
            }
        }

        /// <summary>
        ///     Gets the raw row for the current record that was parsed.
        /// </summary>
        public virtual string RawRecord { get; private set; }

        /// <summary>
        ///     Reads a record from the CSV file.
        /// </summary>
        /// <returns>
        ///     A <see cref="List{T}" /> of fields for the record read.
        ///     If there are no more records, null is returned.
        /// </returns>
        public virtual string[] Read()
        {
            this.CheckDisposed();

            try
            {
                if (this.configuration.HasExcelSeparator && !this.hasExcelSeparatorBeenRead)
                {
                    this.ReadExcelSeparator();
                }

                var row = this.ReadLine();

                if (this.configuration.DetectColumnCountChanges && row != null)
                {
                    if (this.FieldCount > 0 && (this.FieldCount != row.Length || row.Any(field => field == null)))
                    {
                        throw new CsvBadDataException("An inconsistent number of columns has been detected.");
                    }
                }

                return row;
            }
            catch (Exception ex)
            {
                ExceptionHelper.AddExceptionDataMessage(ex, this, null, null, null, null);
                throw;
            }
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
    }
}