using FlatBuffers;
namespace Skyla.Engine.Interfaces;

public interface IRecoveryManager
{
    void Commit();
    void Rollback();
    void Recover();
    int SetInt(IBuffer buffer, int offset, int value);
    int SetString(IBuffer buffer, int offset, string value);
}
