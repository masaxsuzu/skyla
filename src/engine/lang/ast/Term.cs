using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;
public class Term : ITerm
{
    public Term(IExpression left, IExpression right)
    {
        Left = left;
        Right = right;
    }
    public IExpression Left { get; }
    public IExpression Right { get; }
    public bool Equals(ITerm? other)
    {
        if (other == null) return false;

        var l = Left.Equals(other.Left);
        var r = Right.Equals(other.Right);
        return l && r;
    }
}
