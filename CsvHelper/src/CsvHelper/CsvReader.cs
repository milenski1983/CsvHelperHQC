// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com
#if NET_2_0
using CsvHelper.MissingFrom20;
#endif
#if !NET_2_0

#endif
#if !NET_2_0 && !NET_3_5 && !PCL

#endif

namespace CsvHelper
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using CsvHelper.Configuration;
    using CsvHelper.Interfaces;
    using CsvHelper.TypeConversion;

    /// <summary>
    ///     Reads data that was parsed from <see cref="ICsvParser" />.
    /// </summary>
    public class CsvReader : ICsvReader
    {
        private readonly CsvConfiguration configuration;

        private readonly Dictionary<string, List<int>> namedIndexes = new Dictionary<string, List<int>>();

#if !NET_2_0
        private readonly Dictionary<Type, Delegate> recordFuncs = new Dictionary<Type, Delegate>();
#endif

        private int currentIndex = -1;

        private string[] currentRecord;

        private bool disposed;

        private bool doneReading;

        private bool hasBeenRead;

        private string[] headerRecord;

        private ICsvParser parser;

        /// <summary>
        ///     Creates a new CSV reader using the given <see cref="TextReader" /> and
        ///     <see cref="CsvParser" /> as the default parser.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public CsvReader(TextReader reader)
            : this(reader, new CsvConfiguration())
        {
        }

        /// <summary>
        ///     Creates a new CSV reader using the given <see cref="TextReader" /> and
        ///     <see cref="CsvConfiguration" /> and <see cref="CsvParser" /> as the default parser.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="configuration">The configuration.</param>
        public CsvReader(TextReader reader, CsvConfiguration configuration)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.parser = new CsvParser(reader, configuration);
            this.configuration = configuration;
        }

        /// <summary>
        ///     Creates a new CSV reader using the given <see cref="ICsvParser" />.
        /// </summary>
        /// <param name="parser">The <see cref="ICsvParser" /> used to parse the CSV file.</param>
        public CsvReader(ICsvParser parser)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("parser");
            }

            if (parser.Configuration == null)
            {
                throw new CsvConfigurationException("The given parser has no configuration.");
            }

            this.parser = parser;
            this.configuration = parser.Configuration;
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
        ///     Gets the parser.
        /// </summary>
        public virtual ICsvParser Parser
        {
            get
            {
                return this.parser;
            }
        }

        /// <summary>
        ///     Gets the field headers.
        /// </summary>
        public virtual string[] FieldHeaders
        {
            get
            {
                this.CheckDisposed();
                this.CheckHasBeenRead();

                return this.headerRecord;
            }
        }

        /// <summary>
        ///     Get the current record;
        /// </summary>
        public virtual string[] CurrentRecord
        {
            get
            {
                this.CheckDisposed();
                this.CheckHasBeenRead();

                return this.currentRecord;
            }
        }

        /// <summary>
        ///     Gets the current row.
        /// </summary>
        public int Row
        {
            get
            {
                this.CheckDisposed();
                this.CheckHasBeenRead();

                return this.parser.Row;
            }
        }

        /// <summary>
        ///     Gets the raw field at position (column) index.
        /// </summary>
        /// <param name="index">The zero based index of the field.</param>
        /// <returns>The raw field.</returns>
        public virtual string this[int index]
        {
            get
            {
                this.CheckDisposed();
                this.CheckHasBeenRead();

                return this.GetField(index);
            }
        }

        /// <summary>
        ///     Gets the raw field at position (column) name.
        /// </summary>
        /// <param name="name">The named index of the field.</param>
        /// <returns>The raw field.</returns>
        public virtual string this[string name]
        {
            get
            {
                this.CheckDisposed();
                this.CheckHasBeenRead();

                return this.GetField(name);
            }
        }

        /// <summary>
        ///     Gets the raw field at position (column) name.
        /// </summary>
        /// <param name="name">The named index of the field.</param>
        /// <param name="index">The zero based index of the field.</param>
        /// <returns>The raw field.</returns>
        public virtual string this[string name, int index]
        {
            get
            {
                this.CheckDisposed();
                this.CheckHasBeenRead();

                return this.GetField(name, index);
            }
        }

        /// <summary>
        ///     Gets the field converted to <see cref="object" /> using
        ///     the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <param name="index">The index of the field.</param>
        /// <param name="converter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="object" />.</param>
        /// <returns>The field converted to <see cref="object" />.</returns>
        [Obsolete(
            "This method is deprecated and will be removed in the next major release. Use GetField( Type, int, ITypeConverter ) instead.", 
            false)]
        public virtual object GetField(int index, ITypeConverter converter)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var typeConverterOptions = new TypeConverterOptions { CultureInfo = this.configuration.CultureInfo };

            var field = this.GetField(index);
            return converter.ConvertFromString(typeConverterOptions, field);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="object" /> using
        ///     the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <param name="name">The named index of the field.</param>
        /// <param name="converter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="object" />.</param>
        /// <returns>The field converted to <see cref="object" />.</returns>
        [Obsolete(
            "This method is deprecated and will be removed in the next major release. Use GetField( Type, string, ITypeConverter ) instead.", 
            false)]
        public virtual object GetField(string name, ITypeConverter converter)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var index = this.GetFieldIndex(name);
            return this.GetField(index, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="object" /> using
        ///     the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <param name="name">The named index of the field.</param>
        /// <param name="index">The zero based index of the instance of the field.</param>
        /// <param name="converter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="object" />.</param>
        /// <returns>The field converted to <see cref="object" />.</returns>
        [Obsolete(
            "This method is deprecated and will be removed in the next major release. Use GetField( Type, string, int, ITypeConverter ) instead.", 
            false)]
        public virtual object GetField(string name, int index, ITypeConverter converter)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var fieldIndex = this.GetFieldIndex(name, index);
            return this.GetField(fieldIndex, converter);
        }

        /// <summary>
        ///     Gets the raw field at position (column) index.
        /// </summary>
        /// <param name="index">The zero based index of the field.</param>
        /// <returns>The raw field.</returns>
        public virtual string GetField(int index)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            // Set the current index being used so we
            // have more information if an error occurs
            // when reading records.
            this.currentIndex = index;

            if (index >= this.currentRecord.Length)
            {
                if (this.configuration.WillThrowOnMissingField)
                {
                    var ex = new CsvMissingFieldException(string.Format("Field at index '{0}' does not exist.", index));
                    ExceptionHelper.AddExceptionDataMessage(
                        ex,
                        this.Parser,
                        typeof(string),
                        this.namedIndexes,
                        index,
                        this.currentRecord);
                    throw ex;
                }

                return default(string);
            }

            var field = this.currentRecord[index];
            if (this.configuration.TrimFields && field != null)
            {
                field = field.Trim();
            }

            return field;
        }

        /// <summary>
        ///     Gets the raw field at position (column) name.
        /// </summary>
        /// <param name="name">The named index of the field.</param>
        /// <returns>The raw field.</returns>
        public virtual string GetField(string name)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var index = this.GetFieldIndex(name);
            if (index < 0)
            {
                return null;
            }

            return this.GetField(index);
        }

        /// <summary>
        ///     Gets the raw field at position (column) name and the index
        ///     instance of that field. The index is used when there are
        ///     multiple columns with the same header name.
        /// </summary>
        /// <param name="name">The named index of the field.</param>
        /// <param name="index">The zero based index of the instance of the field.</param>
        /// <returns>The raw field.</returns>
        public virtual string GetField(string name, int index)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var fieldIndex = this.GetFieldIndex(name, index);
            if (fieldIndex < 0)
            {
                return null;
            }

            return this.GetField(fieldIndex);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Object" /> using
        ///     the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <param name="type">The type of the field.</param>
        /// <param name="index">The index of the field.</param>
        /// <returns>The field converted to <see cref="Object" />.</returns>
        public virtual object GetField(Type type, int index)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = TypeConverterFactory.GetConverter(type);
            return this.GetField(type, index, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Object" /> using
        ///     the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <param name="type">The type of the field.</param>
        /// <param name="name">The named index of the field.</param>
        /// <returns>The field converted to <see cref="Object" />.</returns>
        public virtual object GetField(Type type, string name)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = TypeConverterFactory.GetConverter(type);
            return this.GetField(type, name, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Object" /> using
        ///     the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <param name="type">The type of the field.</param>
        /// <param name="name">The named index of the field.</param>
        /// <param name="index">The zero based index of the instance of the field.</param>
        /// <returns>The field converted to <see cref="Object" />.</returns>
        public virtual object GetField(Type type, string name, int index)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = TypeConverterFactory.GetConverter(type);
            return this.GetField(type, name, index, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Object" /> using
        ///     the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <param name="type">The type of the field.</param>
        /// <param name="index">The index of the field.</param>
        /// <param name="converter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Object" />.</param>
        /// <returns>The field converted to <see cref="Object" />.</returns>
        public virtual object GetField(Type type, int index, ITypeConverter converter)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var typeConverterOptions = TypeConverterOptionsFactory.GetOptions(type);
            if (typeConverterOptions.CultureInfo == null)
            {
                typeConverterOptions.CultureInfo = this.configuration.CultureInfo;
            }

            var field = this.GetField(index);
            return converter.ConvertFromString(typeConverterOptions, field);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Object" /> using
        ///     the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <param name="type">The type of the field.</param>
        /// <param name="name">The named index of the field.</param>
        /// <param name="converter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Object" />.</param>
        /// <returns>The field converted to <see cref="Object" />.</returns>
        public virtual object GetField(Type type, string name, ITypeConverter converter)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var index = this.GetFieldIndex(name);
            return this.GetField(type, index, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Object" /> using
        ///     the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <param name="type">The type of the field.</param>
        /// <param name="name">The named index of the field.</param>
        /// <param name="index">The zero based index of the instance of the field.</param>
        /// <param name="converter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Object" />.</param>
        /// <returns>The field converted to <see cref="Object" />.</returns>
        public virtual object GetField(Type type, string name, int index, ITypeConverter converter)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var fieldIndex = this.GetFieldIndex(name, index);
            return this.GetField(type, fieldIndex, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) index.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <param name="index">The zero based index of the field.</param>
        /// <returns>The field converted to <see cref="Type" /> T.</returns>
        public virtual T GetField<T>(int index)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = TypeConverterFactory.GetConverter<T>();
            return this.GetField<T>(index, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) name.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <param name="name">The named index of the field.</param>
        /// <returns>The field converted to <see cref="Type" /> T.</returns>
        public virtual T GetField<T>(string name)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = TypeConverterFactory.GetConverter<T>();
            return this.GetField<T>(name, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position
        ///     (column) name and the index instance of that field. The index
        ///     is used when there are multiple columns with the same header name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The named index of the field.</param>
        /// <param name="index">The zero based index of the instance of the field.</param>
        /// <returns></returns>
        public virtual T GetField<T>(string name, int index)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = TypeConverterFactory.GetConverter<T>();
            return this.GetField<T>(name, index, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) index using
        ///     the given <see cref="ITypeConverter" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <param name="index">The zero based index of the field.</param>
        /// <param name="converter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Type" /> T.</param>
        /// <returns>The field converted to <see cref="Type" /> T.</returns>
        public virtual T GetField<T>(int index, ITypeConverter converter)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            if (index >= this.currentRecord.Length || index < 0)
            {
                if (this.configuration.WillThrowOnMissingField)
                {
                    var ex = new CsvMissingFieldException(string.Format("Field at index '{0}' does not exist.", index));
                    ExceptionHelper.AddExceptionDataMessage(
                        ex,
                        this.Parser,
                        typeof(T),
                        this.namedIndexes,
                        index,
                        this.currentRecord);
                    throw ex;
                }

                return default(T);
            }

            return (T)this.GetField(typeof(T), index, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) name using
        ///     the given <see cref="ITypeConverter" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <param name="name">The named index of the field.</param>
        /// <param name="converter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Type" /> T.</param>
        /// <returns>The field converted to <see cref="Type" /> T.</returns>
        public virtual T GetField<T>(string name, ITypeConverter converter)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var index = this.GetFieldIndex(name);
            return this.GetField<T>(index, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position
        ///     (column) name and the index instance of that field. The index
        ///     is used when there are multiple columns with the same header name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The named index of the field.</param>
        /// <param name="index">The zero based index of the instance of the field.</param>
        /// <param name="converter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Type" /> T.</param>
        /// <returns>The field converted to <see cref="Type" /> T.</returns>
        public virtual T GetField<T>(string name, int index, ITypeConverter converter)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var fieldIndex = this.GetFieldIndex(name, index);
            return this.GetField<T>(fieldIndex, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) index using
        ///     the given <see cref="ITypeConverter" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <typeparam name="TConverter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Type" /> T.</typeparam>
        /// <param name="index">The zero based index of the field.</param>
        /// <returns>The field converted to <see cref="Type" /> T.</returns>
        public virtual T GetField<T, TConverter>(int index) where TConverter : ITypeConverter
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = ReflectionHelper.CreateInstance<TConverter>();
            return this.GetField<T>(index, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) name using
        ///     the given <see cref="ITypeConverter" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <typeparam name="TConverter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Type" /> T.</typeparam>
        /// <param name="name">The named index of the field.</param>
        /// <returns>The field converted to <see cref="Type" /> T.</returns>
        public virtual T GetField<T, TConverter>(string name) where TConverter : ITypeConverter
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = ReflectionHelper.CreateInstance<TConverter>();
            return this.GetField<T>(name, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position
        ///     (column) name and the index instance of that field. The index
        ///     is used when there are multiple columns with the same header name.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <typeparam name="TConverter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Type" /> T.</typeparam>
        /// <param name="name">The named index of the field.</param>
        /// <param name="index">The zero based index of the instance of the field.</param>
        /// <returns>The field converted to <see cref="Type" /> T.</returns>
        public virtual T GetField<T, TConverter>(string name, int index) where TConverter : ITypeConverter
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = ReflectionHelper.CreateInstance<TConverter>();
            return this.GetField<T>(name, index, converter);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) index.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <param name="index">The zero based index of the field.</param>
        /// <param name="field">The field converted to type T.</param>
        /// <returns>A value indicating if the get was successful.</returns>
        public virtual bool TryGetField<T>(int index, out T field)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = TypeConverterFactory.GetConverter<T>();
            return this.TryGetField(index, converter, out field);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) name.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <param name="name">The named index of the field.</param>
        /// <param name="field">The field converted to <see cref="Type" /> T.</param>
        /// <returns>A value indicating if the get was successful.</returns>
        public virtual bool TryGetField<T>(string name, out T field)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = TypeConverterFactory.GetConverter<T>();
            return this.TryGetField(name, converter, out field);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position
        ///     (column) name and the index instance of that field. The index
        ///     is used when there are multiple columns with the same header name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The named index of the field.</param>
        /// <param name="index">The zero based index of the instance of the field.</param>
        /// <param name="field">The field converted to <see cref="Type" /> T.</param>
        /// <returns>A value indicating if the get was successful.</returns>
        public virtual bool TryGetField<T>(string name, int index, out T field)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = TypeConverterFactory.GetConverter<T>();
            return this.TryGetField(name, index, converter, out field);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) index
        ///     using the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <param name="index">The zero based index of the field.</param>
        /// <param name="converter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Type" /> T.</param>
        /// <param name="field">The field converted to <see cref="Type" /> T.</param>
        /// <returns>A value indicating if the get was successful.</returns>
        public virtual bool TryGetField<T>(int index, ITypeConverter converter, out T field)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            // DateTimeConverter.ConvertFrom will successfully convert
            // a white space string to a DateTime.MinValue instead of
            // returning null, so we need to handle this special case.
            if (converter is DateTimeConverter)
            {
                if (StringHelper.IsNullOrWhiteSpace(this.currentRecord[index]))
                {
                    field = default(T);
                    return false;
                }
            }

            // TypeConverter.IsValid() just wraps a
            // ConvertFrom() call in a try/catch, so lets not
            // do it twice and just do it ourselves.
            try
            {
                field = this.GetField<T>(index, converter);
                return true;
            }
            catch
            {
                field = default(T);
                return false;
            }
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) name
        ///     using the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <param name="name">The named index of the field.</param>
        /// <param name="converter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Type" /> T.</param>
        /// <param name="field">The field converted to <see cref="Type" /> T.</param>
        /// <returns>A value indicating if the get was successful.</returns>
        public virtual bool TryGetField<T>(string name, ITypeConverter converter, out T field)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var index = this.GetFieldIndex(name, isTryGet: true);
            if (index == -1)
            {
                field = default(T);
                return false;
            }

            return this.TryGetField(index, converter, out field);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) name
        ///     using the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <param name="name">The named index of the field.</param>
        /// <param name="index">The zero based index of the instance of the field.</param>
        /// <param name="converter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Type" /> T.</param>
        /// <param name="field">The field converted to <see cref="Type" /> T.</param>
        /// <returns>A value indicating if the get was successful.</returns>
        public virtual bool TryGetField<T>(string name, int index, ITypeConverter converter, out T field)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var fieldIndex = this.GetFieldIndex(name, index, true);
            if (fieldIndex == -1)
            {
                field = default(T);
                return false;
            }

            return this.TryGetField(fieldIndex, converter, out field);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) index
        ///     using the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <typeparam name="TConverter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Type" /> T.</typeparam>
        /// <param name="index">The zero based index of the field.</param>
        /// <param name="field">The field converted to <see cref="Type" /> T.</param>
        /// <returns>A value indicating if the get was successful.</returns>
        public virtual bool TryGetField<T, TConverter>(int index, out T field) where TConverter : ITypeConverter
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = ReflectionHelper.CreateInstance<TConverter>();
            return this.TryGetField(index, converter, out field);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) name
        ///     using the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <typeparam name="TConverter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Type" /> T.</typeparam>
        /// <param name="name">The named index of the field.</param>
        /// <param name="field">The field converted to <see cref="Type" /> T.</param>
        /// <returns>A value indicating if the get was successful.</returns>
        public virtual bool TryGetField<T, TConverter>(string name, out T field) where TConverter : ITypeConverter
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = ReflectionHelper.CreateInstance<TConverter>();
            return this.TryGetField(name, converter, out field);
        }

        /// <summary>
        ///     Gets the field converted to <see cref="Type" /> T at position (column) name
        ///     using the specified <see cref="ITypeConverter" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the field.</typeparam>
        /// <typeparam name="TConverter">The <see cref="ITypeConverter" /> used to convert the field to <see cref="Type" /> T.</typeparam>
        /// <param name="name">The named index of the field.</param>
        /// <param name="index">The zero based index of the instance of the field.</param>
        /// <param name="field">The field converted to <see cref="Type" /> T.</param>
        /// <returns>A value indicating if the get was successful.</returns>
        public virtual bool TryGetField<T, TConverter>(string name, int index, out T field)
            where TConverter : ITypeConverter
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            var converter = ReflectionHelper.CreateInstance<TConverter>();
            return this.TryGetField(name, index, converter, out field);
        }

        /// <summary>
        ///     Determines whether the current record is empty.
        ///     A record is considered empty if all fields are empty.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if [is record empty]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsRecordEmpty()
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            return this.IsRecordEmpty(true);
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

