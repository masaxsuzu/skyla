using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Skyla.Engine.Buffers;
using Skyla.Engine.Files;
using Skyla.Engine.Format;
using Skyla.Engine.Logs;
using Skyla.Engine.Transactions;
using Xunit;
namespace Skyla.Tests;

public class TransactionsTest : IDisposable
{
    private DirectoryInfo _dir;
    public TransactionsTest()
    {
#pragma warning disable CS8602
#pragma warning disable CS8604
        var entryPath = Assembly.GetEntryAssembly()?.Location;
        var parent = new DirectoryInfo(entryPath).Parent;
        var testPath = Path.Combine(parent.FullName, "TransactionsTest");
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
    public void TransactionTest()
    {
        var file = new FileManager(_dir, 400);
        var log = new LogManager(file, Guid.NewGuid().ToString());
        var buffer = new BufferManager(file, log, 8);

        var tx1 = new Transaction(file, log, buffer);

        var block = new BlockId(Guid.NewGuid().ToString(), 1);
        tx1.Pin(block);
        tx1.SetInt(block, 80, 1, false);
        tx1.SetString(block, 40, "one", false);
        tx1.Commit();

        var tx2 = new Transaction(file, log, buffer);
        tx2.Pin(block);
        int n1 = tx2.GetInt(block, 80);
        string s1 = tx2.GetString(block, 40);

        Assert.Equal(1, n1);
        Assert.Equal("one", s1);

        tx2.SetInt(block, 80, 2, true);
        tx2.SetString(block, 40, "two", true);
        tx2.Commit();

        var tx3 = new Transaction(file, log, buffer);
        tx3.Pin(block);
        tx3.SetInt(block, 80, 3, true);
        tx3.SetString(block, 40, "three", true);
        tx3.Rollback();

        var tx4 = new Transaction(file, log, buffer);
        tx4.Pin(block);
        int n2 = tx4.GetInt(block, 80);
        string s2 = tx4.GetString(block, 40);

        Assert.Equal(2, n2);
        Assert.Equal("two", s2);
    }

    [Fact]

    public async Task ConcurrencyTest()
    {
        var file = new FileManager(_dir, 400);
        var log = new LogManager(file, Guid.NewGuid().ToString());
        var buffer = new BufferManager(file, log, 2);

        var blockName = Guid.NewGuid().ToString();
        var block1 = new BlockId(blockName, 1);
        var block2 = new BlockId(blockName, 2);

        var tx1 = new Transaction(file, log, buffer);
        var tx2 = new Transaction(file, log, buffer);
        var tx3 = new Transaction(file, log, buffer);
        tx1.Pin(block1);
        tx2.Pin(block1);
        tx3.Pin(block1);
        tx1.Pin(block2);
        tx2.Pin(block2);
        tx3.Pin(block2);

        // Any can obtain shared locks if there is no x-lock.
        tx1.GetInt(block1, 0);
        tx1.GetInt(block2, 0);
        tx2.GetInt(block1, 0);
        tx3.GetInt(block2, 0);
        tx1.Commit();

        // Some can obtain exclusive locks if only it has shared locks. 
        tx2.SetInt(block1, 0, 1, false);
        tx3.SetInt(block2, 0, 2, true);

        tx2.Commit();
        tx3.Commit();

        var tx4 = new Transaction(file, log, buffer);

        tx4.Pin(block1);
        tx4.Pin(block2);

        var v1 = tx4.GetInt(block1, 0);
        var v2 = tx4.GetInt(block2, 0);
        tx4.Rollback();

        Assert.Equal(1, v1);
        Assert.Equal(2, v2);

        await Task.Delay(0);
    }
}
