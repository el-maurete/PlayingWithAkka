using Akka.Actor;
using Akka.Persistence;
using WeatherMonitor.Shared;

namespace WeatherMonitor.Engine;

public class WeatherActor : ReceivePersistentActor
{
    private readonly string _id;
    private readonly IWeatherApiClient _client;
    private readonly IActorFactory<AlertActor> _alertActorFactory;

    private Action _state;

    public WeatherActor(string id, IWeatherApiClient client, IActorFactory<AlertActor> alertActorFactory)
    {
        _id = id;
        _client = client;
        _alertActorFactory = alertActorFactory;
        
        _state = Calm;

        Recover<AlertStarted>(_ => _state = Alerting);
        Recover<AlertStopped>(_ => _state = Unalerting);
        Recover<AlertNotified>(_ => _state = _state == Alerting ? OnAlert : Calm);
        Recover<RecoveryCompleted>(_ => Become(_state));
        
        Context.SetReceiveTimeout(TimeSpan.FromSeconds(3));
    }

    public override string PersistenceId =>
        nameof(WeatherActor) + "-" + _id.Replace(" ", "_");

    private void Calm()
    {
        Log.Info("Current status: All good");
        _state = Calm;
        Command<ReceiveTimeout>(CheckTheWeather);
        Command<StartAlert>(
            _ => Persist(new AlertStarted(_id),
                _ => Become(Alerting)));
    }

    private void Alerting()
    {
        void Send() => _alertActorFactory.Build(Context, _id + "-alarm").Tell(new AlertStarted(_id));
        
        _state = Alerting;
        Log.Info("Current status: Sending alert");
        Send();

        Command<ReceiveTimeout>(_ =>
        {
            Log.Info("Previous attempt timed out. Retrying sending the alert...");
            Send();
        });

        Command<AlertNotified>(
            x => Persist(x,
                _ => Become(OnAlert)));
    }

    private void OnAlert()
    {
        Log.Info("Current status: On Alert");
        _state = OnAlert;
        Command<ReceiveTimeout>(CheckTheWeather);
        Command<StopAlert>(
            _ => Persist(new AlertStopped(_id),
                _ => Become(Unalerting)));
    }
    
    private void Unalerting()
    {
        void Send() => _alertActorFactory.Build(Context, _id + "-alarm").Tell(new AlertStopped(_id));

        _state = Unalerting;
        Log.Info("Current status: Cancelling alert");
        Send();
        
        Command<ReceiveTimeout>(_ =>
        {
            Log.Info("Previous attempt timed out. Retrying sending the alert...");
            Send();
        });

        Command<AlertNotified>(
            x => Persist(x,
                _ => Become(Calm)));
    }

    private void CheckTheWeather(object _)
    {
        var weather = _client.Get(PersistenceId);
        Log.Info("{0}", weather);
        var badWeather = weather.WindLevel == WindLevel.Stormy;
        if (badWeather)
        {
            if (_state == Calm)
                Self.Tell(new StartAlert());
        }
        else
        {
            if (_state == OnAlert)
                Self.Tell(new StopAlert());
        }
    }
}
