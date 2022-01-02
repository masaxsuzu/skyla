namespace Skyla.Engine.Interfaces;

public interface IConstant : IEquatable<IConstant>
{
    ConstantType Type { get; }
    string Format();
}
