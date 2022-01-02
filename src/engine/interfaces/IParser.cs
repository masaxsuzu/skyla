namespace Skyla.Engine.Interfaces;

public interface IParser
{
    (StatementType, IStatement) Parse(string sql);
    IQueryStatement ParseQuery(string sql);
    ICreateTableStatement ParseCreateTable(string sql);
    ICreateViewStatement ParseCreateView(string sql);
    ICreateIndexStatement ParseCreateIndex(string sql);
    IInsertStatement ParseInsert(string sql);
    IModifyStatement ParseModify(string sql);
    IDeleteStatement ParseDelete(string sql);

}
