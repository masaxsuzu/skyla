namespace Skyla.Engine.Interfaces;

public interface ITableScan
{
    void Close();
    bool HasField(string name);
    void BeforeFirst();
    bool Next();
    void MoveTo(IRecordId r);
    void Insert();

    int GetInt(string fieldName);
    string GetString(string fieldName);
    void SetInt(string fieldName, int value);
    void SetString(string fieldName, string value);

    IRecordId Record { get; }
    void Delete();
}
