namespace Skyla.Engine.Exceptions;
public class EngineException : Exception
{
    public EngineException(string message, Exception innerException) : base(message, innerException) { }
}
