using System;
using System.IO;
using System.Reflection;
using Skyla.Engine.Buffers;
using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Logs;
using Xunit;
namespace Skyla.Tests;

public class StringTypeTest
{
    [Theory]
    [InlineData(2, "")]
    [InlineData(4, "a")]
    [InlineData(4, "あ")]
    [InlineData(12, "𩸽は魚だよ")]
    [InlineData(34, "1234567890123456")]
    public void LengthTest(int len, string value)
    {
        var t = new StringType();
        var got = t.Length(value);
        var encoded = t.Encode(value);
        Assert.Equal(len, got);
        Assert.True(encoded.Length <= len);
    }
}
