using System.Text;
using FlatBuffers;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Files;
public struct StringType : IVariableLengthType<string>
{
    public int Length(string value)
    {
        return Encode(value).Length;
    }

    public string Decode(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    public byte[] Encode(string value)
    {
        return Encoding.UTF8.GetBytes(value);
    }
}
