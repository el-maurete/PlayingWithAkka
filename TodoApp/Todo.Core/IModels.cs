namespace Todo.Core;

public interface IModels
{
    IModels Apply(IEvents e);

    public static readonly IModels NullModel = new Activity(null!, null!);
    
    public record Activity(string Id, string Title) : IModels
    {
        public IModels Apply(IEvents e) => e switch
        {
            IEvents.Created c => new Activity(c.Id, c.Title),
            IEvents.Completed c => new CompletedActivity(c.Id, Title, c.CompletedOn),
            _ => this
        };
    }

    public record CompletedActivity(
            string Id,
            string Title,
            DateTime CompletedOn)
        : Activity(Id, Title);
}
