namespace Skyla.Engine.Interfaces;

public interface IQueryPlanner
{
    IPlan Create(IQueryStatement query, ITransaction transaction);
}
