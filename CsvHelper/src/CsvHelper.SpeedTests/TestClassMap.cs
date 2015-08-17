namespace CsvHelper.SpeedTests
{
    using CsvHelper.Configuration;

    public sealed class TestClassMap : CsvClassMap<TestClass>
    {
        public TestClassMap()
        {
            this.Map(m => m.IntColumn).Name("Int Column");
            this.Map(m => m.StringColumn).Name("String Column");
            this.Map(m => m.DateColumn).Name("Date Column");
            this.Map(m => m.BoolColumn).Name("Bool Column");
            this.Map(m => m.GuidColumn).Name("Guid Column");
        }
    }
}
