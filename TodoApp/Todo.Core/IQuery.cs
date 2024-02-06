namespace Todo.Core;

public interface IQuery
{
    public record List() : IQuery;
    public record Find(string Id) : IQuery;
}
