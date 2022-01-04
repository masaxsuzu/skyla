using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
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

    public int ReductionFactor(IPlan p)
    {
        int f = 1;
        foreach (var term in _term)
        {
            f *= term.ReductionFactor(p);
        }
        return f;
    }

    public IPredicate SelectSubPredicate(ISchema schema)
    {
        var p = new Predicate();
        foreach (var term in _term)
        {
            if (term.AppliedTo(schema))
            {
                p._term.Add(term);
            }
        }
        return p;
    }

    public IPredicate JoinSubPredicate(ISchema schema1, ISchema schema2)
    {
        var p = new Predicate();
        var s = new Schema();
        s.AddAll(schema1);
        s.AddAll(schema2);

        foreach (var term in _term)
        {
            if (!term.AppliedTo(schema1) && !term.AppliedTo(schema2) && term.AppliedTo(s))
            {
                p._term.Add(term);
            }
        }
        return p;
    }

    public IConstant? EquatesWitConstant(string name)
    {
        foreach (var term in _term)
        {
            var c = term.EquatesWitConstant(name);
            if (c != null)
            {
                return c;
            }
        }
        return null;
    }

    public string? EquatesWithField(string name)
    {
        foreach (var term in _term)
        {
            var f = term.EquatesWithField(name);
            if (f != null)
            {
                return f;
            }
        }
        return null;
    }
}
