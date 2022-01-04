namespace Skyla.Engine.Interfaces;

public interface IPredicate : IEquatable<IPredicate>
{
    ITerm[] Terms { get; }
    bool IsSatisfied(IScan s);

    int ReductionFactor(IPlan p);
    IPredicate SelectSubPredicate(ISchema schema);
    IPredicate JoinSubPredicate(ISchema schema1, ISchema schema2);
    IConstant? EquatesWitConstant(string name);
    string? EquatesWithField(string name);
    string Format();
}
