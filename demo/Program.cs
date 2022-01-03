using System;
using System.IO;
using System.Reflection;
using Skyla.Engine.Database;
using Skyla.Engine.Language;
using Skyla.Engine.Plans;
using Skyla.Engine.Drivers;
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
var driver = new NaiveDriver(s, tx);


#pragma warning disable CS8622
Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs args) {
    args.Cancel = true;
    Console.WriteLine("terminating...");
    System.Environment.Exit(-1);
};

var samples = new string[]{
    "create table a (x int, y varchar(2), z varchar(10))",
    "create view v1 as select x,y,z from a where x = 1",
    "create view v2 as select x,y,z from a where y = '2'",
    "create view v3 as select x,y,z from a where z = '3'",
    "insert into a (x,y,z) values (0, '0', '0')",
    "insert into a (x,y,z) values (0, '0', '3')",
    "insert into a (x,y,z) values (0, '2', '0')",
    "insert into a (x,y,z) values (0, '2', '3')",
    "insert into a (x,y,z) values (1, '0', '0')",
    "insert into a (x,y,z) values (1, '0', '3')",
    "insert into a (x,y,z) values (1, '2', '0')",
    "insert into a (x,y,z) values (1, '2', '3')",
    "select x,y,z from a",
    "select x,y,z from a where x = 1 and y = '2'",
    "select x,y,z from v1",
    "select x,y,z from v2",
    "select x,y,z from v3",
};

foreach (var sample in samples)
{
    var ret = driver.Drive(sample);
}

foreach (var i in Enumerable.Range(0, 1000))
{
    var ret = driver.Drive($"insert into a (x,y,z) values ({i % 10}, '0', '{i.ToString()}')");
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