#if !NET_2_0
        /// <summary>
        ///     Gets the record converted into <see cref="Type" /> T.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the record.</typeparam>
        /// <returns>The record converted to <see cref="Type" /> T.</returns>
        public virtual T GetRecord<T>()
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            T record;
            try
            {
                record = this.CreateRecord<T>();
            }
            catch (Exception ex)
            {
                ExceptionHelper.AddExceptionDataMessage(
                    ex,
                    this.parser,
                    typeof(T),
                    this.namedIndexes,
                    this.currentIndex,
                    this.currentRecord);
                throw;
            }

            return record;
        }

        /// <summary>
        ///     Gets the record.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> of the record.</param>
        /// <returns>The record.</returns>
        public virtual object GetRecord(Type type)
        {
            this.CheckDisposed();
            this.CheckHasBeenRead();

            object record;
            try
            {
                record = this.CreateRecord(type);
            }
            catch (Exception ex)
            {
                ExceptionHelper.AddExceptionDataMessage(
                    ex,
                    this.parser,
                    type,
                    this.namedIndexes,
                    this.currentIndex,
                    this.currentRecord);
                throw;
            }

            return record;
        }

        /// <summary>
        ///     Gets all the records in the CSV file and
        ///     converts each to <see cref="Type" /> T. The Read method
        ///     should not be used when using this.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> of the record.</typeparam>
        /// <returns>An <see cref="IList{T}" /> of records.</returns>
        public virtual IEnumerable<T> GetRecords<T>()
        {
            this.CheckDisposed();

            // Don't need to check if it's been read
            // since we're doing the reading ourselves.
            while (this.Read())
            {
                T record;
                try
                {
                    record = this.CreateRecord<T>();
                }
                catch (Exception ex)
                {
                    ExceptionHelper.AddExceptionDataMessage(
                        ex,
                        this.parser,
                        typeof(T),
                        this.namedIndexes,
                        this.currentIndex,
                        this.currentRecord);

                    if (this.configuration.IgnoreReadingExceptions)
                    {
#if !NET_2_0
                        if (this.configuration.ReadingExceptionCallback != null)
                        {
                            this.configuration.ReadingExceptionCallback(ex, this);
                        }

#endif

                        continue;
                    }

                    throw;
                }

                yield return record;
            }
        }

        /// <summary>
        ///     Gets all the records in the CSV file and
        ///     converts each to <see cref="Type" /> T. The Read method
        ///     should not be used when using this.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> of the record.</param>
        /// <returns>An <see cref="IList{Object}" /> of records.</returns>
        public virtual IEnumerable<object> GetRecords(Type type)
        {
            this.CheckDisposed();

            // Don't need to check if it's been read
            // since we're doing the reading ourselves.
            while (this.Read())
            {
                object record;
                try
                {
                    record = this.CreateRecord(type);
                }
                catch (Exception ex)
                {
                    ExceptionHelper.AddExceptionDataMessage(
                        ex,
                        this.parser,
                        type,
                        this.namedIndexes,
                        this.currentIndex,
                        this.currentRecord);

                    if (this.configuration.IgnoreReadingExceptions)
                    {
#if !NET_2_0
                        if (this.configuration.ReadingExceptionCallback != null)
                        {
                            this.configuration.ReadingExceptionCallback(ex, this);
                        }

#endif

                        continue;
                    }

                    throw;
                }

                yield return record;
            }
        }

        /// <summary>
        ///     Clears the record cache for the given type. After <see cref="ICsvReaderRow.GetRecord{T}" /> is called the
        ///     first time, code is dynamically generated based on the <see cref="CsvPropertyMapCollection" />,
        ///     compiled, and stored for the given type T. If the <see cref="CsvPropertyMapCollection" />
        ///     changes, <see cref="ICsvReaderRow.ClearRecordCache{T}" /> needs to be called to update the
        ///     record cache.
        /// </summary>
        public virtual void ClearRecordCache<T>()
        {
            this.CheckDisposed();

            this.ClearRecordCache(typeof(T));
        }

        /// <summary>
        ///     Clears the record cache for the given type. After <see cref="ICsvReaderRow.GetRecord{T}" /> is called the
        ///     first time, code is dynamically generated based on the <see cref="CsvPropertyMapCollection" />,
        ///     compiled, and stored for the given type T. If the <see cref="CsvPropertyMapCollection" />
        ///     changes, <see cref="ICsvReaderRow.ClearRecordCache(System.Type)" /> needs to be called to update the
        ///     record cache.
        /// </summary>
        /// <param name="type">The type to invalidate.</param>
        public virtual void ClearRecordCache(Type type)
        {
            this.CheckDisposed();

            this.recordFuncs.Remove(type);
        }

        /// <summary>
        ///     Clears the record cache for all types. After <see cref="ICsvReaderRow.GetRecord{T}" /> is called the
        ///     first time, code is dynamically generated based on the <see cref="CsvPropertyMapCollection" />,
        ///     compiled, and stored for the given type T. If the <see cref="CsvPropertyMapCollection" />
        ///     changes, <see cref="ICsvReaderRow.ClearRecordCache()" /> needs to be called to update the
        ///     record cache.
        /// </summary>
        public virtual void ClearRecordCache()
        {
            this.CheckDisposed();

            this.recordFuncs.Clear();
        }

