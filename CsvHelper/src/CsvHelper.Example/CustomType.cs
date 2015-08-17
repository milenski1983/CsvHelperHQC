namespace CsvHelper.Example
{
    using System.Web.Script.Serialization;

    public class CustomType
    {
        public int First { get; set; }

        public int Second { get; set; }

        public int Third { get; set; }

        public override string ToString()
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(this);
        }
    }
}
