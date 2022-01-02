using System.Data;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Plans;

public class TablePlan : IWritablePlan
{
    private readonly ITransaction _transaction;
    private readonly string _tableName;
    private readonly IMetadataManager _metadata;
    private readonly ILayout _layout;
    private readonly IStatInfo _stat;

    public TablePlan(ITransaction transaction, string tableName, IMetadataManager metadata)
    {
        _transaction = transaction;
        _tableName = tableName;
        _metadata = metadata;
        _layout = metadata.GetLayout(tableName, transaction);
        _stat = metadata.GetStatInfo(tableName, _layout, transaction);
    }
    public int AccessedBlocks => _stat.AccessedBlocks;
    public int Records => _stat.Records;

    public ISchema Schema => _layout.Schema;

    public int DistinctValues(string fieldName)
    {
        return _stat.DistinctValues(fieldName);
    }

    public IScan Open()
    {
        return Update();
    }

    public IUpdateScan Update()
    {
        return new TableScan(_transaction, _tableName, _layout);
    }
}
