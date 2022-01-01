namespace Skyla.Engine.Interfaces;

public interface IPredicate : IEquatable<IPredicate>
{
    ITerm[] Terms { get; }
}
