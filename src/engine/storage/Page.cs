using System.Text;
using FlatBuffers;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Storage;
public class Page : IPage
{
    private static readonly Encoding encoding = Encoding.ASCII;
    private ByteBuffer _buffer;
    public Page(int blockSize)
    {
        _buffer = new ByteBuffer(blockSize);
    }
    public Page(byte[] bytes)
    {
        _buffer = new ByteBuffer(bytes);
    }
    public int GetInt(int offset)
    {
        return _buffer.GetInt(offset);
    }
    public void SetInt(int offset, int number)
    {
        _buffer.PutInt(offset, number);
    }
    public byte[] GetBytes(int offset)
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
    public void SetBytes(int offset, byte[] bytes)
    {
        _buffer.Position = offset;
        _buffer.PutInt(offset, bytes.Length);
        for (int i = 0; i < bytes.Length; i++)
        {
            _buffer.PutByte(offset + 4 + i, bytes[i]);
        }
    }
    public string GetString(int offset)
    {
        var bytes = GetBytes(offset);
        return encoding.GetString(bytes);
    }
    public void SetString(int offset, string str)
    {
        var bytes = encoding.GetBytes(str);
        SetBytes(offset, bytes);
    }
    public int MaxLength(int strLength)
    {
        return 4 + strLength * encoding.GetMaxByteCount(1);
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
