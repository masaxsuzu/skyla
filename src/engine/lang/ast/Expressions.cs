using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public class FieldExpression : IExpression
{
    public FieldExpression(IField field)
    {
        Field = field;
    }
    public IField Field { get; }
    public ExpressionType Type => ExpressionType.field;

    public bool Equals(IExpression? other)
    {
        if (other is FieldExpression f)
        {
            return Field.Equals(f.Field);
        }
        return false;
    }
}

public class ConstantExpression : IExpression
{
    public ConstantExpression(IConstant constant)
    {
        Constant = constant;
    }
    public IConstant Constant { get; }
    public ExpressionType Type => ExpressionType.constant;

    public bool Equals(IExpression? other)
    {
        if (other is ConstantExpression c)
        {
            return Constant.Equals(c.Constant);
        }
        return false;
    }
}
