// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com

#if WINRT_4_5
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else

#endif

namespace CsvHelper.Tests
{
    using System;
    using System.IO;
    using CsvHelper.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CsvConfigurationTests
    {
        [TestMethod]
        public void EnsureReaderAndParserConfigIsAreSameTest()
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            {
                var csvReader = new CsvReader(reader);

                Assert.AreSame(csvReader.Configuration, csvReader.Parser.Configuration);

                var config = new CsvConfiguration();
                var parser = new CsvParser(reader, config);
                csvReader = new CsvReader(parser);

                Assert.AreSame(csvReader.Configuration, csvReader.Parser.Configuration);
            }
        }

        [TestMethod]
        public void AddingMappingsWithGenericMethod1Test()
        {
            // TODO: Implement me
        }

        [TestMethod]
        public void AddingMappingsWithGenericMethod2Test()
        {
            var config = new CsvConfiguration();
            config.RegisterClassMap<TestClassMappings>();

            Assert.AreEqual(2, config.Maps[typeof(TestClass)].PropertyMaps.Count);
        }

        [TestMethod]
        public void AddingMappingsWithNonGenericMethodTest()
        {
            var config = new CsvConfiguration();
            config.RegisterClassMap(typeof(TestClassMappings));

            Assert.AreEqual(2, config.Maps[typeof(TestClass)].PropertyMaps.Count);
        }

        [TestMethod]
        public void AddingMappingsWithInstanceMethodTest()
        {
            // TODO: Implement me			
        }

        [TestMethod]
        public void RegisterClassMapGenericTest()
        {
            // TODO: Implement me
        }

        [TestMethod]
        public void RegisterClassMapNonGenericTest()
        {
            // TODO: Implement me
        }

        [TestMethod]
        public void RegisterClassInstanceTest()
        {
            var config = new CsvConfiguration();

            Assert.IsNull(config.Maps[typeof(TestClass)]);
            config.RegisterClassMap(new TestClassMappings());
            Assert.IsNotNull(config.Maps[typeof(TestClass)]);
        }

        [TestMethod]
        public void UnregisterClassMapGenericTest()
        {
            var config = new CsvConfiguration();

            Assert.IsNull(config.Maps[typeof(TestClass)]);
            config.RegisterClassMap<TestClassMappings>();
            Assert.IsNotNull(config.Maps[typeof(TestClass)]);

            config.UnregisterClassMap<TestClassMappings>();
            Assert.IsNull(config.Maps[typeof(TestClass)]);
        }

        [TestMethod]
        public void UnregisterClassNonMapGenericTest()
        {
            // TODO: Implement me			
        }

#if !WINRT_4_5
        [TestMethod]
        public void AddingMappingsWithNonGenericMethodThrowsWhenNotACsvClassMap()
        {
            try
            {
                new CsvConfiguration().RegisterClassMap(typeof(TestClass));
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

#endif

        private class TestClass
        {
            public string StringColumn { get; set; }

            public int IntColumn { get; set; }
        }

        private sealed class TestClassMappings : CsvClassMap<TestClass>
        {
            public TestClassMappings()
            {
                this.Map(c => c.StringColumn);
                this.Map(c => c.IntColumn);
            }
        }
    }
}