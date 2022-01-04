using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Scans;
public class Term : ITerm
{
    public Term(IExpression left, IExpression right)
    {
        Left = left;
        Right = right;
    }
    public IExpression Left { get; }
    public IExpression Right { get; }

    public bool IsSatisfied(IScan s)
    {
        var l = Left.Evaluate(s);
        var r = Right.Evaluate(s);

        return l.Equals(r);
    }

    public bool Equals(ITerm? other)
    {
        if (other == null) return false;

        var l = Left.Equals(other.Left);
        var r = Right.Equals(other.Right);
        return l && r;
    }

    public string Format()
    {
        return $"{Left.Format()} = {Right.Format()}";
    }

    public bool AppliedTo(ISchema schema)
    {
        return Left.AppliesTo(schema) && Right.AppliesTo(schema);
    }
#pragma warning disable CS8604

    public int ReductionFactor(IPlan p)
    {
        var leftName = Left.AsFieldName;
        var rightName = Right.AsFieldName;

        if (Left.Type == ExpressionType.field && Right.Type == ExpressionType.field)
        {
            return Math.Max(p.DistinctValues(leftName), p.DistinctValues(rightName));
        }

        if (Left.Type == ExpressionType.field)
        {
            return p.DistinctValues(leftName);
        }
        if (Right.Type == ExpressionType.field)
        {
            return p.DistinctValues(rightName);
        }
        if (Left.Equals(Right))
        {
            return 1;
        }
        else
        {
            return Int32.MaxValue;
        }

    }

    public IConstant? EquatesWitConstant(string name)
    {
        if (Left.Type == ExpressionType.field && Left.AsFieldName == name && Right.Type != ExpressionType.field)
        {
            return Right.AsConstant;
        }
        if (Right.Type == ExpressionType.field && Right.AsFieldName == name && Left.Type != ExpressionType.field)
        {
            return Left.AsConstant;
        }
        return null;
    }

    public string? EquatesWithField(string name)
    {
        if (Left.Type == ExpressionType.field && Left.AsFieldName == name && Right.Type == ExpressionType.field)
        {
            return Right.AsFieldName;
        }
        if (Right.Type == ExpressionType.field && Right.AsFieldName == name && Left.Type == ExpressionType.field)
        {
            return Left.AsFieldName;
        }
        return null;
    }
}
