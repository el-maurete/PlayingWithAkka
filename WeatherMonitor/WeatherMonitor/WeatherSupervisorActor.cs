using Akka.Actor;
using Akka.Persistence;
using Akka.Util.Internal;

namespace WeatherMonitor;

public class WeatherSupervisorActor : ReceivePersistentActor
{
    private readonly IActorFactory<WeatherActor> _weatherActorFactory;
    private readonly HashSet<string> _cache = new();

    public WeatherSupervisorActor(IActorFactory<WeatherActor> weatherActorFactory)
    {
        _weatherActorFactory = weatherActorFactory;

        Recover<MonitoringStarted>(AddToList);
        Recover<MonitoringStopped>(RemoveFromList);
        Recover<RecoveryCompleted>(WakeUpChildren);

        Command<StartMonitoring>(x =>
            Persist(new MonitoringStarted(x.City),
                y => _weatherActorFactory.Build(Context, y.City)));
        
        Command<StopMonitoring>(x =>
            Persist(new MonitoringStopped(x.City),
                y => _weatherActorFactory.Build(Context, y.City).GracefulStop(TimeSpan.FromSeconds(3))));
    }

    public override string PersistenceId => "weather-supervisor";

    private void AddToList(MonitoringStarted obj) => _cache.Add(obj.City);
    private void RemoveFromList(MonitoringStopped obj) => _cache.Remove(obj.City);
    private void WakeUpChildren(RecoveryCompleted obj)
    {
        _cache.ForEach(x => _weatherActorFactory.Build(Context, x));
        _cache.Clear();
    }
}
