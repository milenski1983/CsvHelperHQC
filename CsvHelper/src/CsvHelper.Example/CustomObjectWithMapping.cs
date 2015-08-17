
namespace CsvHelper.Example
{
    using System;
    using System.Web.Script.Serialization;

    public class CustomObjectWithMapping
    {
        public CustomType CustomTypeColumn { get; set; }

        public Guid GuidColumn { get; set; }

        public int IntColumn { get; set; }

        public string StringColumn { get; set; }

        public string IgnoredColumn { get; set; }

        public override string ToString()
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(this);
        }
    }
}
