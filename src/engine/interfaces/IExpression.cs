namespace Skyla.Engine.Interfaces;

public interface IExpression : IEquatable<IExpression>
{
    ExpressionType Type { get; }
    IConstant Evaluate(IScan s);
    string Format();
}
