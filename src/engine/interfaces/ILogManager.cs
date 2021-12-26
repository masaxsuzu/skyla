using System.Collections.Generic;
namespace Skyla.Engine.Interfaces;

public interface ILogManager
{
    int Append(byte[] record);
    void Flush(int lsNumber);
    IEnumerable<byte[]> AsEnumerable();
}
