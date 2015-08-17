
namespace CsvHelper.Example
{
    using System;

    using CsvHelper.TypeConversion;

    public class CustomTypeTypeConverter : ITypeConverter
    {
        public string ConvertToString(TypeConverterOptions options, object value)
        {
            var obj = (CustomType)value;
            return string.Format("{0}|{1}|{2}", obj.First, obj.Second, obj.Third);
        }

        public object ConvertFromString(TypeConverterOptions options, string text)
        {
            var values = text.Split('|');

            var obj = new CustomType
            {
                First = int.Parse(values[0]),
                Second = int.Parse(values[1]),
                Third = int.Parse(values[2])
            };
            return obj;
        }

        public bool CanConvertFrom(Type type)
        {
            throw new NotImplementedException();
        }

        public bool CanConvertTo(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
