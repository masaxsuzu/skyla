using Skyla.Engine.Format;
using Skyla.Engine.Interfaces;
using Skyla.Engine.Scans;
namespace Skyla.Engine.Indexes;

public record BTreeDirectoryEntry(IConstant Value, int BlockNumber);
