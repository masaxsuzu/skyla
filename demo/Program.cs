using System;
using System.IO;
using System.Reflection;
using Skyla.Engine.Database;
using Skyla.Engine.Language;
using Skyla.Engine.Plans;
using Skyla.Engine.Interfaces;

#pragma warning disable CS8602
#pragma warning disable CS8604
var entryPath = Assembly.GetEntryAssembly()?.Location;
var parent = new DirectoryInfo(entryPath).Parent;
var testPath = Path.Combine(parent.FullName, "demo", System.Guid.NewGuid().ToString());
var dir = new DirectoryInfo(testPath);
if(dir.Parent.Exists){
    dir.Parent.Delete(true);
}
var s = new Server(dir, 400, 8);
var p = new Parser();
var tx = s.Create();
Console.WriteLine("Hello, Skyla!");
while (true)
{
     try
     {
        Console.Write("Skyla > ");
        var sql = Console.ReadLine();
        var (type, query) = p.Parse(sql);
        switch (type)
        {
            case Skyla.Engine.Interfaces.StatementType.table:
                var table = p.ParseCreateTable(sql);
                var plannerT = new NaiveUpdatePlanner(s.Metadata);
                var pT = plannerT.CreateTable(table, tx);
                break;
            case Skyla.Engine.Interfaces.StatementType.index:
                Console.WriteLine("index is not supported yet");
                break;
            case Skyla.Engine.Interfaces.StatementType.view:
                Console.WriteLine("view is not supported yet");
                break;
            case Skyla.Engine.Interfaces.StatementType.query:
                var query1 = p.ParseQuery(sql);
                var plannerQ = new NaiveQueryPlanner(p, s.Metadata);
                var pQ = plannerQ.Create(query1, tx);
                var scan = pQ.Open();
                while (scan.Next())
                {
                    var sb = new System.Text.StringBuilder();
                    foreach (var c in query1.ColumnNames)
                    {
                        var v = scan.Get(c);
                        Console.Write($"{c} = {v.Format()} ");
                    }
                    Console.WriteLine("");
                }
                scan.Close();
                break;
            case Skyla.Engine.Interfaces.StatementType.insert:
                var insert = p.ParseInsert(sql);
                var plannerI = new NaiveUpdatePlanner(s.Metadata);
                var pI = plannerI.Insert(insert, tx);
                break;
            case Skyla.Engine.Interfaces.StatementType.update:
                var modify = p.ParseModify(sql);
                var plannerM = new NaiveUpdatePlanner(s.Metadata);
                var pM = plannerM.Modify(modify, tx);
                break;
            case Skyla.Engine.Interfaces.StatementType.delete:
                var delete = p.ParseDelete(sql);
                var plannerD = new NaiveUpdatePlanner(s.Metadata);
                var pD = plannerD.Delete(delete, tx);
                break;
            default:
                break;
        };
     }
     catch (System.Exception ex)
     {
         Console.WriteLine(ex.Message);
     }
}
