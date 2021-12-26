using FlatBuffers;
namespace Skyla.Engine.Interfaces;

public interface IPage
{
    int GetInt(int offset);
    void SetInt(int offset, int number);
    byte[] GetBytes(int offset);
    void SetBytes(int offset, byte[] bytes);
    string GetString(int offset);
    void SetString(int offset, string str);
    int MaxLength(int strLength);
    Stream Contents { get; }
}
