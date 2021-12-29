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

public class BuffersTest : IDisposable
{
    private DirectoryInfo _dir;
    public BuffersTest()
    {
#pragma warning disable CS8602
#pragma warning disable CS8604
        var entryPath = Assembly.GetEntryAssembly()?.Location;
        var parent = new DirectoryInfo(entryPath).Parent;
        var testPath = Path.Combine(parent.FullName, "BuffersTest");
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
    public void BufferTest()
    {
        IBuffer?[] testPool = new Skyla.Engine.Buffers.Buffer[6];

        var file = new FileManager(_dir, 400);
        var logFileName = Guid.NewGuid().ToString();
        var log = new LogManager(file, logFileName);
        var manager = new BufferManager(file, log, 3);
        Assert.Equal(3, manager.Available);
        testPool[0] = manager.Pin(new BlockId(logFileName, 0));
        Assert.Equal(2, manager.Available);
        testPool[1] = manager.Pin(new BlockId(logFileName, 1));
        Assert.Equal(1, manager.Available);
        testPool[2] = manager.Pin(new BlockId(logFileName, 2));
        Assert.Equal(0, manager.Available);

        manager.Unpin(testPool[1]);
        Assert.Equal(1, manager.Available);

        testPool[3] = manager.Pin(new BlockId(logFileName, 0));
        Assert.Equal(1, manager.Available);
        testPool[4] = manager.Pin(new BlockId(logFileName, 1));
        Assert.Equal(0, manager.Available);

        Assert.Throws<EngineException>(() =>
        {
            testPool[5] = manager.Pin(new BlockId(logFileName, 3));
        });
        Assert.Equal(0, manager.Available);

        manager.Unpin(testPool[2]);
        Assert.Equal(1, manager.Available);

        testPool[5] = manager.Pin(new BlockId(logFileName, 3));
        Assert.Equal(0, manager.Available);

        var internalPool = manager.GetInternalBufferPool;
        AssertBuffer(true, 0, internalPool[0]);
        AssertBuffer(true, 1, internalPool[1]);
        AssertBuffer(true, 3, internalPool[2]);
    }

    private void AssertBuffer(bool isPinned, int number, IBuffer buffer)
    {
        Assert.Equal(number, buffer.Block.Number);
        Assert.Equal(isPinned, buffer.IsPinned);
    }
}
