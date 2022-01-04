namespace Skyla.Engine.Interfaces;

public interface IExpression : IEquatable<IExpression>
{
    ExpressionType Type { get; }
    IConstant Evaluate(IScan s);
    IConstant? AsConstant { get; }
    string? AsFieldName { get; }
    bool AppliesTo(ISchema schema);
    string Format();
}
