using Akka.Actor;
using Akka.Hosting;
using Microsoft.AspNetCore.Mvc;
using WeatherMonitor.Engine;

namespace WeatherMonitor.Controllers;

[Route("[controller]")]
public class MonitorController : ControllerBase
{
    private readonly IActorRef _actors;

    public MonitorController(IReadOnlyActorRegistry registry) =>
        _actors = registry.Get<WeatherSupervisorActor>();

    [HttpGet("{city}/start")]
    public void Start(string city) =>
        _actors.Tell(new StartMonitoring(Normalize(city)));
    
    [HttpGet("{city}/stop")]
    public void Stop(string city) =>
        _actors.Tell(new StopMonitoring(Normalize(city)));
    
    private static string Normalize(string city) => city.Replace(" ", "-");
}
