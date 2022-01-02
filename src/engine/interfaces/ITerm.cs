namespace Skyla.Engine.Interfaces;

public interface ITerm : IEquatable<ITerm>
{
    IExpression Left { get; }
    IExpression Right { get; }
    bool IsSatisfied(IScan s);
    string Format();
}
