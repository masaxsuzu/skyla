using System.Text;
using FlatBuffers;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Format;
public struct ByteArrayType : IVariableLengthType<byte[]>
{
    public int Length(byte[] value)
    {
        return Encode(value).Length;
    }

    public byte[] Decode(byte[] bytes)
    {
        return bytes;
    }

    public byte[] Encode(byte[] value)
    {
        return value;
    }
}
