using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Scans;

public class Field : IField
{
    public Field(string id)
    {
        Identifier = id;
    }
    public string Identifier { get; }
    public bool Equals(IField? other)
    {
        if (other == null) return false;
        return Identifier == other.Identifier;
    }
}

