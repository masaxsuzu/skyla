namespace Skyla.Engine.Interfaces;

public interface IScan
{
    void BeforeFirst();
    bool Next();
    IConstant Get(string fieldName);
    int GetInt(string fieldName);
    string GetString(string fieldName);
    bool HasField(string fieldName);
    void Close();
}

public interface IUpdateScan : IScan
{
    void Set(string fieldName, IConstant value);

    void SetInt(string fieldName, int value);
    void SetString(string fieldName, string value);
    void Insert();
    void Delete();
    IRecordId Record { get; }
    void MoveTo(IRecordId record);
}
