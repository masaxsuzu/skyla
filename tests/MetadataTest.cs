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
using Skyla.Engine.Scans;
using Xunit;
namespace Skyla.Tests;

public class MetadataTest
{
    private Skyla.Engine.Database.Server _server;
    public MetadataTest(Skyla.Engine.Database.Server server)
    {
        _server = server;
    }

    [Fact]
    public void MetadataManagerTest()
    {
        var transaction = _server.Create();
        var metadata = _server.Metadata;
        transaction.Commit();
        // 1. Tables
        {
            var layout1 = metadata.GetLayout("table_catalog", transaction);
            Assert.Equal(4 + 34 + 4, layout1.SlotSize);
            var layout2 = metadata.GetLayout("field_catalog", transaction);
            Assert.Equal(4 + 34 * 2 + 4 * 3, layout2.SlotSize);

            var layout3 = metadata.GetLayout("metadatatest1", transaction);
            Assert.Equal(4 + 4 + 6, layout3.SlotSize);

            var typeA = layout3.Schema.GetType("a");
            var typeB = layout3.Schema.GetType("b");

            Assert.Equal(8, typeA.Type);
            Assert.Equal(22, typeB.Type);
        }

        // 2. Views
        {
            var viewDef = "select a from metadatatest1 where a = 1";
            var got = metadata.GetViewDefinition("metadatatest2", transaction);

            Assert.Equal(viewDef, got);
        }

        // 3. Statistics
        {
            var layout = metadata.GetLayout("metadatatest1", transaction);
            var tableScan = new TableScan(transaction, "metadatatest1", layout);
            for (int i = 0; i < 300; i++)
            {
                tableScan.Insert();
                tableScan.SetInt("a", i);
                tableScan.SetString("b", i.ToString());
            }
            tableScan.Close();

            var stat = metadata.GetStatInfo("metadatatest1", layout, transaction);

            Assert.Equal(300, stat.Records);
            Assert.Equal(22, stat.AccessedBlocks);
            Assert.Equal(101, stat.DistinctValues("a"));
            Assert.Equal(101, stat.DistinctValues("b"));
        }

        transaction.Commit();
    }
}
