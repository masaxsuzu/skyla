using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Buffers;

public class Buffer : IBuffer
{
    private readonly IFileManager _file;
    private readonly ILogManager _log;
    private Page _page;
    private IBlockId? _block = null;
    private int _pins = 0;
    private int _transactionNumber = -1;
    private int _lsNumber = -1;
    public Buffer(IFileManager file, ILogManager log)
    {
        _file = file;
        _log = log;
        _page = new Page(_file.BlockSize);
    }
    public IPage Contents => _page;

    public IBlockId? Block => _block;

    public bool IsPinned => _pins > 0;

    public int ModifyingTransaction => _transactionNumber;

    public void SetModified(int transactionNumber, int lsNumber)
    {
        _transactionNumber = transactionNumber;
        if (lsNumber >= 0)
        {
            _lsNumber = lsNumber;
        }
    }

    public void AssignToBlock(IBlockId block)
    {
        _block = block;
        _file.Read(_block, _page);
        _pins = 0;
    }

    public void Flush()
    {
        if (_transactionNumber >= 0)
        {
            if (_block == null)
            {
                throw new EngineException($"no block is assigned to buffer");
            }
            _log.Flush(_lsNumber);
            _file.Write(_block, _page);
            _transactionNumber = -1;
        }
    }

    public void Pin()
    {
        _pins++;
    }
    public void Unpin()
    {
        _pins--;
    }
}
