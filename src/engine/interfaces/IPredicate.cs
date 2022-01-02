namespace Skyla.Engine.Interfaces;

public interface IPredicate : IEquatable<IPredicate>
{
    ITerm[] Terms { get; }
    bool IsSatisfied(IScan s);
    string Format();
}
