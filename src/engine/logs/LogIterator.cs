using Skyla.Engine.Buffers;
using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Logs;
public class LogIterator
{
    private readonly IFileManager _file;
    private IBlockId _block;
    private IPage _page;
    private int _currentPosition;
    private int _boundary;

    public LogIterator(IFileManager file, IBlockId block)
    {
        _file = file;
        _block = block;
        var bytes = new byte[_file.BlockSize];
        _page = new Page(bytes);
        MoveToBlock(_block);
    }

    private void MoveToBlock(IBlockId block)
    {
        _file.Read(block, _page);
        _boundary = _page.Get(0, new IntegerType());
        _currentPosition = _boundary;
    }

    public byte[] Next()
    {
        if (_currentPosition == _file.BlockSize)
        {
            _block = new BlockId(_block.FileName, _block.Number - 1);
            MoveToBlock(_block);
        }
        byte[] record = _page.Get(_currentPosition, new ByteArrayType());
        _currentPosition += 4 + record.Length;
        return record;
    }
    public bool HasNext => _currentPosition < _file.BlockSize || _block.Number > 0;
}
