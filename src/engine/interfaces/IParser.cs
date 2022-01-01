namespace Skyla.Engine.Interfaces;

public interface IParser
{
    IPredicate Parse(string sql);
}
