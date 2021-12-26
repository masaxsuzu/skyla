using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Storage;
using Xunit;
namespace Skyla.Tests;

public class StorageTest : IDisposable
{
    private DirectoryInfo _dir;
    public StorageTest()
    {
#pragma warning disable CS8602
#pragma warning disable CS8604
        var entryPath = Assembly.GetEntryAssembly()?.Location;
        var parent = new DirectoryInfo(entryPath).Parent;
        var testPath = Path.Combine(parent.FullName, "StorageTest");
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

        var manager = new FileManager(_dir, 400);

        var blockId1 = new BlockId(Guid.NewGuid().ToString(), 2);
        var page1 = new Page(manager.BlockSize);
        page1.SetString(pos1, str1);
        int size = page1.MaxLength(str1.Length);

        int pos2 = pos1 + size;
        page1.SetInt(pos2, num2);
        manager.Write(blockId1, page1);

        var page2 = new Page(manager.BlockSize);
        manager.Read(blockId1, page2);

        Assert.Equal(str1, page2.GetString(pos1));
        Assert.Equal(num2, page2.GetInt(pos2));
    }
}
