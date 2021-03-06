﻿// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com

#if WINRT_4_5
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else

#endif

namespace CsvHelper.Tests
{
    using System.Collections.Generic;
    using CsvHelper.Configuration;
    using CsvHelper.Tests.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ReferenceMappingClassMapTests
    {
        [TestMethod]
        public void ReferenceMappingTest()
        {
            var queue = new Queue<string[]>();
            queue.Enqueue(
                new[]
                    {
                        "FirstName", "LastName", "HomeStreet", "HomeCity", "HomeState", "HomeZip", "WorkStreet", 
                        "WorkCity", "WorkState", "WorkZip"
                    });
            var row = new[]
                          {
                              "John", "Doe", "1234 Home St", "Home Town", "Home State", "12345", "5678 Work Rd", 
                              "Work City", "Work State", "67890"
                          };
            queue.Enqueue(row);

            var parserMock = new ParserMock(queue);

            var reader = new CsvReader(parserMock);
            reader.Configuration.RegisterClassMap<PersonMap>();
            reader.Read();
            var person = reader.GetRecord<Person>();

            Assert.AreEqual(row[0], person.FirstName);
            Assert.AreEqual(row[1], person.LastName);
            Assert.AreEqual(row[2], person.HomeAddress.Street);
            Assert.AreEqual(row[3], person.HomeAddress.City);
            Assert.AreEqual(row[4], person.HomeAddress.State);
            Assert.AreEqual(row[5], person.HomeAddress.Zip);
            Assert.AreEqual(row[6], person.WorkAddress.Street);
            Assert.AreEqual(row[7], person.WorkAddress.City);
            Assert.AreEqual(row[8], person.WorkAddress.State);
            Assert.AreEqual(row[9], person.WorkAddress.Zip);
        }

        [TestMethod]
        public void OnlyReferencesTest()
        {
            var queue = new Queue<string[]>();
            queue.Enqueue(
                new[]
                    {
                        "FirstName", "LastName", "HomeStreet", "HomeCity", "HomeState", "HomeZip", "WorkStreet", 
                        "WorkCity", "WorkState", "WorkZip"
                    });
            queue.Enqueue(
                new[]
                    {
                        "John", "Doe", "1234 Home St", "Home Town", "Home State", "12345", "5678 Work Rd", "Work City", 
                        "Work State", "67890"
                    });
            queue.Enqueue(null);

            var parserMock = new ParserMock(queue);

            var reader = new CsvReader(parserMock);
            reader.Configuration.RegisterClassMap<OnlyReferencesMap>();
            reader.Read();
            var person = reader.GetRecord<Person>();
        }

        private class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public Address HomeAddress { get; set; }

            public Address WorkAddress { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }

            public string City { get; set; }

            public string State { get; set; }

            public string Zip { get; set; }
        }

        private sealed class PersonMap : CsvClassMap<Person>
        {
            public PersonMap()
            {
                this.Map(m => m.FirstName);
                this.Map(m => m.LastName);
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
                this.Map(m => m.Zip).Name("HomeZip");
            }
        }

        private sealed class WorkAddressMap : CsvClassMap<Address>
        {
            public WorkAddressMap()
            {
                this.Map(m => m.Street).Name("WorkStreet");
                this.Map(m => m.City).Name("WorkCity");
                this.Map(m => m.State).Name("WorkState");
                this.Map(m => m.Zip).Name("WorkZip");
            }
        }

        private sealed class OnlyReferencesMap : CsvClassMap<Person>
        {
            public OnlyReferencesMap()
            {
                this.References<HomeAddressMap>(m => m.HomeAddress);
                this.References<WorkAddressMap>(m => m.WorkAddress);
            }
        }
    }
}