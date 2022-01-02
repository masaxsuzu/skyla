using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Scans;

public class Predicate : IPredicate
{
    private readonly List<ITerm> _term = new List<ITerm>();
    public Predicate()
    {
    }
    public Predicate(ITerm[] terms)
    {
        _term.AddRange(terms);
    }

    public ITerm[] Terms => _term.ToArray();

    public Predicate With(Predicate p)
    {
        _term.AddRange(p.Terms);
        return this;
    }

    public bool IsSatisfied(IScan s)
    {
        foreach (var t in _term)
        {
            if (!t.IsSatisfied(s))
            {
                return false;
            }
        }
        return true;
    }

    public bool Equals(IPredicate? other)
    {
        if (other == null)
        {
            return false;
        }

        if (Terms.Length != other.Terms.Length)
        {
            return false;
        }

        for (int i = 0; i < Terms.Length; i++)
        {
            if (!Terms[i].Equals(other.Terms[i]))
            {
                return false;
            }
        }

        return true;
    }

    public string Format()
    {
        var sb = new System.Text.StringBuilder("");
        sb.AppendJoin(" and ", _term.Select(t => t.Format()));
        return sb.ToString();
    }
}
