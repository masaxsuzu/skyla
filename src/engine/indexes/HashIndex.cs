using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Indexes;

public class HashIndex : IIndexable
{
    public HashIndex(ITransaction transaction, string indexName, ILayout layout)
    {

    }
    public static int SearchCost(int numOfBlocks, int recordsPerBlock)
    {
        return 0;
    }
}
