namespace Skyla.Engine.Interfaces;

public interface ICommandPlanner
{
    int Insert(IInsertStatement statement, ITransaction transaction);
    int Modify(IModifyStatement statement, ITransaction transaction);
    int Delete(IDeleteStatement statement, ITransaction transaction);
}
