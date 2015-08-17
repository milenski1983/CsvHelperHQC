
namespace CsvHelper.Example
{
    using System;
    using System.Web.Script.Serialization;

    public class CustomObject
    {
        public CustomType CustomTypeColumn { get; set; }

        public Guid GuidColumn { get; set; }

        public int IntColumn { get; set; }

        public string StringColumn { get; set; }

        public override string ToString()
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(this);
        }
    }
}
