using System.Text;
using FlatBuffers;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Files;
public struct IntegerType : IFixedLengthType<int>
{
    public int Length => 4;

    public int Decode(byte[] bytes)
    {
        var buffer = new FlatBuffers.ByteBuffer(bytes);
        return buffer.GetInt(0);
    }

    public byte[] Encode(int value)
    {
        var buffer = new FlatBuffers.ByteBuffer(Length);
        buffer.PutInt(0, value);
        return buffer.ToArray(0, Length);
    }
}