#endif

        /// <summary>
        ///     Advances the reader to the next record.
        ///     If HasHeaderRecord is true (true by default), the first record of
        ///     the CSV file will be automatically read in as the header record
        ///     and the second record will be returned.
        /// </summary>
        /// <returns>True if there are more records, otherwise false.</returns>
        public virtual bool Read()
        {
            this.CheckDisposed();

            if (this.doneReading)
            {
                const string Message =
                    "The reader has already exhausted all records. "
                    + "If you would like to iterate the records more than "
                    + "once, store the records in memory. i.e. Use " + "CsvReader.GetRecords<T>().ToList()";
                throw new CsvReaderException(Message);
            }

            if (this.configuration.HasHeaderRecord && this.headerRecord == null)
            {
                this.headerRecord = this.parser.Read();
                this.ParseNamedIndexes();
            }

            do
            {
                this.currentRecord = this.parser.Read();
            }
            while (this.ShouldSkipRecord());

            this.currentIndex = -1;
            this.hasBeenRead = true;

            if (this.currentRecord == null)
            {
                this.doneReading = true;
            }

            return this.currentRecord != null;
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
                if (this.parser != null)
                {
                    this.parser.Dispose();
                }
            }

            this.disposed = true;
            this.parser = null;
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
        ///     Checks if the reader has been read yet.
        /// </summary>
        /// <exception cref="CsvReaderException" />
        protected virtual void CheckHasBeenRead()
        {
            if (!this.hasBeenRead)
            {
                throw new CsvReaderException("You must call read on the reader before accessing its data.");
            }
        }

        /// <summary>
        ///     Determines whether the current record is empty.
        ///     A record is considered empty if all fields are empty.
        /// </summary>
        /// <param name="checkHasBeenRead">
        ///     True to check if the record
        ///     has been read, otherwise false.
        /// </param>
        /// <returns>
        ///     <c>true</c> if [is record empty]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsRecordEmpty(bool checkHasBeenRead)
        {
            this.CheckDisposed();
            if (checkHasBeenRead)
            {
                this.CheckHasBeenRead();
            }

            if (this.currentRecord == null)
            {
                return false;
            }

            return this.currentRecord.All(this.GetEmtpyStringMethod());
        }

        /// <summary>
        ///     Gets a function to test for an empty string.
        ///     Will check <see cref="CsvConfiguration.TrimFields" /> when making its decision.
        /// </summary>
        /// <returns>The function to test for an empty string.</returns>
        protected virtual Func<string, bool> GetEmtpyStringMethod()
        {
            if (!this.Configuration.TrimFields)
            {
                return string.IsNullOrEmpty;
            }

#if NET_2_0 || NET_3_5
			return StringHelper.IsNullOrWhiteSpace;
#else
            return string.IsNullOrWhiteSpace;
#endif
        }

        /// <summary>
        ///     Gets the index of the field at name if found.
        /// </summary>
        /// <param name="name">The name of the field to get the index for.</param>
        /// <param name="index">The index of the field if there are multiple fields with the same name.</param>
        /// <param name="isTryGet">A value indicating if the call was initiated from a TryGet.</param>
        /// <returns>The index of the field if found, otherwise -1.</returns>
        /// <exception cref="CsvReaderException">Thrown if there is no header record.</exception>
        /// <exception cref="CsvMissingFieldException">Thrown if there isn't a field with name.</exception>
        protected virtual int GetFieldIndex(string name, int index = 0, bool isTryGet = false)
        {
            return this.GetFieldIndex(new[] { name }, index, isTryGet);
        }

        /// <summary>
        ///     Gets the index of the field at name if found.
        /// </summary>
        /// <param name="names">The possible names of the field to get the index for.</param>
        /// <param name="index">The index of the field if there are multiple fields with the same name.</param>
        /// <param name="isTryGet">A value indicating if the call was initiated from a TryGet.</param>
        /// <returns>The index of the field if found, otherwise -1.</returns>
        /// <exception cref="CsvReaderException">Thrown if there is no header record.</exception>
        /// <exception cref="CsvMissingFieldException">Thrown if there isn't a field with name.</exception>
        protected virtual int GetFieldIndex(string[] names, int index = 0, bool isTryGet = false)
        {
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }

            if (!this.configuration.HasHeaderRecord)
            {
                throw new CsvReaderException("There is no header record to determine the index by name.");
            }

            var compareOptions = !this.Configuration.IsHeaderCaseSensitive
                                     ? CompareOptions.IgnoreCase
                                     : CompareOptions.None;
            string name = null;
            foreach (var pair in this.namedIndexes)
            {
                var namedIndex = pair.Key;
                if (this.configuration.IgnoreHeaderWhiteSpace)
                {
                    namedIndex = Regex.Replace(namedIndex, "\\s", string.Empty);
                }
                else if (this.configuration.TrimHeaders && namedIndex != null)
                {
                    namedIndex = namedIndex.Trim();
                }

                foreach (var n in names)
                {
                    if (this.Configuration.CultureInfo.CompareInfo.Compare(namedIndex, n, compareOptions) == 0)
                    {
                        name = pair.Key;
                    }
                }
            }

            if (name == null)
            {
                if (this.configuration.WillThrowOnMissingField && !isTryGet)
                {
                    // If we're in strict reading mode and the
                    // named index isn't found, throw an exception.
                    var namesJoined = string.Format("'{0}'", string.Join("', '", names));
                    var ex =
                        new CsvMissingFieldException(
                            string.Format("Fields {0} do not exist in the CSV file.", namesJoined));
                    ExceptionHelper.AddExceptionDataMessage(
                        ex, 
                        this.Parser, 
                        null, 
                        this.namedIndexes, 
                        this.currentIndex, 
                        this.currentRecord);
                    throw ex;
                }

                return -1;
            }

            return this.namedIndexes[name][index];
        }

        /// <summary>
        ///     Parses the named indexes from the header record.
        /// </summary>
        protected virtual void ParseNamedIndexes()
        {
            if (this.headerRecord == null)
            {
                throw new CsvReaderException("No header record was found.");
            }

            for (var i = 0; i < this.headerRecord.Length; i++)
            {
                var name = this.headerRecord[i];
                if (!this.Configuration.IsHeaderCaseSensitive)
                {
                    name = name.ToLower();
                }

                if (this.namedIndexes.ContainsKey(name))
                {
                    this.namedIndexes[name].Add(i);
                }
                else
                {
                    this.namedIndexes[name] = new List<int> { i };
                }
            }
        }

        /// <summary>
        ///     Checks if the current record should be skipped or not.
        /// </summary>
        /// <returns><c>true</c> if the current record should be skipped, <c>false</c> otherwise.</returns>
        protected virtual bool ShouldSkipRecord()
        {
            this.CheckDisposed();

            if (this.currentRecord == null)
            {
                return false;
            }

            return this.configuration.ShouldSkipRecord != null
                       ? this.configuration.ShouldSkipRecord(this.currentRecord)
                       : this.configuration.SkipEmptyRecords && this.IsRecordEmpty(false);
        }

