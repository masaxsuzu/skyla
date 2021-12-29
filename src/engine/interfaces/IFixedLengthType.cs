using FlatBuffers;
namespace Skyla.Engine.Interfaces;

public interface IFixedLengthType<T>
{
    int Length { get; }
    byte[] Encode(T value);
    T Decode(byte[] bytes);
}
