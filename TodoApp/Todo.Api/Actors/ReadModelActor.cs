using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;
using Akka.Streams.Dsl;
using Todo.Core;

namespace Todo.Api.Actors;

public class ReadModelActor : ReceiveActor
{
    private readonly SqlReadJournal _recovery;
    private readonly HashSet<string> _active = new();

    public ReadModelActor(SqlReadJournal journal)
    {
        _recovery = journal;
        
        Receive<IQuery.List>(_ => {
            Sender.Tell(_active);
        });

        Receive<Start>(_ => {
             Sender.Tell(new Ack());
        });

        Receive<EventEnvelope>(msg => {
            var _ = msg.Event switch
            {
                Created created => _active.Add(created.Id),
                Completed completed => _active.Remove(completed.Id),
                _ => false
            };
            Sender.Tell(new Ack());
        });

        Receive<Finished>(_ => {
            Sender.Tell(new Ack());
        });
    }

    protected override void PreStart()
    {
        var offset = 0L; // TODO: consider snapshotting
        _recovery.AllEvents(Offset.Sequence(offset))
                .RunWith(Sink.ActorRefWithAck<EventEnvelope>(Self, new Start(), new Ack(), new Finished()), Context.Materializer());
    }

    record Start;
    record Ack;
    record Finished;
}
