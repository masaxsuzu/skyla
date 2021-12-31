using System.Data;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Records;

public class IntegerFieldType : IFieldType
{
    public IntegerFieldType(string name)
    {
        Name = name;
        ByteSize = 4;
        Type = 8;
    }
    public string Name { get; }
    public int Type { get; }

    public int ByteSize { get; }

    public bool Ok(object value)
    {
        return value is int;
    }
}
