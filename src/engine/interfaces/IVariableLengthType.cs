using FlatBuffers;
namespace Skyla.Engine.Interfaces;

public interface IVariableLengthType<T>
{
    int Length(T value);
    byte[] Encode(T value);
    T Decode(byte[] bytes);
}
