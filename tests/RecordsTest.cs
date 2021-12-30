using System;
using System.IO;
using System.Reflection;
using Skyla.Engine.Buffers;
using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Logs;
using Skyla.Engine.Records;
using Skyla.Engine.Transactions;
using Xunit;
namespace Skyla.Tests;

public class RecordsTest : IDisposable
{
    private DirectoryInfo _dir;
    public RecordsTest()
    {
#pragma warning disable CS8602
#pragma warning disable CS8604
        var entryPath = Assembly.GetEntryAssembly()?.Location;
        var parent = new DirectoryInfo(entryPath).Parent;
        var testPath = Path.Combine(parent.FullName, "RecordsTest");
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
    public void TableScanTest()
    {
        var file = new FileManager(_dir, 400);
        var log = new LogManager(file, Guid.NewGuid().ToString());
        var buffer = new BufferManager(file, log, 8);
        var transaction1 = new Transaction(file, log, buffer);
        var transaction2 = new Transaction(file, log, buffer);

        var schema = new Schema();
        schema.AddField(new IntegerFieldType("a"));
        schema.AddField(new StringFieldType("b", 9));
        schema.AddField(new StringFieldType("c", 1));

        var layout = new Layout(schema);
        Assert.Equal(4, layout.Offset("a"));
        Assert.Equal(8, layout.Offset("b"));
        Assert.Equal(8 + 20, layout.Offset("c"));

        var scan1 = new TableScan(transaction1, "T", layout);
        for (int i = 1; i < 4; i++)
        {
            scan1.Insert();
            scan1.SetInt("a", i);
            scan1.SetString("b", (10 * i).ToString());
            scan1.SetString("c", i.ToString());
        }
        scan1.BeforeFirst();
        scan1.Next();
        scan1.Delete(); // Delete the first record.
        scan1.Close();
        transaction1.Commit();

        var scan2 = new TableScan(transaction2, "T", layout);

        scan2.BeforeFirst();
        scan2.Next();
        var (a2, b2, c2) = (scan2.GetInt("a"), scan2.GetString("b"), scan2.GetString("c"));
        scan2.Next();
        var (a3, b3, c3) = (scan2.GetInt("a"), scan2.GetString("b"), scan2.GetString("c"));

        Assert.Equal(2, a2);
        Assert.Equal("20", b2);
        Assert.Equal("2", c2);

        Assert.Equal(3, a3);
        Assert.Equal("30", b3);
        Assert.Equal("3", c3);

    }
}
