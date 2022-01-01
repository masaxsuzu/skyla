using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Language.Ast;

public class IntegerConstant : IConstant
{
    public IntegerConstant(int value)
    {
        Value = value;
    }
    public int Value { get; }
    public ConstantType Type => ConstantType.integer;
    public bool Equals(IConstant? other)
    {
        if (other is IntegerConstant i)
        {
            return Value == i.Value;
        }
        return false;
    }
}

public class StringConstant : IConstant
{
    public StringConstant(string value)
    {
        Value = value;
    }
    public string Value { get; }
    public ConstantType Type => ConstantType.varchar;

    public bool Equals(IConstant? other)
    {
        if (other is StringConstant i)
        {
            return Value == i.Value;
        }
        return false;
    }
}
