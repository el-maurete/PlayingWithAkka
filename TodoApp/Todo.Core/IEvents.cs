namespace Todo.Core;

public interface IEvents
{
    public record Event(string Id) : IEvents;
    public record Created(string Id, string Title) : Event(Id);
    public record Completed(string Id, DateTime CompletedOn) : Event(Id);
}