#if !NET_2_0 && !NET_3_5 && !PCL
        /// <summary>
        ///     Creates a dynamic object from the current record.
        /// </summary>
        /// <returns>The dynamic object.</returns>
        protected virtual dynamic CreateDynamic()
        {
            var obj = new ExpandoObject();
            var dict = obj as IDictionary<string, object>;
            if (this.headerRecord != null)
            {
                for (var i = 0; i < this.headerRecord.Length; i++)
                {
                    var header = this.headerRecord[i];
                    var field = this.currentRecord[i];
                    dict.Add(header, field);
                }
            }
            else
            {
                for (var i = 0; i < this.currentRecord.Length; i++)
                {
                    var propertyName = "Field" + (i + 1);
                    var field = this.currentRecord[i];
                    dict.Add(propertyName, field);
                }
            }

            return obj;
        }

#endif
        
#if !NET_2_0
        /// <summary>
        ///     Creates the record for the given type.
        /// </summary>
        /// <typeparam name="T">The type of record to create.</typeparam>
        /// <returns>The created record.</returns>
        protected virtual T CreateRecord<T>()
        {
#if !NET_3_5 && !PCL

            // If the type is an object, a dynamic
            // object will be created. That is the
            // only way we can dynamically add properties
            // to a type of object.
            if (typeof(T) == typeof(object))
            {
                return this.CreateDynamic();
            }

#endif

            return this.GetReadRecordFunc<T>()();
        }

        /// <summary>
        ///     Creates the record for the given type.
        /// </summary>
        /// <param name="type">The type of record to create.</param>
        /// <returns>The created record.</returns>
        protected virtual object CreateRecord(Type type)
        {
#if !NET_3_5 && !PCL

            // If the type is an object, a dynamic
            // object will be created. That is the
            // only way we can dynamically add properties
            // to a type of object.
            if (type == typeof(object))
            {
                return this.CreateDynamic();
            }

#endif

            try
            {
                return this.GetReadRecordFunc(type).DynamicInvoke();
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>
        ///     Gets the function delegate used to populate
        ///     a custom class object with data from the reader.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type" /> of object that is created
        ///     and populated.
        /// </typeparam>
        /// <returns>The function delegate.</returns>
        protected virtual Func<T> GetReadRecordFunc<T>()
        {
            var recordType = typeof(T);
            this.CreateReadRecordFunc(recordType);

            return (Func<T>)this.recordFuncs[recordType];
        }

        /// <summary>
        ///     Gets the function delegate used to populate
        ///     a custom class object with data from the reader.
        /// </summary>
        /// <param name="recordType">
        ///     The <see cref="Type" /> of object that is created
        ///     and populated.
        /// </param>
        /// <returns>The function delegate.</returns>
        protected virtual Delegate GetReadRecordFunc(Type recordType)
        {
            this.CreateReadRecordFunc(recordType);

            return this.recordFuncs[recordType];
        }

        /// <summary>
        ///     Creates the read record func for the given type if it
        ///     doesn't already exist.
        /// </summary>
        /// <param name="recordType">Type of the record.</param>
        protected virtual void CreateReadRecordFunc(Type recordType)
        {
            if (this.recordFuncs.ContainsKey(recordType))
            {
                return;
            }

            if (this.configuration.Maps[recordType] == null)
            {
                this.configuration.Maps.Add(this.configuration.AutoMap(recordType));
            }

            if (recordType.IsPrimitive)
            {
                this.CreateFuncForPrimitive(recordType);
            }
            else
            {
                this.CreateFuncForObject(recordType);
            }
        }

        /// <summary>
        ///     Creates the function for an object.
        /// </summary>
        /// <param name="recordType">The type of object to create the function for.</param>
        protected virtual void CreateFuncForObject(Type recordType)
        {
            var bindings = new List<MemberBinding>();

            this.CreatePropertyBindingsForMapping(this.configuration.Maps[recordType], recordType, bindings);

            if (bindings.Count == 0)
            {
                throw new CsvReaderException(
                    string.Format("No properties are mapped for type '{0}'.", recordType.FullName));
            }

            var constructorExpression = this.configuration.Maps[recordType].Constructor ?? Expression.New(recordType);
            var body = Expression.MemberInit(constructorExpression, bindings);
            var funcType = typeof(Func<>).MakeGenericType(recordType);
            this.recordFuncs[recordType] = Expression.Lambda(funcType, body).Compile();
        }

        /// <summary>
        ///     Creates the function for a primitive.
        /// </summary>
        /// <param name="recordType">The type of the primitive to create the function for.</param>
        protected virtual void CreateFuncForPrimitive(Type recordType)
        {
            var method = typeof(ICsvReaderRow).GetProperty("Item", typeof(string), new[] { typeof(int) }).GetGetMethod();
            Expression fieldExpression = Expression.Call(
                Expression.Constant(this), 
                method, 
                Expression.Constant(0, typeof(int)));

            var typeConverter = TypeConverterFactory.GetConverter(recordType);
            var typeConverterOptions = TypeConverterOptionsFactory.GetOptions(recordType);
            if (typeConverterOptions.CultureInfo == null)
            {
                typeConverterOptions.CultureInfo = this.configuration.CultureInfo;
            }

            fieldExpression = Expression.Call(
                Expression.Constant(typeConverter), 
                "ConvertFromString", 
                null, 
                Expression.Constant(typeConverterOptions), 
                fieldExpression);
            fieldExpression = Expression.Convert(fieldExpression, recordType);

            var funcType = typeof(Func<>).MakeGenericType(recordType);
            this.recordFuncs[recordType] = Expression.Lambda(funcType, fieldExpression).Compile();
        }

        /// <summary>
        ///     Creates the property bindings for the given <see cref="CsvClassMap" />.
        /// </summary>
        /// <param name="mapping">The mapping to create the bindings for.</param>
        /// <param name="recordType">The type of record.</param>
        /// <param name="bindings">The bindings that will be added to from the mapping.</param>
        protected virtual void CreatePropertyBindingsForMapping(
            CsvClassMap mapping, 
            Type recordType, 
            List<MemberBinding> bindings)
        {
            this.AddPropertyBindings(mapping.PropertyMaps, bindings);

            foreach (var referenceMap in mapping.ReferenceMaps)
            {
                if (!this.CanRead(referenceMap))
                {
                    continue;
                }

                var referenceBindings = new List<MemberBinding>();
                this.CreatePropertyBindingsForMapping(
                    referenceMap.Data.Mapping, 
                    referenceMap.Data.Property.PropertyType, 
                    referenceBindings);
                var referenceBody = Expression.MemberInit(
                    Expression.New(referenceMap.Data.Property.PropertyType), 
                    referenceBindings);
                bindings.Add(Expression.Bind(referenceMap.Data.Property, referenceBody));
            }
        }

        /// <summary>
        ///     Adds a <see cref="MemberBinding" /> for each property for it's field.
        /// </summary>
        /// <param name="properties">The properties to add bindings for.</param>
        /// <param name="bindings">The bindings that will be added to from the properties.</param>
        protected virtual void AddPropertyBindings(CsvPropertyMapCollection properties, List<MemberBinding> bindings)
        {
            foreach (var propertyMap in properties)
            {
                if (propertyMap.Data.ConvertExpression != null)
                {
                    // The user is providing the expression to do the conversion.
                    Expression exp = Expression.Invoke(propertyMap.Data.ConvertExpression, Expression.Constant(this));
                    exp = Expression.Convert(exp, propertyMap.Data.Property.PropertyType);
                    bindings.Add(Expression.Bind(propertyMap.Data.Property, exp));
                    continue;
                }

                if (!this.CanRead(propertyMap))
                {
                    continue;
                }

                if (propertyMap.Data.TypeConverter == null
                    || !propertyMap.Data.TypeConverter.CanConvertFrom(typeof(string)))
                {
                    // Skip if the type isn't convertible.
                    continue;
                }

                var index = -1;
                if (propertyMap.Data.IsNameSet)
                {
                    // If a name was explicitly set, use it.
                    index = this.GetFieldIndex(propertyMap.Data.Names.ToArray(), propertyMap.Data.NameIndex);
                    if (index == -1)
                    {
                        // Skip if the index was not found.
                        continue;
                    }
                }
                else if (propertyMap.Data.IsIndexSet)
                {
                    // If an index was explicity set, use it.
                    index = propertyMap.Data.Index;
                }
                else
                {
                    // Fallback to defaults.
                    if (this.configuration.HasHeaderRecord)
                    {
                        // Fallback to the default name.
                        index = this.GetFieldIndex(propertyMap.Data.Names.ToArray(), propertyMap.Data.NameIndex);
                        if (index == -1)
                        {
                            // Skip if the index was not found.
                            continue;
                        }
                    }
                    else if (index == -1)
                    {
                        // Fallback to the default index.
                        index = propertyMap.Data.Index;
                    }
                }

                // Get the field using the field index.
                var method =
                    typeof(ICsvReaderRow).GetProperty("Item", typeof(string), new[] { typeof(int) }).GetGetMethod();
                Expression fieldExpression = Expression.Call(
                    Expression.Constant(this), 
                    method, 
                    Expression.Constant(index, typeof(int)));

                // Convert the field.
                var typeConverterExpression = Expression.Constant(propertyMap.Data.TypeConverter);
                if (propertyMap.Data.TypeConverterOptions.CultureInfo == null)
                {
                    propertyMap.Data.TypeConverterOptions.CultureInfo = this.configuration.CultureInfo;
                }

                var typeConverterOptions =
                    TypeConverterOptions.Merge(
                        TypeConverterOptionsFactory.GetOptions(propertyMap.Data.Property.PropertyType), 
                        propertyMap.Data.TypeConverterOptions);
                var typeConverterOptionsExpression = Expression.Constant(typeConverterOptions);

                // Create type converter expression.
                Expression typeConverterFieldExpression = Expression.Call(
                    typeConverterExpression, 
                    "ConvertFromString", 
                    null, 
                    typeConverterOptionsExpression, 
                    fieldExpression);
                typeConverterFieldExpression = Expression.Convert(
                    typeConverterFieldExpression, 
                    propertyMap.Data.Property.PropertyType);

                if (propertyMap.Data.IsDefaultSet)
                {
                    // Create default value expression.
                    Expression defaultValueExpression = Expression.Convert(
                        Expression.Constant(propertyMap.Data.Default), 
                        propertyMap.Data.Property.PropertyType);

                    // If null, use string.Empty.
                    var coalesceExpression = Expression.Coalesce(fieldExpression, Expression.Constant(string.Empty));

                    // Check if the field is an empty string.
                    var checkFieldEmptyExpression =
                        Expression.Equal(
                            Expression.Convert(coalesceExpression, typeof(string)), 
                            Expression.Constant(string.Empty, typeof(string)));

                    // Use a default value if the field is an empty string.
                    fieldExpression = Expression.Condition(
                        checkFieldEmptyExpression, 
                        defaultValueExpression, 
                        typeConverterFieldExpression);
                }
                else
                {
                    fieldExpression = typeConverterFieldExpression;
                }

                bindings.Add(Expression.Bind(propertyMap.Data.Property, fieldExpression));
            }
        }

        /// <summary>
        ///     Determines if the property for the <see cref="CsvPropertyMap" />
        ///     can be read.
        /// </summary>
        /// <param name="propertyMap">The property map.</param>
        /// <returns>A value indicating of the property can be read. True if it can, otherwise false.</returns>
        protected virtual bool CanRead(CsvPropertyMap propertyMap)
        {
            var cantRead =

                // Ignored properties.
                (propertyMap.Data.Ignore ||

                // Properties that don't have a public setter
                // and we are honoring the accessor modifier.
                propertyMap.Data.Property.GetSetMethod() == null) && (!this.configuration.IgnorePrivateAccessor ||

                // Properties that don't have a setter at all.
                propertyMap.Data.Property.GetSetMethod(true) == null);
            return !cantRead;
        }

        /// <summary>
        ///     Determines if the property for the <see cref="CsvPropertyReferenceMap" />
        ///     can be read.
        /// </summary>
        /// <param name="propertyReferenceMap">The reference map.</param>
        /// <returns>A value indicating of the property can be read. True if it can, otherwise false.</returns>
        protected virtual bool CanRead(CsvPropertyReferenceMap propertyReferenceMap)
        {
            var cantRead =

                // Properties that don't have a public setter
                // and we are honoring the accessor modifier.
                propertyReferenceMap.Data.Property.GetSetMethod() == null && (!this.configuration.IgnorePrivateAccessor
                ||

                // Properties that don't have a setter at all.
                propertyReferenceMap.Data.Property.GetSetMethod(true) == null);
            return !cantRead;
        }

#endif
    }
}