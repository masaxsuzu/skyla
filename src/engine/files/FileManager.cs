using Skyla.Engine.Exceptions;
using Skyla.Engine.Interfaces;
namespace Skyla.Engine.Files;
public class FileManager : IFileManager
{
    private Dictionary<string, FileStream> _openFiles;
    public FileManager(DirectoryInfo storageDirectory, int blockSize)
    {
        _openFiles = new Dictionary<string, FileStream>();

        StorageDirectory = storageDirectory;
        BlockSize = blockSize;
        IsNew = !storageDirectory.Exists;

        if (IsNew)
        {
            StorageDirectory.Create();
        }

        foreach (var file in StorageDirectory.GetFiles().Where(f => f.FullName.StartsWith("temp")))
        {
            file.Delete();
        }
    }
    public DirectoryInfo StorageDirectory { get; }
    public bool IsNew { get; }

    public int BlockSize { get; }
    public IBlockId Append(string fileName)
    {
        int newBlockNumber = Length(fileName);
        BlockId block = new BlockId(fileName, newBlockNumber);
        byte[] bytes = new byte[BlockSize];
        try
        {
            FileStream file = GetOpenFile(fileName);
            file.Seek(block.Number * BlockSize, SeekOrigin.Begin);
            file.Write(bytes);
        }
        catch (IOException ex)
        {
            throw new EngineException($"cannot append block {block}", ex);
        }
        return block;
    }

    public int Length(string fileName)
    {
        try
        {
            FileStream file = GetOpenFile(fileName);
            return (int)(file.Length / BlockSize);
        }
        catch (IOException ex)
        {
            throw new EngineException($"cannot access {fileName}", ex);
        }
    }

    public void Read(IBlockId block, IPage page)
    {
        lock (this)
        {
            try
            {
                FileStream f = GetOpenFile(block.FileName);
                f.Seek(block.Number * BlockSize, SeekOrigin.Begin);
                f.CopyTo(page.Contents);
            }
            catch (IOException ex)
            {
                throw new EngineException($"cannot read block {block}", ex);
            }
        }
    }

    public void Write(IBlockId block, IPage page)
    {
        lock (this)
        {
            try
            {
                FileStream f = GetOpenFile(block.FileName);
                f.Seek(block.Number * BlockSize, SeekOrigin.Begin);
                page.Contents.CopyTo(f);
            }
            catch (IOException ex)
            {
                throw new EngineException($"cannot write block {block}", ex);
            }
        }
    }

    private FileStream GetOpenFile(string fileName)
    {
        FileStream? openFile = null;
        if (_openFiles.TryGetValue(fileName, out openFile))
        {
            return openFile;
        }
        else
        {
            var file = new FileInfo(Path.Combine(StorageDirectory.FullName, fileName));
            openFile = new FileStream(file.FullName, FileMode.OpenOrCreate);
            _openFiles.Add(fileName, openFile);
            return openFile;
        }
    }
}
