using System.Data;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
namespace Skyla.Engine.Plans;

public class IndexedUpdatePlanner : ICommandPlanner
{
    private readonly IMetadataManager _metadata;
    public IndexedUpdatePlanner(IMetadataManager metadataManager)
    {
        _metadata = metadataManager;
    }

    public int Insert(IInsertStatement statement, ITransaction transaction)
    {
        var tableName = statement.TableName;
        var plan = new TablePlan(transaction, tableName, _metadata);
        var scan = plan.Update();
        scan.Insert();
        var record = scan.Record;
        var indexes = _metadata.GetIndexInfo(tableName, transaction);

        for (int i = 0; i < statement.ColumnNames.Length; i++)
        {
            var f = statement.ColumnNames[i];
            var v = statement.Values[i];
            scan.Set(f, v);
            if (indexes.ContainsKey(f))
            {
                var index = indexes[f].Open();
                index.Insert(v, record);
                index.Close();
            }
        }
        scan.Close();
        return 1;
    }

    public int Delete(IDeleteStatement statement, ITransaction transaction)
    {
        var tableName = statement.TableName;
        IWritablePlan plan = new TablePlan(transaction, tableName, _metadata);
        plan = new SelectPlan(plan, plan, statement.Predicate);
        var indexes = _metadata.GetIndexInfo(tableName, transaction);
        var scan = plan.Update();

        int count = 0;
        while (scan.Next())
        {
            var record = scan.Record;
            foreach (var field in indexes.Keys)
            {
                var value = scan.Get(field);
                var index = indexes[field].Open();
                index.Delete(value, record);
                index.Close();
            }
            scan.Delete();
            count++;
        }
        scan.Close();
        return count;
    }

    public int Modify(IModifyStatement statement, ITransaction transaction)
    {
        var tableName = statement.TableName;
        var fieldName = statement.ColumnName;
        IWritablePlan plan = new TablePlan(transaction, tableName, _metadata);
        plan = new SelectPlan(plan, plan, statement.Predicate);
        var indexes = _metadata.GetIndexInfo(tableName, transaction);

        var index = indexes.ContainsKey(fieldName) ? indexes[fieldName].Open() : null;
        var scan = plan.Update();

        int count = 0;
        while (scan.Next())
        {
            var newValue = statement.Expression.Evaluate(scan);
            var oldValue = scan.Get(fieldName);
            scan.Set(fieldName, newValue);
            if (index != null)
            {
                var record = scan.Record;
                index.Delete(oldValue, record);
                index.Insert(newValue, record);
            }
            count++;
        }
        if (index != null)
        {
            index.Close();
        }
        scan.Close();
        return count;
    }
}
