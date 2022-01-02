using System;
using System.Collections.Generic;
using Skyla.Engine.Buffers;
using Skyla.Engine.Files;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Logs;
using Skyla.Engine.Metadata;
using Skyla.Engine.Transactions;

namespace Skyla.Engine.Database;

public class Server
{

    private readonly DirectoryInfo _dir;
    private readonly int _blockSize;
    private readonly int _bufferSize;

    private readonly FileManager _file;
    private readonly LogManager _log;
    private readonly BufferManager _buffer;
    private readonly MetadataManager _metadata;


    public Server(DirectoryInfo storageDirectory, int blockSize, int bufferSize)
    {
        _dir = storageDirectory;
        _blockSize = blockSize;
        _bufferSize = bufferSize;

        _file = new FileManager(_dir, blockSize);
        _log = new LogManager(_file, Guid.NewGuid().ToString());
        _buffer = new BufferManager(_file, _log, bufferSize);
        var transaction = new Transaction(_file, _log, _buffer);

        var isNew = storageDirectory.Exists;

        if (!isNew)
        {
            transaction.Recover();
        }

        var tables = new TableMetadataManager(isNew, transaction);
        var views = new ViewMetadataManager(isNew, tables, transaction);
        var stats = new StatisticsMetadataManager(tables, transaction);
        var indexes = new IndexMetadataManager(isNew, tables, stats, transaction);

        _metadata = new MetadataManager(tables, views, stats, indexes);

        transaction.Commit();
    }

    public ITransaction Create() => new Transaction(_file, _log, _buffer);

    public IMetadataManager Metadata => _metadata;
}
