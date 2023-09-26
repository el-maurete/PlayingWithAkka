using Akka.Persistence.Fsm;
using Todo.Core;

namespace Todo.Api.Actors;

public class TodoActor : PersistentFSM<IStates, IModels, IEvents>
{
    public override string PersistenceId { get; }

    public TodoActor()
    {
        PersistenceId = nameof(TodoActor) + "_" + Self.Path.Name;
        
        StartWith(NullState, NullModel);
        
        When(NullState, (e, _) => e.FsmEvent switch
        {
            Create data => GoTo(InProgress)
                .Applying(new Created(data.Id, data.Title))
                .Replying(new Created(data.Id, data.Title)),

            _ => Stay()
                .Replying(e.StateData)
        });
        
        When(InProgress, (e, _) => e.FsmEvent switch
        {
            Complete data => GoTo(Done)
                .Applying(new Completed(data.Id, data.CompletedOn))
                .Replying(new Completed(data.Id, data.CompletedOn)),
            
            _ => Stay()
                .Replying(e.StateData)
        });
        
        When(Done, (e, _) => Stay().Replying(e.StateData));
    }

    protected override IModels ApplyEvent(IEvents domainEvent, IModels currentData) =>
        currentData.Apply(domainEvent);

    protected override void PreStart() => Metrics.Basket.ActiveActors.Inc();
    protected override void PostStop() => Metrics.Basket.ActiveActors.Dec();
}