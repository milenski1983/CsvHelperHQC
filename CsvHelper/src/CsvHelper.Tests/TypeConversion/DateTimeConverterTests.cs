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
    using System;
    using System.Globalization;
    using CsvHelper.TypeConversion;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DateTimeConverterTests
    {
        [TestMethod]
        public void ConvertToStringTest()
        {
            var converter = new DateTimeConverter();
            var typeConverterOptions = new TypeConverterOptions { CultureInfo = CultureInfo.CurrentCulture };

            var dateTime = DateTime.Now;

            // Valid conversions.
            Assert.AreEqual(dateTime.ToString(), converter.ConvertToString(typeConverterOptions, dateTime));

            // Invalid conversions.
            Assert.AreEqual("1", converter.ConvertToString(typeConverterOptions, 1));
            Assert.AreEqual(string.Empty, converter.ConvertToString(typeConverterOptions, null));
        }

        [TestMethod]
        public void ConvertFromStringTest()
        {
            var converter = new DateTimeConverter();
            var typeConverterOptions = new TypeConverterOptions { CultureInfo = CultureInfo.CurrentCulture };

            var dateTime = DateTime.Now;

            // Valid conversions.
            Assert.AreEqual(
                dateTime.ToString(), 
                converter.ConvertFromString(typeConverterOptions, dateTime.ToString()).ToString());
            Assert.AreEqual(
                dateTime.ToString(), 
                converter.ConvertFromString(typeConverterOptions, dateTime.ToString("o")).ToString());
            Assert.AreEqual(
                dateTime.ToString(), 
                converter.ConvertFromString(typeConverterOptions, " " + dateTime + " ").ToString());

            // Invalid conversions.
            try
            {
                converter.ConvertFromString(typeConverterOptions, null);
                Assert.Fail();
            }
            catch (CsvTypeConverterException)
            {
            }
        }

#if !PCL
        [TestMethod]
        public void ComponentModelCompatibilityTest()
        {
            var converter = new DateTimeConverter();
            var cmConverter = new System.ComponentModel.DateTimeConverter();

            var typeConverterOptions = new TypeConverterOptions { CultureInfo = CultureInfo.CurrentCulture };

            var val = (DateTime)cmConverter.ConvertFromString(string.Empty);
            Assert.AreEqual(DateTime.MinValue, val);

            val = (DateTime)converter.ConvertFromString(typeConverterOptions, string.Empty);
            Assert.AreEqual(DateTime.MinValue, val);

            try
            {
                cmConverter.ConvertFromString(null);
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                converter.ConvertFromString(typeConverterOptions, null);
                Assert.Fail();
            }
            catch (CsvTypeConverterException)
            {
            }

            try
            {
                cmConverter.ConvertFromString("blah");
                Assert.Fail();
            }
            catch (FormatException)
            {
            }

            try
            {
                converter.ConvertFromString(typeConverterOptions, "blah");
            }
            catch (FormatException)
            {
            }
        }

#endif
    }
}