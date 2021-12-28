using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Transactions;

public class BufferList
{
    private readonly IBufferManager _manager;
    private readonly Dictionary<IBlockId, IBuffer> _buffers;
    private readonly List<IBlockId> _pins;
    public BufferList(IBufferManager manager)
    {
        _manager = manager;
        _buffers = new Dictionary<IBlockId, IBuffer>();
        _pins = new List<IBlockId>();
    }

    public IBuffer Get(IBlockId block)
    {
        if (_buffers.TryGetValue(block, out IBuffer? buffer))
        {
            return buffer;
        }
        throw new EngineException("unreachable");
    }

    public void Pin(IBlockId block)
    {
        var buffer = _manager.Pin(block);
        if (buffer != null)
        {
            _buffers.Put(block, buffer);
            _pins.Add(block);
        }
    }
    public void Unpin(IBlockId block)
    {
        var buffer = Get(block);
        if (buffer != null)
        {
            _manager.Unpin(buffer);
            _pins.Remove(block);
            if (!_pins.Contains(block))
            {
                _buffers.Remove(block);
            }
        }
    }

    public void UnpinAll()
    {
        foreach (var block in _pins)
        {
            var buffer = Get(block);
            if (buffer != null)
            {
                _manager.Unpin(buffer);
            }
        }
        _buffers.Clear();
        _pins.Clear();
    }
}
