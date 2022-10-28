using Akka.Actor;
using Akka.Hosting;
using Microsoft.AspNetCore.SignalR;

namespace WeatherMonitor;

public class WeatherHub : Hub
{
    private readonly IActorRef _weather;

    public WeatherHub(IReadOnlyActorRegistry registry)
    {
        _weather = registry.Get<WeatherSupervisorActor>();
    }

    public Task StartMonitoring(string city)
    {
        _weather.Tell(new StartMonitoring(city));
        return Task.CompletedTask;
    }

    public Task StopMonitoring(string city)
    {
        _weather.Tell(new StopMonitoring(city));
        return Task.CompletedTask;
    }
}
