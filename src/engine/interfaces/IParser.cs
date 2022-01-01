namespace Skyla.Engine.Interfaces;

public interface IParser
{
    IStatement Parse(string sql);
    IQueryStatement ParseQuery(string sql);
    ICreateTableStatement ParseCreateTable(string sql);
    ICreateViewStatement ParseCreateView(string sql);
    ICreateIndexStatement ParseCreateIndex(string sql);
    IInsertStatement ParseInsertStatement(string sql);
    IModifyStatement ParseModify(string sql);
    IDeleteStatement ParseDelete(string sql);

}
