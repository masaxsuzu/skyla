namespace Skyla.Engine.Interfaces;

public interface IFieldType
{
    string Name { get; }
    int Type { get; }
    int ByteSize { get; }
    bool Ok(object value);
}
