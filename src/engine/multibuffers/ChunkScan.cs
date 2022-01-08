using Skyla.Engine.Exceptions;
using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.MultiBuffers;

public class ChunkScan : IScan
{
    List<IRecordPage> _b = new List<IRecordPage>();
    ITransaction _t;
    string _f;
    ILayout _l;
    int _from, _to, _at;
    IRecordPage _page;
    int _slot;
    public ChunkScan(ITransaction transaction, string fileName, ILayout layout, int from, int to)
    {
        _t = transaction;
        _f = fileName;
        _l = layout;
        _from = from;
        _to = to;
        for (int i = from; i <= to; i++)
        {
            var block = new BlockId(_f, i);
            _b.Add(new RecordPage(_t, block, _l));
        }
        _page = MoveToBlock(_from);
    }
    public void BeforeFirst()
    {
        MoveToBlock(_from);
    }

    public void Close()
    {
        for (int i = 0; i < _b.Count; i++)
        {
            var b = new BlockId(_f, _from + i);
            _t.UnPin(b);
        }
    }

    public IConstant Get(string fieldName)
    {
        var type = _l.Schema.GetType(fieldName);
        if (type.Type == 8)
        {
            return new IntegerConstant(GetInt(fieldName));
        }
        if (type.Type == 22)
        {
            return new StringConstant(GetString(fieldName));
        }
        throw new Exceptions.EngineException($"unsupported constant type {type.Type}");
    }

    public int GetInt(string fieldName)
    {
        return _page.GetInt(_slot, fieldName);
    }

    public string GetString(string fieldName)
    {
        return _page.GetString(_slot, fieldName);
    }

    public bool HasField(string fieldName)
    {
        return _l.Schema.Has(fieldName);
    }

    public bool Next()
    {
        _slot = _page.NextAfter(_slot);
        while (_slot < 0)
        {
            if (_at == _to)
            {
                return false;
            }
            MoveToBlock(_page.Block.Number + 1);
            _slot = _page.NextAfter(_slot);
        }
        return true;
    }

    private IRecordPage MoveToBlock(int at)
    {
        _at = at;
        _page = _b[_at - _from];
        _slot = -1;

        return _page;
    }
}

