using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Records;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Materialization;

#pragma warning disable CS8602
public class SortScan : IScan
{
    IUpdateScan _s1;
    IUpdateScan? _s2;
    IUpdateScan? _currentScan;
    bool _hasMore1;
    bool _hasMore2;
    List<IRecordId>? _savedPositions;
    RecordComparer _comp;
    public SortScan(List<TempTable> runs, RecordComparer comp)
    {
        _comp = comp;
        _s1 = runs[0].Update();
        _hasMore1 = _s1.Next();
        if (1 < runs.Count)
        {
            _s2 = runs[1].Update();
            _hasMore2 = _s2.Next();
        }
    }
    public void BeforeFirst()
    {
        _s1.BeforeFirst();
        _hasMore1 = _s1.Next();
        if (_s2 != null)
        {
            _s2.BeforeFirst();
            _hasMore2 = _s2.Next();
        }
    }

    public void Close()
    {
        _s1.Close();
        if (_s2 != null)
        {
            _s2.Close();
        }
    }

    public IConstant Get(string fieldName)
    {
        return _currentScan.Get(fieldName);
    }

    public int GetInt(string fieldName)
    {
        return _currentScan.GetInt(fieldName);
    }

    public string GetString(string fieldName)
    {
        return _currentScan.GetString(fieldName);
    }

    public bool HasField(string fieldName)
    {
        return _currentScan.HasField(fieldName);
    }
    public bool Next()
    {
        if (_currentScan == _s1)
        {
            _hasMore1 = _s1.Next();
        }
        else if (_currentScan == _s2)
        {
            _hasMore2 = _s2.Next();
        }

        if (!_hasMore1 && _hasMore2)
        {
            return false;
        }
        else if (_hasMore1 && _hasMore2)
        {
            if (_comp.Compare(_s1, _s2) < 0)
            {
                _currentScan = _s1;
            }
            else
            {
                _currentScan = _s2;
            }
        }
        else if (_hasMore1)
        {
            _currentScan = _s1;
        }
        else if (_hasMore2)
        {
            _currentScan = _s2;
        }
        return true;

    }

    public void SavePositions()
    {
        var r1 = _s1.Record;
        var r2 = _s2.Record;

        _savedPositions = new List<IRecordId>();
        _savedPositions.Add(r1);
        _savedPositions.Add(r2);
    }

    public void RestorePositions()
    {
        var r1 = _savedPositions[0];
        var r2 = _savedPositions[1];
        _s1.MoveTo(r1);
        _s2.MoveTo(r2);
    }
}
