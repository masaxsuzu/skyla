using System.Data;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Plans;

public class HeuristicQueryPlanner : IQueryPlanner
{
    private List<TablePlanner> _planners = new List<TablePlanner>();
    private IMetadataManager _metadata;
    public HeuristicQueryPlanner(IMetadataManager metadata)
    {
        _metadata = metadata;
    }
    public IPlan Create(IQueryStatement query, ITransaction transaction)
    {
#pragma warning disable CS8604
        foreach (var tableName in query.TableNames)
        {
            var planner = new TablePlanner(tableName, query.Predicate, transaction, _metadata);
            _planners.Add(planner);
        }
        var currentPlan = GetLowestSelectPlan();

        while (_planners.Count != 0)
        {
            var p = GetLowestJoinPlan(currentPlan);
            if (p != null)
            {
                currentPlan = p;
            }
            else
            {
                currentPlan = GetLowestProductPlan(currentPlan);
            }
        }

        return new ProjectPlan(currentPlan, query.ColumnNames);
    }

    private IPlan? GetLowestSelectPlan()
    {
        TablePlanner? bestPlanner = null;
        IPlan? bestPlan = null;
        foreach (var tp in _planners)
        {
            var plan = tp.MakeSelectPlan();
            if (bestPlan == null || plan.Records < bestPlan.Records)
            {
                bestPlanner = tp;
                bestPlan = plan;
            }
        }
        if (bestPlanner != null && bestPlan != null)
        {
            _planners.Remove(bestPlanner);
        }
        return bestPlan;
    }

    private IPlan? GetLowestJoinPlan(IPlan current)
    {
        TablePlanner? bestPlanner = null;
        IPlan? bestPlan = null;
        foreach (var tp in _planners)
        {
            var plan = tp.MakeJoinPlan(current);
            if (plan != null && (bestPlan == null || plan.Records < bestPlan.Records))
            {
                bestPlanner = tp;
                bestPlan = plan;
            }
        }
        if (bestPlanner != null && bestPlan != null)
        {
            _planners.Remove(bestPlanner);
        }
        return bestPlan;
    }
    private IPlan? GetLowestProductPlan(IPlan current)
    {
        TablePlanner? bestPlanner = null;
        IPlan? bestPlan = null;
        foreach (var tp in _planners)
        {
            var plan = tp.MakeProductPlan(current);
            if (bestPlan == null || plan.Records < bestPlan.Records)
            {
                bestPlanner = tp;
                bestPlan = plan;
            }
        }
        if (bestPlanner != null && bestPlan != null)
        {
            _planners.Remove(bestPlanner);
        }
        return bestPlan;
    }
}
