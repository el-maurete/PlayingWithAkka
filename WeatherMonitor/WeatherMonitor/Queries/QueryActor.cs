using Akka;
using Akka.Actor;
using Akka.Persistence;
using Akka.Persistence.Query;
using Akka.Streams;
using Akka.Streams.Dsl;
using WeatherMonitor.Engine;

namespace WeatherMonitor.Queries;

internal record AlertsQuery;
internal record StreamEnded;

public class QueryActor : ReceivePersistentActor
{
    private readonly HashSet<string> _citiesOnAlert = new();

    private readonly IAllEventsQuery _allEvents;

    public QueryActor(IAllEventsQuery allEvents)
    {
        _allEvents = allEvents;
        Subscribe();
        Command<EventEnvelope>(HandleEventEnvelope);
        Command<AlertsQuery>(_ => Sender.Tell(_citiesOnAlert.ToArray()));
    }

    private void HandleEventEnvelope(EventEnvelope envelope)
    {
        envelope.Event.Match().With<CityEvent>(ev =>
        {
            // _lastestOffset = (envelope.Offset as Sequence)?.Value ?? 0;
            
            switch (envelope.Event)
            {
                case AlertStarted x : _citiesOnAlert.Add(x.City); break; 
                case AlertStopped x : _citiesOnAlert.Remove(x.City); break; 
            }
            
            // Persist(_allEvents, x => SaveSnapshot(_allEvents));
        });
    }

    private ActorMaterializer Subscribe()
    {
        var mat = Context.Materializer();
        _allEvents.AllEvents(Offset.NoOffset()) //(Offset.Sequence(_lastestOffset))
            .Where(x => x.Event is CityEvent)
            .RunWith(Sink.ActorRef<EventEnvelope>(Self, new StreamEnded()), mat);
        return mat;
    }

    public override string PersistenceId => "alerts-cache";
}
