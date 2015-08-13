// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com
namespace CsvHelper.Tests
{
    using CsvHelper.Configuration;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MappingInheritedClassTests
    {
        [TestMethod]
        public void Test()
        {
            var map = new AMap<A>();
            Assert.AreEqual(2, map.PropertyMaps.Count);
        }

        private interface IA
        {
            int Id { get; set; }
        }

        private class A : IA
        {
            public int Name { get; set; }

            public int Id { get; set; }
        }

        private sealed class AMap<T> : CsvClassMap<T>
            where T : IA
        {
            public AMap()
            {
                this.AutoMap();
                this.Map(m => m.Id);
            }
        }
    }
}