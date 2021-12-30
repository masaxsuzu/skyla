namespace Skyla.Engine.Interfaces;

public interface ILayout
{
    ISchema Schema { get; }
    int Offset(string fieldName);
    int SlotSize { get; }
}
