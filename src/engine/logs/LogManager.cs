using Skyla.Engine.Files;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Logs;
public class LogManager : ILogManager
{
    private readonly IFileManager _fileManager;
    private readonly string _logFileName;
    private IPage _logPage;
    private IBlockId _currentBlockId;
    private int _lastLsNumber = 0;
    private int _lastSavedLsNumber = 0;
    public LogManager(IFileManager fileManager, string logFileName)
    {
        _fileManager = fileManager;
        _logFileName = logFileName;
        byte[] bytes = new byte[_fileManager.BlockSize];
        _logPage = new Page(bytes);
        int logSize = _fileManager.Length(logFileName);
        if (logSize == 0)
        {
            _currentBlockId = AppendNewBlock();
        }
        else
        {
            _currentBlockId = new BlockId(logFileName, logSize - 1);
            _fileManager.Read(_currentBlockId, _logPage);
        }
    }

    public IEnumerable<byte[]> AsEnumerable()
    {
        Flush();
        var i = new LogIterator(_fileManager, _currentBlockId);
        while (i.HasNext)
        {
            yield return i.Next();
        }
    }

    public int Append(byte[] record)
    {
        lock (this)
        {
            int boundary = _logPage.GetInt(0);
            int recordSize = record.Length;
            int bytesNeeded = recordSize + 4;

            if (boundary - bytesNeeded < 4)
            {
                Flush();
                _currentBlockId = AppendNewBlock();
                boundary = _logPage.GetInt(0);
            }

            int recordPosition = boundary - bytesNeeded;
            _logPage.SetBytes(recordPosition, record);
            _logPage.SetInt(0, recordPosition);
            _lastLsNumber += 1;
            return _lastLsNumber;
        }
    }

    public void Flush(int lsNumber)
    {
        if (_lastSavedLsNumber <= lsNumber)
        {
            Flush();
        }
    }

    private IBlockId AppendNewBlock()
    {
        IBlockId blockId = _fileManager.Append(_logFileName);
        _logPage.SetInt(0, _fileManager.BlockSize);
        _fileManager.Write(blockId, _logPage);
        return blockId;
    }

    private void Flush()
    {
        _fileManager.Write(_currentBlockId, _logPage);
        _lastSavedLsNumber = _lastLsNumber;
    }

    internal IPage GetInternalPage => _logPage;
    internal int GetInternalLastSavedLsNumber => _lastSavedLsNumber;
}
