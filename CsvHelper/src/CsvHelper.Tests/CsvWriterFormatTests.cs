// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com

using Microsoft.VisualStudio.TestTools.UnitTesting;
#if WINRT_4_5
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else

#endif

namespace CsvHelper.Tests
{
    using System;
    using System.Globalization;
    using System.IO;

    using CsvHelper.Configuration;

    [TestClass]
    public class CsvWriterFormatTests
    {
        [TestMethod]
        public void WriteFieldTest()
        {
            var record = new TestRecord
                             {
                                 IntColumn = 1, 
                                 DateColumn = new DateTime(2012, 10, 1, 12, 12, 12), 
                                 DecimalColumn = 150.99m, 
                                 FirstColumn = "first column"
                             };

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream) { AutoFlush = true };
            var csv = new CsvWriter(writer);
            csv.Configuration.CultureInfo = new CultureInfo("en-US");
            csv.Configuration.RegisterClassMap<TestRecordMap>();

            csv.WriteRecord(record);

            stream.Position = 0;
            var reader = new StreamReader(stream);
            var csvFile = reader.ReadToEnd();
            var expected = "first column,0001,10/1/2012,$150.99\r\n";

            Assert.AreEqual(expected, csvFile);
        }

        [TestMethod]
        public void WriteFieldTrimTest()
        {
            var record = new TestRecord
                             {
                                 IntColumn = 1, 
                                 DateColumn = new DateTime(2012, 10, 1, 12, 12, 12), 
                                 DecimalColumn = 150.99m, 
                                 FirstColumn = "first column "
                             };

            var config = new CsvConfiguration { CultureInfo = new CultureInfo("en-US"), TrimFields = true };
            config.RegisterClassMap<TestRecordMap>();

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream) { AutoFlush = true };
            var csv = new CsvWriter(writer, config);

            csv.WriteRecord(record);

            stream.Position = 0;
            var reader = new StreamReader(stream);
            var csvFile = reader.ReadToEnd();
            var expected = "first column,0001,10/1/2012,$150.99\r\n";

            Assert.AreEqual(expected, csvFile);
        }

        [TestMethod]
        public void WriteFieldShouldQuoteNoTest()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream) { AutoFlush = true };
            var csv = new CsvWriter(writer);

            csv.WriteField("a \"b\" c", false);
            csv.NextRecord();

            stream.Position = 0;
            var reader = new StreamReader(stream);
            var csvFile = reader.ReadToEnd();
            var expected = "a \"b\" c\r\n";

            Assert.AreEqual(expected, csvFile);
        }

        [TestMethod]
        public void WriteFieldShouldQuoteYesTest()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream) { AutoFlush = true };
            var csv = new CsvWriter(writer);

            csv.WriteField("a \"b\" c", true);
            csv.NextRecord();

            stream.Position = 0;
            var reader = new StreamReader(stream);
            var csvFile = reader.ReadToEnd();
            var expected = "\"a \"\"b\"\" c\"\r\n";

            Assert.AreEqual(expected, csvFile);
        }

        [TestMethod]
        public void WriteRecordWithReferencesTest()
        {
            var record = new Person
                             {
                                 FirstName = "First Name", 
                                 LastName = "Last Name", 
                                 Updated = new DateTime(2012, 10, 1, 12, 12, 12, 123), 
                                 HomeAddress =
                                     new Address
                                         {
                                             Street = "Home Street", 
                                             City = "Home City", 
                                             State = "Home State", 
                                             Zip = 02201
                                         }, 
                                 WorkAddress =
                                     new Address
                                         {
                                             Street = "Work Street", 
                                             City = "Work City", 
                                             State = "Work State", 
                                             Zip = 04100
                                         }
                             };

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream) { AutoFlush = true };
            var csv = new CsvWriter(writer);
            csv.Configuration.RegisterClassMap<PersonMap>();

            csv.WriteRecord(record);

            stream.Position = 0;
            var reader = new StreamReader(stream);
            var csvFile = reader.ReadToEnd();

            var expected =
                "First Name,Last Name,2012-10-01 12:12:12.123,Home Street,Home City,Home State,02201,Work Street,Work City,Work State,04100\r\n";

            Assert.AreEqual(expected, csvFile);
        }

        private class TestRecord
        {
            public int IntColumn { get; set; }

            public DateTime DateColumn { get; set; }

            public decimal DecimalColumn { get; set; }

            public string FirstColumn { get; set; }
        }

        private sealed class TestRecordMap : CsvClassMap<TestRecord>
        {
            public TestRecordMap()
            {
                this.Map(m => m.IntColumn).Name("Int Column").Index(1).TypeConverterOption("0000");
                this.Map(m => m.DateColumn).Index(2).TypeConverterOption("d");
                this.Map(m => m.DecimalColumn).Index(3).TypeConverterOption("c");
                this.Map(m => m.FirstColumn).Index(0);
            }
        }

        private class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public DateTime Updated { get; set; }

            public Address HomeAddress { get; set; }

            public Address WorkAddress { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }

            public string City { get; set; }

            public string State { get; set; }

            public int Zip { get; set; }
        }

        private sealed class PersonMap : CsvClassMap<Person>
        {
            public PersonMap()
            {
                this.Map(m => m.FirstName);
                this.Map(m => m.LastName);
                this.Map(m => m.Updated).TypeConverterOption("yyyy-MM-dd HH:mm:ss.fff");
                this.References<HomeAddressMap>(m => m.HomeAddress);
                this.References<WorkAddressMap>(m => m.WorkAddress);
            }
        }

        private sealed class HomeAddressMap : CsvClassMap<Address>
        {
            public HomeAddressMap()
            {
                this.Map(m => m.Street).Name("HomeStreet");
                this.Map(m => m.City).Name("HomeCity");
                this.Map(m => m.State).Name("HomeState");
                this.Map(m => m.Zip).Name("HomeZip").TypeConverterOption("00000");
            }
        }

        private sealed class WorkAddressMap : CsvClassMap<Address>
        {
            public WorkAddressMap()
            {
                this.Map(m => m.Street).Name("WorkStreet");
                this.Map(m => m.City).Name("WorkCity");
                this.Map(m => m.State).Name("WorkState");
                this.Map(m => m.Zip).Name("WorkZip").TypeConverterOption("00000");
            }
        }
    }
}