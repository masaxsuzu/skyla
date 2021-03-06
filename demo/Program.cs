using System;
using System.IO;
using System.Reflection;
using Skyla.Engine.Database;
using Skyla.Engine.Language;
using Skyla.Engine.Plans;
using Skyla.Engine.Drivers;
using Skyla.Engine.Scans;
using Skyla.Engine.Interfaces;

Console.WriteLine("initializing...");

#pragma warning disable CS8602
#pragma warning disable CS8604
var entryPath = Assembly.GetEntryAssembly()?.Location;
var parent = new DirectoryInfo(entryPath).Parent;
var testPath = Path.Combine(parent.FullName, "demo", System.Guid.NewGuid().ToString());
var dir = new DirectoryInfo(testPath);
if(dir.Parent.Exists){
    dir.Parent.Delete(true);
}

var s = new Server(dir, 4096, 256);
var tx = s.Create();

NaiveDriver  driver; 
if(1 < args.Length && args[0] == "naive")
{
    var queryPlanner = new NaiveQueryPlanner(new Parser(), s.Metadata);
    var commandPlanner = new NaiveUpdatePlanner(s.Metadata);
    var schemaPlanner = commandPlanner;
    driver = new NaiveDriver(s, tx, queryPlanner, commandPlanner, schemaPlanner);
}
else {
    var queryPlanner = new HeuristicQueryPlanner(s.Metadata);
    var commandPlanner = new IndexedUpdatePlanner(s.Metadata);
    var schemaPlanner = new NaiveUpdatePlanner(s.Metadata);
    driver = new NaiveDriver(s, tx, queryPlanner, commandPlanner, schemaPlanner);
}


#pragma warning disable CS8622
Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs args) {
    args.Cancel = true;
    Console.WriteLine("terminating...");
    System.Environment.Exit(-1);
};

var samples1 = new string[]{
    "create table a (x int, y varchar(2), z varchar(10))",
    "create view v1 as select x,y,z from a where x = 1",
    "create view v2 as select x,y,z from a where y = '2'",
    "create view v3 as select x,y,z from a where z = '3'",
    "create index i1 on a (x)",
};

var samples2 = new string[]{
    "select x,y,z from a",
    "select x,y,z from a where x = 1 and y = '2'",
    "select x,y,z from v1",
    "select x,y,z from v2",
    "select x,y,z from v3",
};

foreach (var sample in samples1)
{
    var ret = driver.Drive(sample);
}

var table = new TableScan(tx, "a", s.Metadata.GetLayout("a", tx));
table.BeforeFirst();
foreach (var i in System.Linq.Enumerable.Range(0, 100000))
{
    table.Insert();
    table.SetInt("x", i);
    table.SetString("y", (i % 10).ToString());
    table.SetString("z", (i % 100).ToString());
};
table.Close();


foreach (var sample in samples2)
{
    var ret = driver.Drive(sample);
}

while (true)
{
    Console.Write("Skyla > ");
    var sql = Console.ReadLine();
    if(sql == ":q")
    break;
    var ret = driver.Drive(sql);
    Console.WriteLine($"{ret.Message}");
}
