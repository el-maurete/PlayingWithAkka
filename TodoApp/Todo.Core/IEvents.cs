namespace Todo.Core;

public interface IEvents
{
    public record Event : IEvents;

    public static Event NullEvent = new ();

    public record Created(string Id, string Title) : IEvents;
    public record Completed(string Id, DateTime CompletedOn) : IEvents;
}