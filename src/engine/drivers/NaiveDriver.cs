using System;
using System.Collections.Generic;
using Skyla.Engine.Buffers;
using Skyla.Engine.Database;
using Skyla.Engine.Files;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Language;
using Skyla.Engine.Plans;
using Skyla.Engine.Scans;

namespace Skyla.Engine.Drivers;

public class NaiveDriver
{
    private readonly Server _server;
    private readonly Parser _parser;
    private readonly IQueryPlanner _queryPlanner;
    private readonly ICommandPlanner _commandPlanner;
    private readonly ISchemaPlanner _schemaPlanner;
    private readonly ITransaction _tx;
    public NaiveDriver(Server server, ITransaction tx, IQueryPlanner q, ICommandPlanner c, ISchemaPlanner s)
    {
        _tx = tx;
        _server = server;
        _parser = new Parser();
        _queryPlanner = q;
        _commandPlanner = c;
        _schemaPlanner = s;
    }
#pragma warning disable CS8602
#pragma warning disable CS8604
    public IExecutionResult Drive(string sql)
    {
        try
        {
            var (type, statement) = _parser.Parse(sql);
            switch (type)
            {
                case Skyla.Engine.Interfaces.StatementType.table:
                    var s1 = statement as ICreateTableStatement;
                    var r1 = _schemaPlanner.CreateTable(s1, _tx);
                    return new ExecutionResult(r1, $"table '{s1.TableName}' has been created");
                case Skyla.Engine.Interfaces.StatementType.view:
                    var s2 = statement as ICreateViewStatement;
                    var r2 = _schemaPlanner.CreateView(s2, _tx);
                    return new ExecutionResult(r2, $"view '{s2.ViewName}' has been created");
                case Skyla.Engine.Interfaces.StatementType.index:
                    var s3 = statement as ICreateIndexStatement;
                    var r3 = _schemaPlanner.CreateIndex(s3, _tx);
                    return new ExecutionResult(r3, $"index '{s3.IndexName}' has been created");
                case Skyla.Engine.Interfaces.StatementType.query:
                    var q4 = statement as IQueryStatement;
                    var p4 = _queryPlanner.Create(q4, _tx);
                    var s4 = p4.Open();
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine(string.Join("\t", q4.ColumnNames));
                    sb.AppendLine(string.Join("\t", q4.ColumnNames.Select(s => "----")));
                    var r4 = 0;
                    while (s4.Next())
                    {
                        var line = new List<string>();
                        foreach (var columnName in q4.ColumnNames)
                        {
                            var v = s4.Get(columnName);
                            line.Add(v.Format());
                        }
                        r4++;
                        sb.AppendLine(string.Join("\t", line));
                    }
                    s4.Close();
                    return new ExecutionResult(r4, sb.ToString());
                case Skyla.Engine.Interfaces.StatementType.insert:
                    var c5 = statement as IInsertStatement;
                    var r5 = _commandPlanner.Insert(c5, _tx);
                    return new ExecutionResult(r5, $"Affected rows: {r5}");
                case Skyla.Engine.Interfaces.StatementType.update:
                    var c6 = statement as IModifyStatement;
                    var r6 = _commandPlanner.Modify(c6, _tx);
                    return new ExecutionResult(r6, $"Affected rows: {r6}");
                case Skyla.Engine.Interfaces.StatementType.delete:
                    var c7 = statement as IDeleteStatement;
                    var r7 = _commandPlanner.Delete(c7, _tx);
                    return new ExecutionResult(r7, $"Affected rows: {r7}");
                default:
                    return new ExecutionResult(-1, $"statement type {type} is not supported");
            };
        }
        catch (Skyla.Engine.Exceptions.EngineException ex1)
        {
            return new ExecutionResult(-1, ex1.Message);
        }
        catch (Exception ex2)
        {
            return new ExecutionResult(-1, ex2.StackTrace);
        }
    }
}
