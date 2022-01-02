using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Scans;

public class FieldExpression : IExpression
{
    public FieldExpression(IField field)
    {
        Field = field;
    }
    public IField Field { get; }
    public ExpressionType Type => ExpressionType.field;

    public IConstant Evaluate(IScan s)
    {
        return s.Get(Field.Identifier);
    }


    public bool Equals(IExpression? other)
    {
        if (other is FieldExpression f)
        {
            return Field.Equals(f.Field);
        }
        return false;
    }

    public string Format()
    {
        return Field.Identifier;
    }
}

public class ConstantExpression : IExpression
{
    public ConstantExpression(IConstant constant)
    {
        Constant = constant;
    }
    public IConstant Constant { get; }

    public IConstant Evaluate(IScan s) => Constant;
    public ExpressionType Type => ExpressionType.constant;

    public bool Equals(IExpression? other)
    {
        if (other is ConstantExpression c)
        {
            return Constant.Equals(c.Constant);
        }
        return false;
    }

    public string Format()
    {
        return Constant.Format();
    }
}
