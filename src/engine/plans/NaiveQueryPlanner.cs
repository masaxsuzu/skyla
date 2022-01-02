using System.Data;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
namespace Skyla.Engine.Plans;

public class NaiveQueryPlanner : IQueryPlanner
{
    private IParser _parser;
    private IMetadataManager _metadata;
    public NaiveQueryPlanner(IParser parser, IMetadataManager metadata)
    {
        _metadata = metadata;
        _parser = parser;
    }
    public IPlan Create(IQueryStatement query, ITransaction transaction)
    {
        var plans = new List<IPlan>();
        foreach (var tableName in query.TableNames)
        {
            var view = _metadata.GetViewDefinition(tableName, transaction);
            if (view != "")
            {
                var q = _parser.ParseQuery(view);
                plans.Add(Create(q, transaction));
            }
            else
            {
                plans.Add(new TablePlan(transaction, tableName, _metadata));
            }
        }
        IPlan p = plans[0];
        plans.RemoveAt(0);
        foreach (var next in plans)
        {
            p = new ProductPlan(p, next);
        }

        p = new SelectPlan(p, query.Predicate);

        return new ProjectPlan(p, query.ColumnNames);
    }
}
