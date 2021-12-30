using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Records;

public class Layout : ILayout
{
    private Dictionary<string, int> _offsets;
    private int _slotSize;
    public Layout(ISchema schema)
    {
        Schema = schema;
        _offsets = new Dictionary<string, int>();
        int pos = 4;
        foreach (var field in schema.Fields)
        {
            _offsets.Put(field.Name, pos);
            pos += field.ByteSize;
        }
        _slotSize = pos;
    }

    public Layout(ISchema schema, Dictionary<string, int> offsets, int slotSize)
    {
        Schema = schema;
        _offsets = offsets;
        _slotSize = slotSize;
    }
    public ISchema Schema { get; }

    public int Offset(string fieldName)
    {
        return _offsets[fieldName];
    }

    public int SlotSize => _slotSize;
}
