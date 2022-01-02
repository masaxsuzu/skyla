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
}
