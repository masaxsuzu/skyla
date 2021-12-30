using System.Text;
using FlatBuffers;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Format;
public struct StringType : IVariableLengthType<string>
{
    public int Length(string value)
    {
        var count = new System.Globalization.StringInfo(value).LengthInTextElements;
        return Encoding.Unicode.GetMaxByteCount(count);
    }

    public string Decode(byte[] bytes)
    {
        return Encoding.Unicode.GetString(bytes);
    }

    public byte[] Encode(string value)
    {
        return Encoding.Unicode.GetBytes(value);
    }
}
