using System;
using System.IO;
using System.Reflection;
using Skyla.Engine.Buffers;
using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Logs;
using Skyla.Engine.Metadata;
using Skyla.Engine.Records;
using Skyla.Engine.Transactions;
using Xunit;
namespace Skyla.Tests;

public class MetadataTest : IDisposable
{
    private DirectoryInfo _dir;
    public MetadataTest()
    {
#pragma warning disable CS8602
#pragma warning disable CS8604
        var entryPath = Assembly.GetEntryAssembly()?.Location;
        var parent = new DirectoryInfo(entryPath).Parent;
        var testPath = Path.Combine(parent.FullName, "MetadataTest");
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
    public void MetadataManagerTest()
    {
        var file = new FileManager(_dir, 400);
        var log = new LogManager(file, Guid.NewGuid().ToString());
        var buffer = new BufferManager(file, log, 8);
        var transaction = new Transaction(file, log, buffer);

        var tables = new TableMetadataManager(true, transaction);
        var views = new ViewMetadataManager(true, tables, transaction);
        var stats = new StatisticsMetadataManager(tables, transaction);
        var indexes = new IndexMetadataManager(true, tables, stats, transaction);

        var metadata = new MetadataManager(tables, views, stats, indexes);
        transaction.Commit();
        // 1. Tables
        {
            var layout1 = metadata.GetLayout("table_catalog", transaction);
            Assert.Equal(4 + 34 + 4, layout1.SlotSize);
            var layout2 = metadata.GetLayout("field_catalog", transaction);
            Assert.Equal(4 + 34 * 2 + 4 * 3, layout2.SlotSize);

            var schema = new Schema();
            schema.AddField(new IntegerFieldType("a"));
            schema.AddField(new StringFieldType("b", 2));

            metadata.CreateTable("t1", schema, transaction);
            var layout3 = metadata.GetLayout("t1", transaction);
            Assert.Equal(4 + 4 + 6, layout3.SlotSize);

            var typeA = layout3.Schema.GetType("a");
            var typeB = layout3.Schema.GetType("b");

            Assert.Equal(8, typeA.Type);
            Assert.Equal(22, typeB.Type);
        }

        // 2. Views
        {
            var viewDef = "SELECT a FROM t1 WHERE a = 1;";
            metadata.CreateView("v1", viewDef, transaction);
            var got = metadata.GetViewDefinition("v1", transaction);

            Assert.Equal(viewDef, got);
        }

        // 3. Statistics
        {
            var layout = metadata.GetLayout("t1", transaction);
            var tableScan = new TableScan(transaction, "t1", layout);
            for (int i = 0; i < 300; i++)
            {
                tableScan.Insert();
                tableScan.SetInt("a", i);
                tableScan.SetString("b", i.ToString());
            }
            tableScan.Close();

            var stat = metadata.GetStatInfo("t1", layout, transaction);

            Assert.Equal(300, stat.Records);
            Assert.Equal(22, stat.AccessedBlocks);
            Assert.Equal(101, stat.DistinctValues("a"));
            Assert.Equal(101, stat.DistinctValues("b"));
        }
    }
}
