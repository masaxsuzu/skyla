using Skyla.Engine.Exceptions;
using Skyla.Engine.Files;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Recovery.LogRecords;

public class SetStringLogRecord : ILogRecord
{
    private readonly IBlockId _block;
    private readonly int _transactionNumber;
    private readonly int _offset;
    private readonly string _value;
    public SetStringLogRecord(IPage page)
    {
        int tpos = 4;
        _transactionNumber = page.GetInt(tpos);
        int fpos = tpos + 4;
        var fileName = page.GetString(fpos);
        int bpos = fpos + page.MaxLength(fileName.Length);
        int blockNumber = page.GetInt(bpos);
        _block = new BlockId(fileName, blockNumber);
        int opos = bpos + 4;
        _offset = page.GetInt(opos);
        int vpos = opos + 4;
        _value = page.GetString(vpos);
    }
    public int Operation => 5;

    public int TransactionNumber => _transactionNumber;

    public void Undo(ITransaction transaction)
    {
        transaction.Pin(_block);
        transaction.SetString(_block, _offset, _value, false);
        transaction.UnPin(_block);
    }

    public override string ToString()
    {
        return $"<SETSTRING {_transactionNumber} {_block} {_offset} {_value}>";
    }

    public static int WriteToLog(ILogManager logManager, int transactionNumber, IBlockId block, int offset, string value)
    {
        var dummyPage = new Page(0);
        int tpos = 4;
        int fpos = tpos + 4;
        int bpos = fpos + dummyPage.MaxLength(block.FileName.Length);
        int opos = bpos + 4;
        int vpos = opos + 4;
        byte[] record = new byte[vpos + dummyPage.MaxLength(value.Length)];
        Page p = new Page(record);
        p.SetInt(0, 5);
        p.SetInt(tpos, transactionNumber);
        p.SetString(fpos, block.FileName);
        p.SetInt(bpos, block.Number);
        p.SetInt(opos, offset);
        p.SetString(vpos, value);
        return logManager.Append(record);
    }
}
