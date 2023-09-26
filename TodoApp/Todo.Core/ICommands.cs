namespace Todo.Core;

public interface ICommands
{
    string Id { get; }
    
    public record Find(string Id) : ICommands;
    public record Create(string Id, string Title, string Description) : ICommands;
    public record Complete(string Id, DateTime CompletedOn) : ICommands;
}