﻿// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com

#if WINRT_4_5
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else

#endif

namespace CsvHelper.Tests.TypeConversion
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TypeConverterOptionsFactoryTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public void AddGetRemoveTest()
        {
            var customOptions = new TypeConverterOptions { Format = "custom" };
            TypeConverterOptionsFactory.AddOptions<string>(customOptions);
            var options = TypeConverterOptionsFactory.GetOptions<string>();

            Assert.AreEqual(customOptions.Format, options.Format);

            TypeConverterOptionsFactory.RemoveOptions<string>();

            options = TypeConverterOptionsFactory.GetOptions<string>();

            Assert.AreNotEqual(customOptions.Format, options.Format);
        }

        [TestMethod]
        public void GetFieldTest()
        {
            var options = new TypeConverterOptions { NumberStyle = NumberStyles.AllowThousands };
            TypeConverterOptionsFactory.AddOptions<int>(options);

            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var csvReader = new CsvReader(reader))
            {
                writer.WriteLine("\"1,234\",\"5,678\"");
                writer.Flush();
                stream.Position = 0;

                csvReader.Configuration.HasHeaderRecord = false;
                csvReader.Read();
                Assert.AreEqual(1234, csvReader.GetField<int>(0));
                Assert.AreEqual(5678, csvReader.GetField(typeof(int), 1));
            }
        }

        [TestMethod]
        public void GetRecordsTest()
        {
            var options = new TypeConverterOptions { NumberStyle = NumberStyles.AllowThousands };
            TypeConverterOptionsFactory.AddOptions<int>(options);

            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var csvReader = new CsvReader(reader))
            {
                writer.WriteLine("\"1,234\",\"5,678\"");
                writer.Flush();
                stream.Position = 0;

                csvReader.Configuration.HasHeaderRecord = false;
                csvReader.GetRecords<Test>().ToList();
            }
        }

        [TestMethod]
        public void GetRecordsAppliedWhenMappedTest()
        {
            var options = new TypeConverterOptions { NumberStyle = NumberStyles.AllowThousands };
            TypeConverterOptionsFactory.AddOptions<int>(options);

            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var csvReader = new CsvReader(reader))
            {
                writer.WriteLine("\"1,234\",\"$5,678\"");
                writer.Flush();
                stream.Position = 0;

                csvReader.Configuration.HasHeaderRecord = false;
                csvReader.Configuration.RegisterClassMap<TestMap>();
                csvReader.GetRecords<Test>().ToList();
            }
        }

        [TestMethod]
        public void WriteFieldTest()
        {
            var options = new TypeConverterOptions { Format = "c" };
            TypeConverterOptionsFactory.AddOptions<int>(options);

            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var csvWriter = new CsvWriter(writer))
            {
                csvWriter.WriteField(1234);
                csvWriter.NextRecord();
                writer.Flush();
                stream.Position = 0;
                var record = reader.ReadToEnd();

                Assert.AreEqual("\"$1,234.00\"\r\n", record);
            }
        }

        [TestMethod]
        public void WriteRecordsTest()
        {
            var options = new TypeConverterOptions { Format = "c" };
            TypeConverterOptionsFactory.AddOptions<int>(options);

            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var csvWriter = new CsvWriter(writer))
            {
                var list = new List<Test> { new Test { Number = 1234, NumberOverridenInMap = 5678 } };
                csvWriter.Configuration.HasHeaderRecord = false;
                csvWriter.WriteRecords(list);
                writer.Flush();
                stream.Position = 0;
                var record = reader.ReadToEnd();

                Assert.AreEqual("\"$1,234.00\",\"$5,678.00\"\r\n", record);
            }
        }

        [TestMethod]
        public void WriteRecordsAppliedWhenMappedTest()
        {
            var options = new TypeConverterOptions { Format = "c" };
            TypeConverterOptionsFactory.AddOptions<int>(options);

            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var csvWriter = new CsvWriter(writer))
            {
                var list = new List<Test> { new Test { Number = 1234, NumberOverridenInMap = 5678 } };
                csvWriter.Configuration.HasHeaderRecord = false;
                csvWriter.Configuration.RegisterClassMap<TestMap>();
                csvWriter.WriteRecords(list);
                writer.Flush();
                stream.Position = 0;
                var record = reader.ReadToEnd();

                Assert.AreEqual("\"$1,234.00\",\"5,678.00\"\r\n", record);
            }
        }

        private class Test
        {
            public int Number { get; set; }

            public int NumberOverridenInMap { get; set; }
        }

        private class TestMap : CsvClassMap<Test>
        {
            public TestMap()
            {
                this.Map(m => m.Number);
                this.Map(m => m.NumberOverridenInMap)
                    .TypeConverterOption(NumberStyles.AllowThousands | NumberStyles.AllowCurrencySymbol)
                    .TypeConverterOption("N");
            }
        }
    }
}