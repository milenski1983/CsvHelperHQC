// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com
#if !NET_2_0
using System.Linq;
using System.Linq.Expressions;

#endif

#if NET_2_0
using CsvHelper.MissingFrom20;
#endif

#if PCL
using CsvHelper.MissingFromPcl;
#endif

namespace CsvHelper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;

    /// <summary>
    ///     Used to write CSV files.
    /// </summary>
    public class CsvWriter : ICsvWriter
    {
        /// <summary>
        ///     Creates a new CSV writer using the given <see cref="TextWriter" />,
        ///     a default <see cref="CsvConfiguration" /> and <see cref="CsvSerializer" />
        ///     as the default serializer.
        /// </summary>
        /// <param name="writer">The writer used to write the CSV file.</param>
        public CsvWriter(TextWriter writer)
            : this(writer, new CsvConfiguration())
        {
        }

        /// <summary>
        ///     Creates a new CSV writer using the given <see cref="TextWriter" />
        ///     and <see cref="CsvConfiguration" /> and <see cref="CsvSerializer" />
        ///     as the default serializer.
        /// </summary>
        /// <param name="writer">The <see cref="StreamWriter" /> use to write the CSV file.</param>
        /// <param name="configuration">The configuration.</param>
        public CsvWriter(TextWriter writer, CsvConfiguration configuration)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.configuration = configuration;
            this.serializer = new CsvSerializer(writer, configuration);
        }

        /// <summary>
        ///     Creates a new CSV writer using the given <see cref="ICsvSerializer" />.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        public CsvWriter(ICsvSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            if (serializer.Configuration == null)
            {
                throw new CsvConfigurationException("The given serializer has no configuration.");
            }

            this.serializer = serializer;
            this.configuration = serializer.Configuration;
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
                if (this.serializer != null)
                {
                    this.serializer.Dispose();
                }
            }

            this.disposed = true;
            this.serializer = null;
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
        ///     Writes the field to the CSV file. The field
        ///     may get quotes added to it.
        ///     When all fields are written for a record,
        ///     <see cref="ICsvWriter.NextRecord" /> must be called
        ///     to complete writing of the current record.
        /// </summary>
        /// <param name="field">The field to write.</param>
        public virtual void WriteField(string field)
        {
            this.CheckDisposed();

            var shouldQuote = this.configuration.QuoteAllFields;

            if (this.configuration.TrimFields)
            {
                field = field.Trim();
            }

            if (!this.configuration.QuoteNoFields && !string.IsNullOrEmpty(field))
            {
                var hasQuote = field.Contains(this.configuration.QuoteString);

                if (shouldQuote || hasQuote || field[0] == ' ' || field[field.Length - 1] == ' '
                    || field.IndexOfAny(this.configuration.QuoteRequiredChars) > -1
                    || (this.configuration.Delimiter.Length > 1 && field.Contains(this.configuration.Delimiter)))
                {
                    shouldQuote = true;
                }
            }

            this.WriteField(field, shouldQuote);
        }

        /// <summary>
        ///     Writes the field to the CSV file. This will
        ///     ignore any need to quote and ignore the
        ///     <see cref="CsvConfiguration.QuoteAllFields" />
        ///     and just quote based on the shouldQuote
        ///     parameter.
        ///     When all fields are written for a record,
        ///     <see cref="ICsvWriter.NextRecord" /> must be called
        ///     to complete writing of the current record.
        /// </summary>
        /// <param name="field">The field to write.</param>
        /// <param name="shouldQuote">True to quote the field, otherwise false.</param>
        public virtual void WriteField(string field, bool shouldQuote)
        {
            this.CheckDisposed();

            // All quotes must be doubled.       
            if (shouldQuote && !string.IsNullOrEmpty(field))
            {
                field = field.Replace(this.configuration.QuoteString, this.configuration.DoubleQuoteString);
            }

            if (this.configuration.UseExcelLeadingZerosFormatForNumerics && !string.IsNullOrEmpty(field)
                && field[0] == '0' && field.All(char.IsDigit))
            {
                field = "=" + this.configuration.Quote + field + this.configuration.Quote;
            }
            else if (shouldQuote)
            {
                field = this.configuration.Quote + field + this.configuration.Quote;
            }

            this.currentRecord.Add(field);
        }

        /// <summary>
        ///     Writes the field to the CSV file.
        ///     When all fields are written for a record,
        ///     <see cref="ICsvWriter.NextRecord" /> must be called
        ///     to complete writing of the current record.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="field">The field to write.</param>
        public virtual void WriteField<T>(T field)
        {
            this.CheckDisposed();

            var type = field.GetType();
            if (type == typeof(string))
            {
                this.WriteField(field as string);
            }
            else
            {
                var converter = TypeConverterFactory.GetConverter(type);
                this.WriteField(field, converter);
            }
        }

        /// <summary>
        ///     Writes the field to the CSV file.
        ///     When all fields are written for a record,
        ///     <see cref="ICsvWriter.NextRecord" /> must be called
        ///     to complete writing of the current record.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="field">The field to write.</param>
        /// <param name="converter">The converter used to convert the field into a string.</param>
        public virtual void WriteField<T>(T field, ITypeConverter converter)
        {
            this.CheckDisposed();

            var typeConverterOptions = TypeConverterOptionsFactory.GetOptions(field.GetType());
            if (typeConverterOptions.CultureInfo == null)
            {
                typeConverterOptions.CultureInfo = this.configuration.CultureInfo;
            }

            var fieldString = converter.ConvertToString(typeConverterOptions, field);
            this.WriteField(fieldString);
        }

        /// <summary>
        ///     Writes the field to the CSV file
        ///     using the given <see cref="ITypeConverter" />.
        ///     When all fields are written for a record,
        ///     <see cref="ICsvWriter.NextRecord" /> must be called
        ///     to complete writing of the current record.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <typeparam name="TConverter">The type of the converter.</typeparam>
        /// <param name="field">The field to write.</param>
        public virtual void WriteField<T, TConverter>(T field)
        {
            this.CheckDisposed();

            var converter = TypeConverterFactory.GetConverter<TConverter>();
            this.WriteField(field, converter);
        }

        /// <summary>
        ///     Writes the field to the CSV file.
        ///     When all fields are written for a record,
        ///     <see cref="ICsvWriter.NextRecord" /> must be called
        ///     to complete writing of the current record.
        /// </summary>
        /// <param name="type">The type of the field.</param>
        /// <param name="field">The field to write.</param>
        [Obsolete(
            "This method is deprecated and will be removed in the next major release. Use WriteField<T>( T field ) instead.", 
            false)]
        public virtual void WriteField(Type type, object field)
        {
            this.CheckDisposed();

            if (type == typeof(string))
            {
                this.WriteField(field as string);
            }
            else
            {
                var converter = TypeConverterFactory.GetConverter(type);
                this.WriteField(type, field, converter);
            }
        }

        /// <summary>
        ///     Writes the field to the CSV file.
        ///     When all fields are written for a record,
        ///     <see cref="ICsvWriter.NextRecord" /> must be called
        ///     to complete writing of the current record.
        /// </summary>
        /// <param name="type">The type of the field.</param>
        /// <param name="field">The field to write.</param>
        /// <param name="converter">The converter used to convert the field into a string.</param>
        [Obsolete(
            "This method is deprecated and will be removed in the next major release. Use WriteField<T>( T field, ITypeConverter converter ) instead.", 
            false)]
        public virtual void WriteField(Type type, object field, ITypeConverter converter)
        {
            this.CheckDisposed();

            var typeConverterOptions = TypeConverterOptionsFactory.GetOptions(type);
            if (typeConverterOptions.CultureInfo == null)
            {
                typeConverterOptions.CultureInfo = this.configuration.CultureInfo;
            }

            var fieldString = converter.ConvertToString(typeConverterOptions, field);
            this.WriteField(fieldString);
        }

        /// <summary>
        ///     Ends writing of the current record
        ///     and starts a new record. This is used
        ///     when manually writing records with WriteField.
        /// </summary>
        public virtual void NextRecord()
        {
            this.CheckDisposed();

            this.serializer.Write(this.currentRecord.ToArray());
            this.currentRecord.Clear();
        }

        /// <summary>
        ///     Write the Excel seperator record.
        /// </summary>
        public virtual void WriteExcelSeparator()
        {
            this.CheckDisposed();

            if (this.hasHeaderBeenWritten)
            {
                throw new CsvWriterException("The Excel seperator record must be the first record written in the file.");
            }

            if (this.hasRecordBeenWritten)
            {
                throw new CsvWriterException("The Excel seperator record must be the first record written in the file.");
            }

            this.WriteField("sep=" + this.configuration.Delimiter, false);

            this.NextRecord();
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

        private bool disposed;

        private readonly List<string> currentRecord = new List<string>();

        private ICsvSerializer serializer;

        private bool hasHeaderBeenWritten;

        private bool hasRecordBeenWritten;

#if !NET_2_0
        private readonly Dictionary<Type, Delegate> typeActions = new Dictionary<Type, Delegate>();
#endif

        private readonly CsvConfiguration configuration;

        private bool hasExcelSeperatorBeenRead;

#if !NET_2_0
        /// <summary>
        ///     Writes the header record from the given properties.
        /// </summary>
        /// <typeparam name="T">The type of the record.</typeparam>
        public virtual void WriteHeader<T>()
        {
            this.CheckDisposed();

            this.WriteHeader(typeof(T));
        }

        /// <summary>
        ///     Writes the header record from the given properties.
        /// </summary>
        /// <param name="type">The type of the record.</param>
        public virtual void WriteHeader(Type type)
        {
            this.CheckDisposed();

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (!this.configuration.HasHeaderRecord)
            {
                throw new CsvWriterException(
                    "Configuration.HasHeaderRecord is false. This will need to be enabled to write the header.");
            }

            if (this.hasHeaderBeenWritten)
            {
                throw new CsvWriterException(
                    "The header record has already been written. You can't write it more than once.");
            }

            if (this.hasRecordBeenWritten)
            {
                throw new CsvWriterException(
                    "Records have already been written. You can't write the header after writing records has started.");
            }

            if (type == typeof(object))
            {
                return;
            }

            if (this.configuration.Maps[type] == null)
            {
                this.configuration.Maps.Add(this.configuration.AutoMap(type));
            }

            var properties = new CsvPropertyMapCollection();
            this.AddProperties(properties, this.configuration.Maps[type]);

            foreach (var property in properties)
            {
                if (this.CanWrite(property))
                {
                    this.WriteField(property.Data.Names.FirstOrDefault());
                }
            }

            this.NextRecord();

            this.hasHeaderBeenWritten = true;
        }

        /// <summary>
        ///     Writes the record to the CSV file.
        /// </summary>
        /// <typeparam name="T">The type of the record.</typeparam>
        /// <param name="record">The record to write.</param>
        public virtual void WriteRecord<T>(T record)
        {
            this.CheckDisposed();

            try
            {
                this.GetWriteRecordAction<T>()(record);
                this.hasRecordBeenWritten = true;
                this.NextRecord();
            }
            catch (Exception ex)
            {
                ExceptionHelper.AddExceptionDataMessage(ex, null, record.GetType(), null, null, null);
                throw;
            }
        }

        /// <summary>
        ///     Writes the record to the CSV file.
        /// </summary>
        /// <param name="type">The type of the record.</param>
        /// <param name="record">The record to write.</param>
        [Obsolete(
            "This method is deprecated and will be removed in the next major release. Use WriteRecord<T>( T record ) instead.", 
            false)]
        public virtual void WriteRecord(Type type, object record)
        {
            this.CheckDisposed();

            try
            {
                try
                {
                    this.GetWriteRecordAction(type).DynamicInvoke(record);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }

                this.hasRecordBeenWritten = true;
                this.NextRecord();
            }
            catch (Exception ex)
            {
                ExceptionHelper.AddExceptionDataMessage(ex, null, type, null, null, null);
                throw;
            }
        }

        /// <summary>
        ///     Writes the list of records to the CSV file.
        /// </summary>
        /// <param name="records">The list of records to write.</param>
        public virtual void WriteRecords(IEnumerable records)
        {
            this.CheckDisposed();

            Type recordType = null;
            try
            {
                if (this.configuration.HasExcelSeparator && !this.hasExcelSeperatorBeenRead)
                {
                    this.WriteExcelSeparator();
                    this.hasExcelSeperatorBeenRead = true;
                }

                // Write the header. If records is a List<dynamic>, the header won't be written.
                // This is because typeof( T ) = Object.
                var genericEnumerable =
                    records.GetType()
                        .GetInterfaces()
                        .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (genericEnumerable != null)
                {
                    recordType = genericEnumerable.GetGenericArguments().Single();
                    if (this.configuration.HasHeaderRecord && !this.hasHeaderBeenWritten && !recordType.IsPrimitive)
                    {
                        this.WriteHeader(recordType);
                    }
                }

                foreach (var record in records)
                {
                    // If records is a List<dynamic>, the header hasn't been written yet.
                    // Write the header based on the record type.
                    recordType = record.GetType();
                    if (this.configuration.HasHeaderRecord && !this.hasHeaderBeenWritten && !recordType.IsPrimitive)
                    {
                        this.WriteHeader(recordType);
                    }

                    try
                    {
                        this.GetWriteRecordAction(record.GetType()).DynamicInvoke(record);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException;
                    }

                    this.NextRecord();
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.AddExceptionDataMessage(ex, null, recordType, null, null, null);
                throw;
            }
        }

        /// <summary>
        ///     Clears the record cache for the given type. After <see cref="ICsvWriter.WriteRecord{T}" /> is called the
        ///     first time, code is dynamically generated based on the <see cref="CsvPropertyMapCollection" />,
        ///     compiled, and stored for the given type T. If the <see cref="CsvPropertyMapCollection" />
        ///     changes, <see cref="ICsvWriter.ClearRecordCache{T}" /> needs to be called to update the
        ///     record cache.
        /// </summary>
        /// <typeparam name="T">The record type.</typeparam>
        public virtual void ClearRecordCache<T>()
        {
            this.CheckDisposed();

            this.ClearRecordCache(typeof(T));
        }

        /// <summary>
        ///     Clears the record cache for the given type. After <see cref="ICsvWriter.WriteRecord{T}" /> is called the
        ///     first time, code is dynamically generated based on the <see cref="CsvPropertyMapCollection" />,
        ///     compiled, and stored for the given type T. If the <see cref="CsvPropertyMapCollection" />
        ///     changes, <see cref="ICsvWriter.ClearRecordCache(System.Type)" /> needs to be called to update the
        ///     record cache.
        /// </summary>
        /// <param name="type">The record type.</param>
        public virtual void ClearRecordCache(Type type)
        {
            this.CheckDisposed();

            this.typeActions.Remove(type);
        }

        /// <summary>
        ///     Clears the record cache for all types. After <see cref="ICsvWriter.WriteRecord{T}" /> is called the
        ///     first time, code is dynamically generated based on the <see cref="CsvPropertyMapCollection" />,
        ///     compiled, and stored for the given type T. If the <see cref="CsvPropertyMapCollection" />
        ///     changes, <see cref="ICsvWriter.ClearRecordCache()" /> needs to be called to update the
        ///     record cache.
        /// </summary>
        public virtual void ClearRecordCache()
        {
            this.CheckDisposed();

            this.typeActions.Clear();
        }

        /// <summary>
        ///     Adds the properties from the mapping. This will recursively
        ///     traverse the mapping tree and add all properties for
        ///     reference maps.
        /// </summary>
        /// <param name="properties">The properties to be added to.</param>
        /// <param name="mapping">The mapping where the properties are added from.</param>
        protected virtual void AddProperties(CsvPropertyMapCollection properties, CsvClassMap mapping)
        {
            properties.AddRange(mapping.PropertyMaps);
            foreach (var refMap in mapping.ReferenceMaps)
            {
                this.AddProperties(properties, refMap.Data.Mapping);
            }
        }

        /// <summary>
        ///     Creates a property expression for the given property on the record.
        ///     This will recursively traverse the mapping to find the property
        ///     and create a safe property accessor for each level as it goes.
        /// </summary>
        /// <param name="recordExpression">The current property expression.</param>
        /// <param name="mapping">The mapping to look for the property to map on.</param>
        /// <param name="propertyMap">The property map to look for on the mapping.</param>
        /// <returns>An Expression to access the given property.</returns>
        protected virtual Expression CreatePropertyExpression(
            Expression recordExpression, 
            CsvClassMap mapping, 
            CsvPropertyMap propertyMap)
        {
            if (mapping.PropertyMaps.Any(pm => pm == propertyMap))
            {
                // The property is on this level.
                return Expression.Property(recordExpression, propertyMap.Data.Property);
            }

            // The property isn't on this level of the mapping.
            // We need to search down through the reference maps.
            foreach (var refMap in mapping.ReferenceMaps)
            {
                var wrapped = Expression.Property(recordExpression, refMap.Data.Property);
                var propertyExpression = this.CreatePropertyExpression(wrapped, refMap.Data.Mapping, propertyMap);
                if (propertyExpression == null)
                {
                    continue;
                }

                var isReferenceValueType = refMap.Data.Property.PropertyType.IsValueType;
                if (isReferenceValueType)
                {
                    return propertyExpression;
                }

                var nullCheckExpression = Expression.Equal(wrapped, Expression.Constant(null));

                var isValueType = propertyMap.Data.Property.PropertyType.IsValueType;
                var isGenericType = isValueType && propertyMap.Data.Property.PropertyType.IsGenericType;
                Type propertyType;
                if (isValueType && !isGenericType && !this.configuration.UseNewObjectForNullReferenceProperties)
                {
                    propertyType = typeof(Nullable<>).MakeGenericType(propertyMap.Data.Property.PropertyType);
                    propertyExpression = Expression.Convert(propertyExpression, propertyType);
                }
                else
                {
                    propertyType = propertyMap.Data.Property.PropertyType;
                }

                var defaultValueExpression = isValueType && !isGenericType
                                                 ? (Expression)Expression.New(propertyType)
                                                 : Expression.Constant(null, propertyType);
                var conditionExpression = Expression.Condition(
                    nullCheckExpression, 
                    defaultValueExpression, 
                    propertyExpression);
                return conditionExpression;
            }

            return null;
        }

#endif
#if !NET_2_0
        /// <summary>
        ///     Gets the action delegate used to write the custom
        ///     class object to the writer.
        /// </summary>
        /// <typeparam name="T">The type of the custom class being written.</typeparam>
        /// <returns>The action delegate.</returns>
        protected virtual Action<T> GetWriteRecordAction<T>()
        {
            var type = typeof(T);
            this.CreateWriteRecordAction(type);

            return (Action<T>)this.typeActions[type];
        }

        /// <summary>
        ///     Gets the action delegate used to write the custom
        ///     class object to the writer.
        /// </summary>
        /// <param name="type">The type of the custom class being written.</param>
        /// <returns>The action delegate.</returns>
        protected virtual Delegate GetWriteRecordAction(Type type)
        {
            this.CreateWriteRecordAction(type);

            return this.typeActions[type];
        }

        /// <summary>
        ///     Creates the write record action for the given type if it
        ///     doesn't already exist.
        /// </summary>
        /// <param name="type">The type of the custom class being written.</param>
        protected virtual void CreateWriteRecordAction(Type type)
        {
            if (this.typeActions.ContainsKey(type))
            {
                return;
            }

            if (this.configuration.Maps[type] == null)
            {
                // We need to check again in case the header was not written.
                this.configuration.Maps.Add(this.configuration.AutoMap(type));
            }

            if (type.IsPrimitive)
            {
                this.CreateActionForPrimitive(type);
            }
            else
            {
                this.CreateActionForObject(type);
            }
        }

        /// <summary>
        ///     Creates the action for an object.
        /// </summary>
        /// <param name="type">The type of object to create the action for.</param>
        protected virtual void CreateActionForObject(Type type)
        {
            var recordParameter = Expression.Parameter(type, "record");

            // Get a list of all the properties so they will
            // be sorted properly.
            var properties = new CsvPropertyMapCollection();
            this.AddProperties(properties, this.configuration.Maps[type]);

            if (properties.Count == 0)
            {
                throw new CsvWriterException(string.Format("No properties are mapped for type '{0}'.", type.FullName));
            }

            var delegates = new List<Delegate>();

            foreach (var propertyMap in properties)
            {
                if (!this.CanWrite(propertyMap))
                {
                    continue;
                }

                if (propertyMap.Data.TypeConverter == null
                    || !propertyMap.Data.TypeConverter.CanConvertTo(typeof(string)))
                {
                    // Skip if the type isn't convertible.
                    continue;
                }

                var fieldExpression = this.CreatePropertyExpression(
                    recordParameter, 
                    this.configuration.Maps[type], 
                    propertyMap);

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

                var method = propertyMap.Data.TypeConverter.GetType().GetMethod("ConvertToString");
                fieldExpression = Expression.Convert(fieldExpression, typeof(object));
                fieldExpression = Expression.Call(
                    typeConverterExpression, 
                    method, 
                    typeConverterOptionsExpression, 
                    fieldExpression);

                if (type.IsClass)
                {
                    var areEqualExpression = Expression.Equal(recordParameter, Expression.Constant(null));
                    fieldExpression = Expression.Condition(
                        areEqualExpression, 
                        Expression.Constant(string.Empty), 
                        fieldExpression);
                }

                var writeFieldMethodCall = Expression.Call(
                    Expression.Constant(this), 
                    "WriteField", 
                    new[] { typeof(string) }, 
                    fieldExpression);

                var actionType = typeof(Action<>).MakeGenericType(type);
                delegates.Add(Expression.Lambda(actionType, writeFieldMethodCall, recordParameter).Compile());
            }

            this.typeActions[type] = this.CombineDelegates(delegates);
        }

        /// <summary>
        ///     Creates the action for a primitive.
        /// </summary>
        /// <param name="type">The type of primitive to create the action for.</param>
        protected virtual void CreateActionForPrimitive(Type type)
        {
            var recordParameter = Expression.Parameter(type, "record");

            Expression fieldExpression = Expression.Convert(recordParameter, typeof(object));

            var typeConverter = TypeConverterFactory.GetConverter(type);
            var typeConverterExpression = Expression.Constant(typeConverter);
            var method = typeConverter.GetType().GetMethod("ConvertToString");

            var typeConverterOptions = TypeConverterOptionsFactory.GetOptions(type);
            if (typeConverterOptions.CultureInfo == null)
            {
                typeConverterOptions.CultureInfo = this.configuration.CultureInfo;
            }

            fieldExpression = Expression.Call(
                typeConverterExpression, 
                method, 
                Expression.Constant(typeConverterOptions), 
                fieldExpression);

            fieldExpression = Expression.Call(
                Expression.Constant(this), 
                "WriteField", 
                new[] { typeof(string) }, 
                fieldExpression);

            var actionType = typeof(Action<>).MakeGenericType(type);
            this.typeActions[type] = Expression.Lambda(actionType, fieldExpression, recordParameter).Compile();
        }

        /// <summary>
        ///     Combines the delegates into a single multicast delegate.
        ///     This is needed because Silverlight doesn't have the
        ///     Delegate.Combine( params Delegate[] ) overload.
        /// </summary>
        /// <param name="delegates">The delegates to combine.</param>
        /// <returns>A multicast delegate combined from the given delegates.</returns>
        protected virtual Delegate CombineDelegates(IEnumerable<Delegate> delegates)
        {
            return delegates.Aggregate<Delegate, Delegate>(null, Delegate.Combine);
        }

        /// <summary>
        ///     Checks if the property can be written.
        /// </summary>
        /// <param name="propertyMap">The property map that we are checking.</param>
        /// <returns>
        ///     A value indicating if the property can be written.
        ///     True if the property can be written, otherwise false.
        /// </returns>
        protected virtual bool CanWrite(CsvPropertyMap propertyMap)
        {
            var cantWrite =

                // Ignored properties.
                propertyMap.Data.Ignore ||

                // Properties that don't have a public getter
                // and we are honoring the accessor modifier.
                propertyMap.Data.Property.GetGetMethod() == null && !this.configuration.IgnorePrivateAccessor ||

                // Properties that don't have a getter at all.
                propertyMap.Data.Property.GetGetMethod(true) == null;
            return !cantWrite;
        }

#endif
    }
}