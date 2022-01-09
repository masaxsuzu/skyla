using System.Collections.Generic;
using Skyla.Engine.Database;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Language;
using Skyla.Engine.Language.Ast;
using Skyla.Engine.Scans;
using Xunit;
namespace Skyla.Tests;

public class ServerResolver
{
    private Dictionary<string, Server> _servers = new Dictionary<string, Server>();
    public ServerResolver(Dictionary<string, Server> s)
    {
        _servers = s;
    }
    public Server Resolve(string name)
    {
        return _servers[name];
    }
}
