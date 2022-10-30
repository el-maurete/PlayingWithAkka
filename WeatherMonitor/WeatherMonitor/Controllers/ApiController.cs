using Akka.Actor;
using Akka.Hosting;
using Microsoft.AspNetCore.Mvc;
using WeatherMonitor.Queries;

namespace WeatherMonitor.Controllers;

[Route("[controller]")]
public class ApiController : ControllerBase
{
    private readonly IActorRef _query;

    public ApiController(IReadOnlyActorRegistry registry) =>
        _query = registry.Get<QueryActor>();

    [HttpGet("alerts")]
    public async Task<string[]> Get()
    {
        return await _query.Ask<string[]>(new AlertsQuery());
    }
}
