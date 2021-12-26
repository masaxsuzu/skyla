using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Logs;
using Skyla.Engine.Storage;
using Xunit;
namespace Skyla.Tests;

public class LogsTest : IDisposable
{
    private DirectoryInfo _dir;
    public LogsTest()
    {
#pragma warning disable CS8602
#pragma warning disable CS8604
        var entryPath = Assembly.GetEntryAssembly()?.Location;
        var parent = new DirectoryInfo(entryPath).Parent;
        var testPath = Path.Combine(parent.FullName, "LogTest");
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
    public void LogTest()
    {
        try
        {
            var file = new FileManager(_dir, 400);
            var log = new LogManager(file, Guid.NewGuid().ToString());
            CreateRecords(log, file, 1, 35);
            Assert.Equal(28, log.GetInternalLastSavedLsNumber);

            CreateRecords(log, file, 36, 70);
            log.Flush(65);
            Assert.Equal(70, log.GetInternalLastSavedLsNumber);

        }
        catch (EngineException ex)
        {
            Assert.False(true, ex.InnerException.Message);
        }
    }

    private static void CreateRecords(LogManager log, FileManager file, int start, int end)
    {
        for (int i = start; i <= end; i++)
        {
            var record = CreateLogRecord(log, file, $"record{i}", i + 100);
            int lsn = log.Append(record);
        }
    }
    private static byte[] CreateLogRecord(LogManager log, FileManager file, string str, int number)
    {
        int numberPosition = log.GetInternalPage.MaxLength(str.Length);
        byte[] bytes = new byte[numberPosition + 4];
        var page = new Page(bytes);
        page.SetString(0, str);
        page.SetInt(numberPosition, number);
        return bytes;
    }
}
