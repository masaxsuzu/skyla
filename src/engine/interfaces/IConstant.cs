namespace Skyla.Engine.Interfaces;

public interface IConstant : IEquatable<IConstant>, IComparable<IConstant>
{
    ConstantType Type { get; }
    string Format();
}
