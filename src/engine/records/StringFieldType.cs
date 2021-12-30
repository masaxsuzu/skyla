using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Records;

public class StringFieldType : IFieldType
{
    public StringFieldType(string name, int strLength)
    {
        Name = name;
        // Due to own (generic) design choice,
        // any actual (string) value is required to compute length.
        var dummyString = Enumerable.Range(0, strLength)
            .Select(_ => "s")
            .Aggregate(new System.Text.StringBuilder(), (sb, s) => sb.Append(s))
            .ToString();
        ByteSize = new StringType().Length(dummyString);
    }
    public string Name { get; }

    public int ByteSize { get; }

    public bool Ok(object value)
    {
        return value is string;
    }
}
