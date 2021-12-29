namespace Skyla.Engine.Interfaces;

public interface ILogRecord
{
    int Operation { get; }
    int TransactionNumber { get; }
    void Undo(ITransaction transaction);
    int WriteToLog(ILogManager logManager);
}
