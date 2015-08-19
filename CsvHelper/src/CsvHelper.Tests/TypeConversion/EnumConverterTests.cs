// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com

#if WINRT_4_5
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
#endif

namespace CsvHelper.Tests.TypeConversion
{
    using System;
    using System.Globalization;
    using CsvHelper.TypeConversion;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EnumConverterTests
    {
        private enum TestEnum
        {
            None = 0, 

            One = 1
        }

        [TestMethod]
        public void ConstructorTest()
        {
            try
            {
                new EnumConverter(typeof(string));
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("'System.String' is not an Enum.", ex.Message);
            }
        }

        [TestMethod]
        public void ConvertToStringTest()
        {
            var converter = new EnumConverter(typeof(TestEnum));
            var typeConverterOptions = new TypeConverterOptions { CultureInfo = CultureInfo.CurrentCulture };

            Assert.AreEqual("None", converter.ConvertToString(typeConverterOptions, (TestEnum)0));
            Assert.AreEqual("None", converter.ConvertToString(typeConverterOptions, TestEnum.None));
            Assert.AreEqual("One", converter.ConvertToString(typeConverterOptions, (TestEnum)1));
            Assert.AreEqual("One", converter.ConvertToString(typeConverterOptions, TestEnum.One));
            Assert.AreEqual(string.Empty, converter.ConvertToString(typeConverterOptions, null));
        }

        [TestMethod]
        public void ConvertFromStringTest()
        {
            var converter = new EnumConverter(typeof(TestEnum));
            var typeConverterOptions = new TypeConverterOptions { CultureInfo = CultureInfo.CurrentCulture };

            Assert.AreEqual(TestEnum.One, converter.ConvertFromString(typeConverterOptions, "One"));
            Assert.AreEqual(TestEnum.One, converter.ConvertFromString(typeConverterOptions, "one"));
            Assert.AreEqual(TestEnum.One, converter.ConvertFromString(typeConverterOptions, "1"));
            try
            {
                Assert.AreEqual(TestEnum.One, converter.ConvertFromString(typeConverterOptions, string.Empty));
                Assert.Fail();
            }
            catch (CsvTypeConverterException)
            {
            }

            try
            {
                Assert.AreEqual(TestEnum.One, converter.ConvertFromString(typeConverterOptions, null));
                Assert.Fail();
            }
            catch (CsvTypeConverterException)
            {
            }
        }
    }
}