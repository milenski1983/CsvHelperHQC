﻿namespace CsvHelper.Tests.ExcelCompatibility
{
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using CsvHelper.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LeadingZeroTests
    {
        [TestMethod]
        public void WriteTest()
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer))
            {
                csv.Configuration.UseExcelLeadingZerosFormatForNumerics = true;

                var record = new Simple { Id = 1, Name = "09010" };

                csv.WriteRecord(record);

                writer.Flush();
                stream.Position = 0;

                var text = reader.ReadToEnd();
                Assert.AreEqual("1,=\"09010\"\r\n", text);
            }
        }

        [TestMethod]
        public void ParseValid1FieldTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                writer.Write("=\"01\"\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);
            }
        }

        [TestMethod]
        public void ParseValid1FieldNoLineEndingTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                writer.Write("=\"01\"");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);
            }
        }

        [TestMethod]
        public void ParseValid2FieldsTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                writer.Write("=\"01\",=\"02\"\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);
                Assert.AreEqual("02", row[1]);
            }
        }

        [TestMethod]
        public void ParseValid2FieldsNoLineEndingTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                writer.Write("=\"01\",=\"02\"");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);
                Assert.AreEqual("02", row[1]);
            }
        }

        [TestMethod]
        public void ParseValid1Field2RowsTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                writer.Write("=\"01\"\r\n");
                writer.Write("=\"02\"\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);

                row = parser.Read();

                Assert.AreEqual("02", row[0]);
            }
        }

        [TestMethod]
        public void ParseValid1Field2RowsNoLineEndingTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                writer.Write("=\"01\"\r\n");
                writer.Write("=\"02\"");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);

                row = parser.Read();

                Assert.AreEqual("02", row[0]);
            }
        }

        [TestMethod]
        public void ParseValid2Fields2RowsTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                writer.Write("=\"01\",=\"02\"\r\n");
                writer.Write("=\"03\",=\"04\"\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);
                Assert.AreEqual("02", row[1]);

                row = parser.Read();

                Assert.AreEqual("03", row[0]);
                Assert.AreEqual("04", row[1]);
            }
        }

        [TestMethod]
        public void ParseValid2Fields2RowsNoLineEndingTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                writer.Write("=\"01\",=\"02\"\r\n");
                writer.Write("=\"03\",=\"04\"");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);
                Assert.AreEqual("02", row[1]);

                row = parser.Read();

                Assert.AreEqual("03", row[0]);
                Assert.AreEqual("04", row[1]);
            }
        }

        [TestMethod]
        public void ParseInvalid1FieldNoQuotesTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                writer.Write("=01\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("=01", row[0]);
            }
        }

        [TestMethod]
        public void ParseInvalid1FieldQuotedTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                writer.Write("\"=\"\"01\"\"\"\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("=\"01\"", row[0]);
            }
        }

        [TestMethod]
        public void ParseInvalidSpace1FieldSpaceBeforeEqualsTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                writer.Write(" =\"01\"\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual(" =\"01\"", row[0]);
            }
        }

        [TestMethod]
        public void ParseInvalid1FieldNotAllNumbersTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                writer.Write("=\"0a1\"\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("=\"0a1\"", row[0]);
            }
        }

        [TestMethod]
        public void ParseValid1FieldBufferOf1Test()
        {
            var config = new CsvConfiguration { BufferSize = 1 };

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader, config))
            {
                writer.Write("=\"01\"\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);
            }
        }

        [TestMethod]
        public void ParseValid1FieldNoLineEndingBufferOf1Test()
        {
            var config = new CsvConfiguration { BufferSize = 1 };

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader, config))
            {
                writer.Write("=\"01\"");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);
            }
        }

        [TestMethod]
        public void ParseValid2FieldsBufferOf1Test()
        {
            var config = new CsvConfiguration { BufferSize = 1 };

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader, config))
            {
                writer.Write("=\"01\",=\"02\"\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);
                Assert.AreEqual("02", row[1]);
            }
        }

        [TestMethod]
        public void ParseValid2FieldsNoLineEndingBufferOf1Test()
        {
            var config = new CsvConfiguration { BufferSize = 1 };

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader, config))
            {
                writer.Write("=\"01\",=\"02\"");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);
                Assert.AreEqual("02", row[1]);
            }
        }

        [TestMethod]
        public void ParseValid1Field2RowsBufferOf1Test()
        {
            var config = new CsvConfiguration { BufferSize = 1 };

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader, config))
            {
                writer.Write("=\"01\"\r\n");
                writer.Write("=\"02\"\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);

                row = parser.Read();

                Assert.AreEqual("02", row[0]);
            }
        }

        [TestMethod]
        public void ParseValid1Field2RowsNoLineEndingBufferOf1Test()
        {
            var config = new CsvConfiguration { BufferSize = 1 };

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader, config))
            {
                writer.Write("=\"01\"\r\n");
                writer.Write("=\"02\"");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);

                row = parser.Read();

                Assert.AreEqual("02", row[0]);
            }
        }

        [TestMethod]
        public void ParseValid2Fields2RowsNoLineEndingBufferOf1Test()
        {
            var config = new CsvConfiguration { BufferSize = 1 };

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader, config))
            {
                writer.Write("=\"01\",=\"02\"\r\n");
                writer.Write("=\"03\",=\"04\"");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("01", row[0]);
                Assert.AreEqual("02", row[1]);

                row = parser.Read();

                Assert.AreEqual("03", row[0]);
                Assert.AreEqual("04", row[1]);
            }
        }

        [TestMethod]
        public void ParseInvalid1FieldBufferOf1Test()
        {
            var config = new CsvConfiguration { BufferSize = 1 };

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader, config))
            {
                writer.Write("=01\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("=01", row[0]);
            }
        }

        [TestMethod]
        public void ParseInvalid2FieldBufferOf1Test()
        {
            var config = new CsvConfiguration { BufferSize = 1 };

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader, config))
            {
                writer.Write("=01,=02\r\n");
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                var row = parser.Read();

                Assert.AreEqual("=01", row[0]);
                Assert.AreEqual("=02", row[1]);
            }
        }

        [TestMethod]
        public void ParseValid1FieldBytePositionTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                var csv = "=\"01\"\r\n";
                writer.Write(csv);
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                parser.Configuration.CountBytes = true;
                var row = parser.Read();

                Assert.AreEqual(Encoding.Default.GetByteCount(csv), parser.BytePosition);
            }
        }

        [TestMethod]
        public void ParseValid2FieldsBytePositionTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                var csv = "=\"01\",=\"02\"\r\n";
                writer.Write(csv);
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                parser.Configuration.CountBytes = true;
                var row = parser.Read();

                Assert.AreEqual(Encoding.Default.GetByteCount(csv), parser.BytePosition);
            }
        }

        [TestMethod]
        public void ParseValid2Fields2RowsBytePositionTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new CsvParser(reader))
            {
                var csvRow1 = "=\"01\",=\"02\"\r\n";
                var csvRow2 = "=\"03\",=\"04\"";
                writer.Write(csvRow1 + csvRow2);
                writer.Flush();
                stream.Position = 0;

                parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                parser.Configuration.HasHeaderRecord = false;
                parser.Configuration.CountBytes = true;

                parser.Read();

                Assert.AreEqual(Encoding.Default.GetByteCount(csvRow1), parser.BytePosition);

                parser.Read();

                Assert.AreEqual(Encoding.Default.GetByteCount(csvRow1 + csvRow2), parser.BytePosition);
            }
        }

        [TestMethod]
        public void ParseValid2Fields2RowsBytePositionDifferentCultureTest()
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("uk-UA");

                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream))
                using (var reader = new StreamReader(stream))
                using (var parser = new CsvParser(reader))
                {
                    var csvRow1 = "=\"01\",=\"02\"\r\n";
                    var csvRow2 = "=\"03\",=\"04\"";
                    writer.Write(csvRow1 + csvRow2);
                    writer.Flush();
                    stream.Position = 0;

                    parser.Configuration.UseExcelLeadingZerosFormatForNumerics = true;
                    parser.Configuration.HasHeaderRecord = false;
                    parser.Configuration.CountBytes = true;
                    parser.Configuration.CultureInfo = Thread.CurrentThread.CurrentCulture;

                    parser.Read();

                    Assert.AreEqual(Encoding.Default.GetByteCount(csvRow1), parser.BytePosition);

                    parser.Read();

                    Assert.AreEqual(Encoding.Default.GetByteCount(csvRow1 + csvRow2), parser.BytePosition);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        private class Simple
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}