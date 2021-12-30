using FlatBuffers;
using Skyla.Engine.Exceptions;
using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Buffers;
public class Page : IPage
{
    private ByteBuffer _buffer;
    public Page(int blockSize)
    {
        _buffer = new ByteBuffer(blockSize);
    }
    public Page(byte[] bytes)
    {
        _buffer = new ByteBuffer(bytes);
    }
    public T Get<T>(int offset, IFixedLengthType<T> size)
    {
        byte[] bytes = new byte[size.Length];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = _buffer.Get(offset + i);
        }
        return size.Decode(bytes);
    }
    public void Set<T>(int offset, IFixedLengthType<T> size, T value)
    {
        byte[] bytes = size.Encode(value);
        for (int i = 0; i < bytes.Length; i++)
        {
            _buffer.Put(offset + i, bytes[i]);
        }
    }

    public T Get<T>(int offset, IVariableLengthType<T> size)
    {
        byte[] bytes = GetBytes(offset);
        T value = size.Decode(bytes);
        var len = size.Length(value);
        if (len < bytes.Length)
        {
            throw new EngineException($"length of {value} must be less than ${len + 1}");
        }
        return value;
    }
    public void Set<T>(int offset, IVariableLengthType<T> size, T value)
    {
        var len = size.Length(value);
        var bytes = size.Encode(value);
        if (len < bytes.Length)
        {
            throw new EngineException($"length of {value} must be less than {len + 1}");
        }
        SetBytes(offset, bytes);
    }
    private byte[] GetBytes(int offset)
    {
        _buffer.Position = offset;
        int len = _buffer.GetInt(offset);
        byte[] bytes = new byte[len];
        for (int i = 0; i < len; i++)
        {
            bytes[i] = _buffer.Get(offset + 4 + i);
        }
        return bytes;
    }
    private void SetBytes(int offset, byte[] bytes)
    {
        _buffer.Position = offset;
        _buffer.PutInt(offset, bytes.Length);
        for (int i = 0; i < bytes.Length; i++)
        {
            _buffer.PutByte(offset + 4 + i, bytes[i]);
        }
    }

    public int Length<T>(IFixedLengthType<T> size)
    {
        return size.Length;
    }

    public int Length<T>(IVariableLengthType<T> size, T value)
    {
        return new IntegerType().Length + size.Length(value);
    }
    public Stream Contents
    {
        get
        {
            _buffer.Reset();
            return _buffer.ToMemoryStream(0, _buffer.Length);
        }
    }
}
