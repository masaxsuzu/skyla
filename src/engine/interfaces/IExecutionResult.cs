namespace Skyla.Engine.Interfaces;

public interface IExecutionResult
{
    int AffectedRows { get; }
    string Message { get; }
}
