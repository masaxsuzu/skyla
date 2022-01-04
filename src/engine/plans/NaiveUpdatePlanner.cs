using System.Data;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
namespace Skyla.Engine.Plans;

public class NaiveUpdatePlanner : ISchemaPlanner, ICommandPlanner
{
    private readonly IMetadataManager _metadata;
    public NaiveUpdatePlanner(IMetadataManager metadataManager)
    {
        _metadata = metadataManager;
    }

    public int CreateIndex(ICreateIndexStatement statement, ITransaction transaction)
    {
        _metadata.CreateIndex(statement.IndexName, statement.TableName, statement.ColumnName, transaction);
        return 0;
    }

    public int CreateTable(ICreateTableStatement statement, ITransaction transaction)
    {
        _metadata.CreateTable(statement.TableName, ToSchema(statement.Types), transaction);
        return 0;
    }

    public int CreateView(ICreateViewStatement statement, ITransaction transaction)
    {
        _metadata.CreateView(statement.ViewName, statement.ViewDefinition, transaction);
        return 0;
    }

    public int Delete(IDeleteStatement statement, ITransaction transaction)
    {
        IWritablePlan plan = new TablePlan(transaction, statement.TableName, _metadata);
        plan = new SelectPlan(plan, plan, statement.Predicate);
        var s = plan.Update();
        int c = 0;
        while (s.Next())
        {
            s.Delete();
            c++;
        }
        s.Close();
        return c;
    }

    public int Modify(IModifyStatement statement, ITransaction transaction)
    {
        IWritablePlan plan = new TablePlan(transaction, statement.TableName, _metadata);
        plan = new SelectPlan(plan, plan, statement.Predicate);
        var s = plan.Update();
        int c = 0;
        while (s.Next())
        {
            s.Set(statement.ColumnName, statement.Expression.Evaluate(s));
            c++;
        }
        s.Close();
        return c;
    }

    public int Insert(IInsertStatement statement, ITransaction transaction)
    {
        IWritablePlan plan = new TablePlan(transaction, statement.TableName, _metadata);
        var s = plan.Update();
        s.Insert();

        var columns = statement.ColumnNames;
        var values = statement.Values;

        for (int i = 0; i < columns.Length; i++)
        {
            var v = values[i];
            var col = columns[i];
            s.Set(col, v);
        }

        s.Close();
        return 1;
    }

    private ISchema ToSchema(TypeDefinition[] types)
    {
        var s = new Schema();
        foreach (var type in types)
        {
            switch (type.Type)
            {
                case DefineType.integer:
                    s.AddField(new IntegerFieldType(type.Identifier));
                    break;
                case DefineType.varchar:
                    s.AddField(new StringFieldType(type.Identifier, type.Length));
                    break;
                default:
                    break;
            }
        }
        return s;
    }
}
