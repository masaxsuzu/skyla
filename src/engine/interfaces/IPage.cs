using FlatBuffers;
namespace Skyla.Engine.Interfaces;

public interface IPage
{
    T Get<T>(int offset, IFixedLengthType<T> fixedLength);
    void Set<T>(int offset, IFixedLengthType<T> fixedLength, T value);
    T Get<T>(int offset, IVariableLengthType<T> variableLength);
    void Set<T>(int offset, IVariableLengthType<T> variableLength, T value);
    int Length<T>(IFixedLengthType<T> fixedLength);
    int Length<T>(IVariableLengthType<T> variableLength, T value);
    Stream Contents { get; }
}
