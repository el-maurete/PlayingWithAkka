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
    private HashSet<string> _citiesOnAlert = new();
    private int _counter;
    private long _lastestOffset;

    public QueryActor(IAllEventsQuery allEvents)
    {
        Recover<SnapshotOffer>(x =>
        {
            var recovered = (ValueTuple<long, string[]>)x.Snapshot;
            _lastestOffset = recovered.Item1;
            _citiesOnAlert = new HashSet<string>(recovered.Item2);
            Log.Info($"Snaphot: {_citiesOnAlert.Count} cities on alert, offset = {_lastestOffset}");
        });
        
        Recover<RecoveryCompleted>(_ =>
        {
            allEvents.AllEvents(Offset.Sequence(_lastestOffset))
                .Where(x => x.Event is CityEvent)
                .RunWith(Sink.ActorRef<EventEnvelope>(Self, new StreamEnded()), Context.Materializer());
            Log.Info($"Recovery completed");
        });
        
        Command<AlertsQuery>(_ => Sender.Tell(_citiesOnAlert.ToArray()));
        Command<EventEnvelope>(HandleEventEnvelope);
        Command<SaveSnapshotSuccess>(_ => Log.Info("Snapshot saved!"));
    }

    private void HandleEventEnvelope(EventEnvelope envelope)
    {
        envelope.Event.Match().With<CityEvent>(ev =>
        {
            _lastestOffset = (envelope.Offset as Sequence)?.Value ?? 0;
            Log.Info($"Event {envelope.Event} handled (offset = {_lastestOffset})");
            
            switch (envelope.Event)
            {
                case AlertStarted x : _citiesOnAlert.Add(x.City); break; 
                case AlertStopped x : _citiesOnAlert.Remove(x.City); break; 
            }

            if (++_counter >= 10)
            {
                Log.Info("Saving snapshot...");

                SaveSnapshot((_lastestOffset, _citiesOnAlert.ToArray()));
                _counter = 0;
            }
        });
    }

    public override string PersistenceId => "alerts-cache";
}
