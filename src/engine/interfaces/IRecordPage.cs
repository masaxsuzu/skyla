namespace Skyla.Engine.Interfaces;

public interface IRecordPage
{
    IBlockId Block { get; }
    int GetInt(int slot, string fieldName);
    string GetString(int slot, string fieldName);
    void SetInt(int slot, string fieldName, int value);
    void SetString(int slot, string fieldName, string value);
    void Format();
    void Delete(int slot);
    int NextAfter(int slot);
    int InsertAfter(int slot);
}
