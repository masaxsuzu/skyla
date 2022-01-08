using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.MultiBuffers;

#pragma warning disable CS8602
public class MultiBufferProductScan : IScan
{
    ITransaction _t;
    IScan _ls;
    IScan? _rs, _ps;
    string _f;
    ILayout _layout;
    int _size, _nextBlock, _fSize;

    public MultiBufferProductScan(ITransaction transaction, IScan left, string fileName, ILayout layout)
    {
        _t = transaction;
        _ls = left;
        _f = fileName;
        _layout = layout;
        _fSize = _t.Size(_f);
        int a = _t.AvailableBuffers;
        _size = BufferNeeds.BestFactor(a, _fSize);
        BeforeFirst();
    }

    public void BeforeFirst()
    {
        _nextBlock = 0;
        UseNextChunk();
    }

    public void Close()
    {
        _ps.Close();
    }

    public IConstant Get(string fieldName)
    {
        return _ps.Get(fieldName);
    }

    public int GetInt(string fieldName)
    {
        return _ps.GetInt(fieldName);
    }

    public string GetString(string fieldName)
    {
        return _ps.GetString(fieldName);
    }

    public bool HasField(string fieldName)
    {
        return _ps.HasField(fieldName);
    }

    public bool Next()
    {
        while (!_ps.Next())
        {
            if (!UseNextChunk())
            {
                return false;
            }
        }
        return true;
    }
    private bool UseNextChunk()
    {
        if (_rs != null)
        {
            _rs.Close();
        }
        if (_fSize <= _nextBlock)
        {
            return false;
        }
        int end = _nextBlock + _size - 1;
        if (_fSize <= end)
        {
            end = _fSize - 1;
        }
        _rs = new ChunkScan(_t, _f, _layout, _nextBlock, end);
        _ls.BeforeFirst();
        _ps = new ProductScan(_ls, _rs);
        _nextBlock = end + 1;
        return true;
    }
}
