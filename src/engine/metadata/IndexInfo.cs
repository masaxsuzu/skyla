using Skyla.Engine.Indexes;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
namespace Skyla.Engine.Metadata;

public class IndexInfo : IIndexInfo
{
    private readonly string _indexName;
    private readonly string _fieldName;
    private readonly ITransaction _transaction;
    private readonly ISchema _schema;
    private readonly ILayout _layout;
    private readonly IStatInfo _stat;
    public IndexInfo(string indexName, string fieldName, ISchema schema, ITransaction transaction, IStatInfo stat)
    {
        _indexName = indexName;
        _fieldName = fieldName;
        _schema = schema;
        _transaction = transaction;
        _layout = CreateIndexLayout();
        _stat = stat;
    }

    public int Records => _stat.Records;
    public int AccessedBlocks
    {
        get
        {
            var rbp = _transaction.BlockSize / _layout.SlotSize;
            var numOfBlocks = _stat.AccessedBlocks / rbp;
            return HashIndex.SearchCost(numOfBlocks, rbp);
        }
    }
    public int DistinctValues(string fieldName)
    {
        return fieldName == _fieldName ? 1 : _stat.DistinctValues(fieldName);
    }

    public IIndexable Open()
    {
        return new HashIndex(_transaction, _indexName, _layout);
    }
    private ILayout CreateIndexLayout()
    {
        var s = new Schema();
        s.AddField(new IntegerFieldType("block"));
        s.AddField(new IntegerFieldType("id"));

        var type = _schema.GetType(_fieldName);
        switch (type.Type)
        {
            case 8:
                s.AddField(new IntegerFieldType("value"));
                break;
            case 22:
                s.AddField(new StringFieldType("value", type.ByteSize));
                break;
            default:
                throw new Exceptions.EngineException($"unsupported type {type.Type}");
        }

        return new Layout(s);
    }
}
