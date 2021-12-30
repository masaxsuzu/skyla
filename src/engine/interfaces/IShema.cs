namespace Skyla.Engine.Interfaces;

public interface ISchema
{
    void AddField(IFieldType type);
    void AddField(string name, ISchema schema);
    void AddAll(ISchema schema);
    IReadOnlyCollection<IFieldType> Fields { get; }
    bool Has(string name);
    IFieldType GetType(string name);
}
