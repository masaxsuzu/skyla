using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Skyla.Engine.Buffers;
using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Language;
using Skyla.Engine.Logs;
using Skyla.Engine.Metadata;
using Skyla.Engine.Plans;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
using Skyla.Engine.Transactions;
using Xunit;
namespace Skyla.Tests;

public class OptimizedPlannerTest
{
    private Skyla.Engine.Database.Server _server;
    public OptimizedPlannerTest(ServerResolver r)
    {
        _server = r.Resolve("test_db2");
    }

    [Fact]
    public async Task IndexedPlannerTest1()
    {
        var transaction1 = _server.Create();
        var metadata = _server.Metadata;

        var schema = new Schema();
        schema.AddField(new IntegerFieldType("a"));
        var layout = new Layout(schema);

        await Task.Delay(0);

        var scan1 = new TableScan(transaction1, "plannerstest1", layout);
        for (int i = 1; i < 4; i++)
        {
            scan1.Insert();
            scan1.SetInt("a", i);
        }
        scan1.Close();
        transaction1.Commit();


        var transaction2 = _server.Create();

        var p = new Parser();
        {
            var sql1 = "select a from plannerstest1";
            var q = p.ParseQuery(sql1);

            var planner = new HeuristicQueryPlanner(metadata);
            var plan = planner.Create(q, transaction2);

            var s = plan.Open();

            var want = 1;
            while (s.Next())
            {
                Assert.Equal(want, s.GetInt("a"));
                want++;
            }
            s.Close();
            transaction2.Commit();
        }
        {
            var sql2 = "select a from plannerstest1 where a = 2";
            var q2 = p.ParseQuery(sql2);

            var planner2 = new HeuristicQueryPlanner(metadata);
            var plan2 = planner2.Create(q2, transaction2);

            var s2 = plan2.Open();

            while (s2.Next())
            {
                Assert.Equal(2, s2.GetInt("a"));
            }
            s2.Close();
        }
        transaction2.Commit();
    }

    [Fact]
    public async Task IndexedPlannerTest2()
    {
        await Task.Delay(0);

        var transaction1 = _server.Create();
        var metadata = _server.Metadata;

        var transaction = _server.Create();

        var p = new Parser();
        {
            var sql1 = "insert into plannerstest3 (y) values ('123')";
            var q = p.ParseInsert(sql1);

            var planner = new IndexedUpdatePlanner(metadata);
            int c = planner.Insert(q, transaction);

            Assert.Equal(1, c);
            transaction.Commit();
        }
        {
            var sql2 = "select y from plannerstest3";
            var q2 = p.ParseQuery(sql2);

            var planner2 = new HeuristicQueryPlanner(metadata);
            var plan2 = planner2.Create(q2, transaction);

            var s2 = plan2.Open();

            while (s2.Next())
            {
                Assert.Equal("123", s2.GetString("y"));
            }
            s2.Close();
            transaction.Commit();
        }
        {
            var sql3 = "update plannerstest3 set x = 1 where y = '123'";
            var q3 = p.ParseModify(sql3);

            var planner3 = new IndexedUpdatePlanner(metadata);
            var c = planner3.Modify(q3, transaction);
            Assert.Equal(1, c);
            transaction.Commit();
        }
        {
            var sql4 = "delete from plannerstest3 where x = 1";
            var q4 = p.ParseDelete(sql4);

            var planner4 = new IndexedUpdatePlanner(metadata);
            var c = planner4.Delete(q4, transaction);
            Assert.Equal(1, c);
            transaction.Commit();
        }
        {
            var sql5 = "delete from plannerstest3";
            var q5 = p.ParseDelete(sql5);

            var planner5 = new IndexedUpdatePlanner(metadata);
            var c = planner5.Delete(q5, transaction);
            Assert.Equal(0, c);
            transaction.Commit();
        }
    }
}
