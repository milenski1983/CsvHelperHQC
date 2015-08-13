namespace CsvHelper.Tests.TypeConversion
{
    using System;
    using System.Globalization;

    using CsvHelper.TypeConversion;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DateTimeOffsetConverterTests
    {
        [TestMethod]
        public void ConvertToStringTest()
        {
            var converter = new DateTimeOffsetConverter();
            var typeConverterOptions = new TypeConverterOptions { CultureInfo = CultureInfo.CurrentCulture };

            var dateTime = DateTimeOffset.Now;

            // Valid conversions.
            Assert.AreEqual(dateTime.ToString(), converter.ConvertToString(typeConverterOptions, dateTime));

            // Invalid conversions.
            Assert.AreEqual("1", converter.ConvertToString(typeConverterOptions, 1));
            Assert.AreEqual(string.Empty, converter.ConvertToString(typeConverterOptions, null));
        }

        [TestMethod]
        public void ConvertFromStringTest()
        {
            var converter = new DateTimeOffsetConverter();
            var typeConverterOptions = new TypeConverterOptions { CultureInfo = CultureInfo.CurrentCulture };

            var dateTime = DateTimeOffset.Now;

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
            var converter = new DateTimeOffsetConverter();
            var cmConverter = new System.ComponentModel.DateTimeOffsetConverter();

            var typeConverterOptions = new TypeConverterOptions { CultureInfo = CultureInfo.CurrentCulture };

            var val = (DateTimeOffset)cmConverter.ConvertFromString(string.Empty);
            Assert.AreEqual(DateTimeOffset.MinValue, val);

            val = (DateTimeOffset)converter.ConvertFromString(typeConverterOptions, string.Empty);
            Assert.AreEqual(DateTimeOffset.MinValue, val);

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