namespace Skyla.Engine.Interfaces;

public interface ITerm : IEquatable<ITerm>
{
    IExpression Left { get; }
    IExpression Right { get; }
    bool IsSatisfied(IScan s);
    bool AppliedTo(ISchema schema);
    int ReductionFactor(IPlan p);
    IConstant? EquatesWitConstant(string name);
    string? EquatesWithField(string name);
    string Format();
}
