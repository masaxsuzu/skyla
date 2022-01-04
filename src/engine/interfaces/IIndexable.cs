namespace Skyla.Engine.Interfaces;

public interface IIndexable
{
    void BeforeFirst(IConstant constant);
    bool Next();
    IRecordId Record { get; }
    void Insert(IConstant data, IRecordId record);
    void Delete(IConstant data, IRecordId record);
    void Close();
}
