using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Scans;

#pragma warning disable CS8602
public class IntegerConstant : IConstant
{
    public IntegerConstant(int value)
    {
        Value = value;
    }
    public int Value { get; }
    public ConstantType Type => ConstantType.integer;

    public int CompareTo(IConstant? other)
    {
        if (other == null) return -1;
        if (Type != other.Type) return Type - other.Type;
        return Value.CompareTo((other as IntegerConstant).Value);
    }

    public bool Equals(IConstant? other)
    {
        if (other is IntegerConstant i)
        {
            return Value == i.Value;
        }
        return false;
    }
    public string Format()
    {
        return Value.ToString();
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
    public string Format()
    {
        return $"\'{Value.ToString()}\'";
    }
    public int CompareTo(IConstant? other)
    {
        if (other == null) return -1;
        if (Type != other.Type) return Type - other.Type;
        return Value.CompareTo((other as StringConstant).Value);
    }
}
