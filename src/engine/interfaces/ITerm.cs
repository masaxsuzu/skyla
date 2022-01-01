namespace Skyla.Engine.Interfaces;

public interface ITerm : IEquatable<ITerm>
{
    IExpression Left { get; }
    IExpression Right { get; }
}
