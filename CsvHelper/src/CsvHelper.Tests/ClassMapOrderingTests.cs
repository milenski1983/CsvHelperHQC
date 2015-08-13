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
    using System.Collections.Generic;
    using System.IO;

    using CsvHelper.Configuration;

    [TestClass]
    public class ClassMapOrderingTests
    {
        [TestMethod]
        public void OrderingTest()
        {
            var list = new List<ContainerClass>
                           {
                               new ContainerClass
                                   {
                                       Contents =
                                           new ThirdClass
                                               {
                                                   Third = 3, 
                                                   Second =
                                                       new SecondClass
                                                           {
                                                               Second
                                                                   =
                                                                   2
                                                           }, 
                                                   First =
                                                       new FirstClass
                                                           {
                                                               First
                                                                   =
                                                                   1
                                                           }
                                               }
                                   }
                           };

            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer))
            {
                csv.Configuration.RegisterClassMap<ContainerClassMap>();
                csv.WriteRecords(list);
                writer.Flush();
                stream.Position = 0;

                Assert.AreEqual("First,Second,Third", reader.ReadLine());
            }
        }

        private class ContainerClass
        {
            public ThirdClass Contents { get; set; }
        }

        private class ThirdClass
        {
            public int Third { get; set; }

            public SecondClass Second { get; set; }

            public FirstClass First { get; set; }
        }

        private sealed class ContainerClassMap : CsvClassMap<ContainerClass>
        {
            public ContainerClassMap()
            {
                this.References<ThirdClassMap>(m => m.Contents);
            }
        }

        private sealed class ThirdClassMap : CsvClassMap<ThirdClass>
        {
            public ThirdClassMap()
            {
                this.References<FirstClassMap>(m => m.First);
                this.References<SecondClassMap>(m => m.Second);
                this.Map(m => m.Third);
            }
        }

        private class SecondClass
        {
            public int Second { get; set; }
        }

        private sealed class SecondClassMap : CsvClassMap<SecondClass>
        {
            public SecondClassMap()
            {
                this.Map(m => m.Second);
            }
        }

        private class FirstClass
        {
            public int First { get; set; }
        }

        private sealed class FirstClassMap : CsvClassMap<FirstClass>
        {
            public FirstClassMap()
            {
                this.Map(m => m.First);
            }
        }
    }
}