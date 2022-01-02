using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Skyla.Engine.Buffers;
using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Language;
using Skyla.Engine.Logs;
using Skyla.Engine.Plans;
using Skyla.Engine.Records;
using Skyla.Engine.Transactions;
using Xunit;
namespace Skyla.Tests;

public class Startup : IDisposable
{
    DirectoryInfo _dir;

    public Startup()
    {
#pragma warning disable CS8602
#pragma warning disable CS8604
        var entryPath = Assembly.GetEntryAssembly()?.Location;
        var parent = new DirectoryInfo(entryPath).Parent;
        var testPath = Path.Combine(parent.FullName, "test_db");
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
    public void ConfigureServices(IServiceCollection services)
    {
        var s = new Skyla.Engine.Database.Server(_dir, 400, 8);
        services.AddSingleton(s);

        // creating tables/views in separated tests can cause deadlocks.
        var parser = new Parser();
        var planner = new NaiveUpdatePlanner(s.Metadata);

        var t = s.Create();

        var tables = new string[] {
            "create table metadatatest1 (a int, b varchar(2))",
            "create table plannerstest1 (a int)",
            "create table plannerstest3 (x int, y varchar(4), z int)",
        };

        foreach (var table in tables)
        {
            var st = parser.ParseCreateTable(table);
            planner.CreateTable(st, t);
        }

        var views = new string[] {
            "create view metadatatest2 as select a from metadatatest1 where a = 1",
            "create view plannerstest2 as select a from plannerstest1 where a = 2",
        };

        foreach (var view in views)
        {
            var st = parser.ParseCreateView(view);
            planner.CreateView(st, t);
        }

        t.Commit();

    }

    public void Dispose()
    {
        foreach (var file in _dir.GetFiles())
        {
            file.Delete();
        }
        _dir.Delete();
    }

    public Schema CreateSchema(IFieldType[] fields)
    {
        var s = new Schema();
        foreach (var item in fields)
        {
            s.AddField(item);
        }
        return s;
    }
}
