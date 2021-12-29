using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Recovery.LogRecords;

public class SetIntLogRecord : ILogRecord
{
    private readonly IBlockId _block;
    private readonly int _transactionNumber;
    private readonly int _offset;
    private readonly int _value;
    public SetIntLogRecord(IPage page)
    {
        int tpos = 4;
        _transactionNumber = page.Get(tpos, new IntegerType());
        int fpos = tpos + 4;
        var fileName = page.Get(fpos, new StringType());
        int bpos = fpos + page.Length(new StringType(), fileName);
        int blockNumber = page.Get(bpos, new IntegerType());
        _block = new BlockId(fileName, blockNumber);
        int opos = bpos + 4;
        _offset = page.Get(opos, new IntegerType());
        int vpos = opos + 4;
        _value = page.Get(vpos, new IntegerType());
    }
    public int Operation => 4;

    public int TransactionNumber => _transactionNumber;

    public void Undo(ITransaction transaction)
    {
        transaction.Pin(_block);
        transaction.SetInt(_block, _offset, _value, false);
        transaction.UnPin(_block);
    }

    public override string ToString()
    {
        return $"<SETINT {_transactionNumber} {_block} {_offset} {_value}>";
    }

    public static int WriteToLog(ILogManager logManager, int transactionNumber, IBlockId block, int offset, int value)
    {
        var dummyPage = new Page(0);
        int tpos = 4;
        int fpos = tpos + 4;
        int bpos = fpos + dummyPage.Length(new StringType(), block.FileName);
        int opos = bpos + 4;
        int vpos = opos + 4;
        byte[] record = new byte[vpos + 4];
        Page p = new Page(record);
        p.Set(0, new IntegerType(), 4);
        p.Set(tpos, new IntegerType(), transactionNumber);
        p.Set(fpos, new StringType(), block.FileName);
        p.Set(bpos, new IntegerType(), block.Number);
        p.Set(opos, new IntegerType(), offset);
        p.Set(vpos, new IntegerType(), value);
        return logManager.Append(record);
    }
}
