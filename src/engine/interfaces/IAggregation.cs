using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Interfaces;

public interface IAggregation
{
    void ProcessFirst(IScan s);
    void ProcessNext(IScan s);
    string FieldName { get; }
    IConstant? Value { get; }
}
