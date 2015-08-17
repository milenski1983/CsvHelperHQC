// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com
namespace CsvHelper.Example
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web.Script.Serialization;

    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;

    public class Program
    {
        private const string ColumnSeparator = ":";

        public static void Main(string[] args)
        {
            // ReadRawFieldsByIndex();
            // ReadRawFieldsByName();
            // ReadFieldsByIndex();
            // ReadRecordsNoAttributes();
            // ReadRecordsWithAttributes();
            // ReadAllRecords();

            WriteRawFields();
            // WriteFields();
            // WriteRecordsNoAttributes();
            // WriteRecordsWithAttributes();
            // WriteAllRecords();
            Console.ReadKey();
        }

        public static void ReadRawFieldsByIndex()
        {
            Console.WriteLine("Raw fields by index:");

            using (var reader = new CsvReader(new StreamReader(GetDataStream(true, true))))
            {
                while (reader.Read())
                {
                    Console.Write(reader.GetField(0) + ColumnSeparator);
                    Console.Write(reader.GetField(1) + ColumnSeparator);
                    Console.Write(reader.GetField(2) + ColumnSeparator);
                    Console.WriteLine(reader.GetField(3));
                }
            }

            Console.WriteLine();
        }

        public static void ReadRawFieldsByName()
        {
            Console.WriteLine("Raw fields by name:");

            using (var reader = new CsvReader(new StreamReader(GetDataStream(true, true))))
            {
                while (reader.Read())
                {
                    Console.Write(reader.GetField("String Column") + ColumnSeparator);
                    Console.Write(reader.GetField("Int Column") + ColumnSeparator);
                    Console.Write(reader.GetField("Guid Column") + ColumnSeparator);
                    Console.Write(reader.GetField("Does Not Exist Column") + ColumnSeparator);
                    Console.WriteLine(reader.GetField("Custom Type Column"));
                }
            }

            Console.WriteLine();
        }

        public static void ReadFieldsByIndex()
        {
            Console.WriteLine("Fields by index:");

            var customTypeTypeConverter = new CustomTypeTypeConverter();

            using (var reader = new CsvReader(new StreamReader(GetDataStream(true, true))))
            {
                while (reader.Read())
                {
                    Console.Write(reader.GetField<string>(0) + ColumnSeparator);
                    Console.Write(reader.GetField<int>("Int Column") + ColumnSeparator);
                    Console.Write(reader.GetField<Guid>(2) + ColumnSeparator);
                    Console.WriteLine(reader.GetField<CustomType>(3, customTypeTypeConverter));
                }
            }

            Console.WriteLine();
        }

        public static void ReadRecordsNoAttributes()
        {
            Console.WriteLine("Records no attributes:");

            using (var reader = new CsvReader(new StreamReader(GetDataStream(true, false))))
            {
                while (reader.Read())
                {
                    Console.WriteLine(reader.GetRecord<CustomObject>());
                }
            }

            Console.WriteLine();
        }

        public static void ReadRecordsWithAttributes()
        {
            Console.WriteLine("Records with attributes:");

            using (var reader = new CsvReader(new StreamReader(GetDataStream(true, true))))
            {
                reader.Configuration.RegisterClassMap<CustomObjectWithMappingMap>();

                while (reader.Read())
                {
                    Console.WriteLine(reader.GetRecord<CustomObjectWithMapping>());
                }
            }

            Console.WriteLine();
        }

        public static void ReadAllRecords()
        {
            Console.WriteLine("All records:");

            using (var reader = new CsvReader(new StreamReader(GetDataStream(true, false))))
            {
                var records = reader.GetRecords<CustomObject>();
                foreach (var record in records)
                {
                    Console.WriteLine(record);
                }
            }

            Console.WriteLine();
        }

        public static void WriteRawFields()
        {
            Console.WriteLine("Write raw fields");

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var streamReader = new StreamReader(memoryStream))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.WriteField("String Column");
                writer.WriteField("Int Column");
                writer.WriteField("Guid Column");
                writer.WriteField("Custom Type Column");
                writer.NextRecord();

                writer.WriteField("one");
                writer.WriteField(1.ToString());
                writer.WriteField(Guid.NewGuid().ToString());
                writer.WriteField((new CustomType { First = 1, Second = 2, Third = 3 }).ToString());
                writer.NextRecord();

                memoryStream.Position = 0;

                Console.WriteLine(streamReader.ReadToEnd());
            }

            Console.WriteLine();
        }

        public static void WriteFields()
        {
            Console.WriteLine("Write fields");

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var streamReader = new StreamReader(memoryStream))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.WriteField("String Column");
                writer.WriteField("Int Column");
                writer.WriteField("Guid Column");
                writer.WriteField("Custom Type Column");
                writer.NextRecord();

                writer.WriteField("one");
                writer.WriteField(1);
                writer.WriteField(Guid.NewGuid());
                writer.WriteField(new CustomType { First = 1, Second = 2, Third = 3 });
                writer.NextRecord();

                memoryStream.Position = 0;

                Console.WriteLine(streamReader.ReadToEnd());
            }

            Console.WriteLine();
        }

        public static void WriteRecordsNoAttributes()
        {
            Console.WriteLine("Write records no attributes:");

            var records = new List<CustomObject>
                              {
                                  new CustomObject
                                      {
                                          CustomTypeColumn = new CustomType
                                                  {
                                                      First = 1, 
                                                      Second = 2, 
                                                      Third = 3
                                                  }, 
                                          GuidColumn = Guid.NewGuid(), 
                                          IntColumn = 1, 
                                          StringColumn = "one"
                                      }, 
                                  new CustomObject
                                      {
                                          CustomTypeColumn = new CustomType
                                                  {
                                                      First = 4, 
                                                      Second = 5, 
                                                      Third = 6
                                                  }, 
                                          GuidColumn = Guid.NewGuid(), 
                                          IntColumn = 2, 
                                          StringColumn = "two"
                                      }
                              };

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var streamReader = new StreamReader(memoryStream))
            using (var writer = new CsvWriter(streamWriter))
            {
                foreach (var record in records)
                {
                    writer.WriteRecord(record);
                }

                memoryStream.Position = 0;

                Console.WriteLine(streamReader.ReadToEnd());
            }

            Console.WriteLine();
        }

        public static void WriteRecordsWithAttributes()
        {
            Console.WriteLine("Write records with attributes:");

            var records = new List<CustomObjectWithMapping>
                              {
                                  new CustomObjectWithMapping
                                      {
                                          CustomTypeColumn = new CustomType
                                                  {
                                                      First = 1, 
                                                      Second = 2, 
                                                      Third = 3
                                                  }, 
                                          GuidColumn = Guid.NewGuid(), 
                                          IntColumn = 1, 
                                          StringColumn = "one"
                                      }, 
                                  new CustomObjectWithMapping
                                      {
                                          CustomTypeColumn = new CustomType
                                                  {
                                                      First = 4, 
                                                      Second = 5, 
                                                      Third = 6
                                                  }, 
                                          GuidColumn = Guid.NewGuid(), 
                                          IntColumn = 2, 
                                          StringColumn = "two"
                                      }
                              };

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var streamReader = new StreamReader(memoryStream))
            using (var writer = new CsvWriter(streamWriter))
            {
                foreach (var record in records)
                {
                    writer.WriteRecord(record);
                }

                memoryStream.Position = 0;

                Console.WriteLine(streamReader.ReadToEnd());
            }

            Console.WriteLine();
        }

        public static void WriteAllRecords()
        {
            Console.WriteLine("Write all records with attributes:");

            var records = new List<CustomObjectWithMapping>
                              {
                                  new CustomObjectWithMapping
                                      {
                                          CustomTypeColumn = new CustomType
                                                  {
                                                      First = 1, 
                                                      Second = 2, 
                                                      Third = 3
                                                  }, 
                                          GuidColumn = Guid.NewGuid(), 
                                          IntColumn = 1, 
                                          StringColumn = "one"
                                      }, 
                                  new CustomObjectWithMapping
                                      {
                                          CustomTypeColumn = new CustomType
                                                  {
                                                      First = 4, 
                                                      Second = 5, 
                                                      Third = 6
                                                  }, 
                                          GuidColumn = Guid.NewGuid(), 
                                          IntColumn = 2, 
                                          StringColumn = "two"
                                      }
                              };

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var streamReader = new StreamReader(memoryStream))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.Configuration.RegisterClassMap<CustomObjectWithMappingMap>();
                writer.WriteRecords(records);

                memoryStream.Position = 0;

                Console.WriteLine(streamReader.ReadToEnd());
            }

            Console.WriteLine();
        }

        public static MemoryStream GetDataStream(bool hasHeader, bool hasSpacesInHeaderNames)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            if (hasHeader)
            {
                var header = hasSpacesInHeaderNames
                                 ? "String Column,Int Column,Guid Column,Custom Type Column"
                                 : "StringColumn,IntColumn,GuidColumn,CustomTypeColumn";
                writer.WriteLine(header);
            }

            writer.WriteLine("one,1,{0},1|2|3", Guid.NewGuid());
            writer.WriteLine("two,2,{0},4|5|6", Guid.NewGuid());
            writer.WriteLine("\"this, has a comma\",2,{0},7|8|9", Guid.NewGuid());
            writer.WriteLine("\"this has \"\"'s\",4,{0},10|11|12", Guid.NewGuid());
            writer.Flush();
            stream.Position = 0;

            return stream;
        }
    }
}