using System;
using System.IO;
using System.Reflection;
using Skyla.Engine.Buffers;
using Skyla.Engine.Files;
using Skyla.Engine.Format;
using Xunit;
namespace Skyla.Tests;

public class FilesTest : IDisposable
{
    private IntegerType _iSize = new IntegerType();
    private StringType _sSize = new StringType();
    private DirectoryInfo _dir;
    public FilesTest()
    {
#pragma warning disable CS8602
#pragma warning disable CS8604
        var entryPath = Assembly.GetEntryAssembly()?.Location;
        var parent = new DirectoryInfo(entryPath).Parent;
        var testPath = Path.Combine(parent.FullName, "FilesTest");
        _dir = new DirectoryInfo(testPath);
        if (!_dir.Exists)
        {
            _dir.Create();
        }
        foreach (var file in _dir.GetFiles())
        {
            file.Delete();
        }
    }

    public void Dispose()
    {
        foreach (var file in _dir.GetFiles())
        {
            file.Delete();
        }
    }

    [Fact]
    public void FileTest()
    {
        var (pos1, str1) = (88, "abcdefghijklm");
        int num2 = 345;
        var (pos3, str3) = (2, "𩸽はサロゲートペアだよ🙇‍♂️。");

        var manager = new FileManager(_dir, 400);

        var blockId1 = new BlockId(Guid.NewGuid().ToString(), 2);
        var page1 = new Page(manager.BlockSize);
        page1.Set(pos1, _sSize, str1);
        int size1 = page1.Length(_sSize, str1);

        int pos2 = pos1 + size1;
        page1.Set(pos2, _iSize, num2);
        page1.Set(pos3, _sSize, str3);
        manager.Write(blockId1, page1);

        var page2 = new Page(manager.BlockSize);
        manager.Read(blockId1, page2);

        Assert.Equal(str1, page2.Get(pos1, _sSize));
        Assert.Equal(num2, page2.Get(pos2, _iSize));
        Assert.Equal(str3, page2.Get(pos3, _sSize));
    }
}
