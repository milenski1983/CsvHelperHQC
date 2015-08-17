
namespace CsvHelper.Example
{
    using CsvHelper.Configuration;

    public sealed class CustomObjectWithMappingMap : CsvClassMap<CustomObjectWithMapping>
    {
        public CustomObjectWithMappingMap()
        {
            this.Map(m => m.CustomTypeColumn)
                .Name("Custom Type Column")
                .Index(3)
                .TypeConverter<CustomTypeTypeConverter>();
            this.Map(m => m.GuidColumn).Name("Guid Column").Index(2);
            this.Map(m => m.IntColumn).Name("Int Column").Index(1);
            this.Map(m => m.StringColumn).Name("String Column").Index(0);
        }
    }
}
