namespace Skyla.Engine.Interfaces;

public interface ISchemaPlanner
{
    int CreateTable(ICreateTableStatement statement, ITransaction transaction);
    int CreateView(ICreateViewStatement statement, ITransaction transaction);
    int CreateIndex(ICreateIndexStatement statement, ITransaction transaction);
}
