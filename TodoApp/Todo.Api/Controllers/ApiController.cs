using Akka.Hosting;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Actors;
using Todo.Core;

namespace Todo.Api.Controllers;

[Route("[controller]")]
public class ApiController : ControllerBase
{
    private readonly ActorRegistry _registry;

    public ApiController(ActorRegistry registry) => _registry = registry;

    [HttpGet("list")]
    public async Task<ActionResult<Activity[]>> List()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var gateway = await _registry.GetAsync<ReadModelActor>(cts.Token);
        var response = await gateway.Ask(new IQuery.List(), cts.Token);
        return response switch
        {
            {} data => Ok(data),
            _ => Problem("Something went wrong")
        };
    }

    [HttpPost]
    public async Task<ActionResult<Created>> Create([FromBody] Create create)
    {
        var gateway = await _registry.GetAsync<TodoActor>();
        var response = await gateway.Ask(create, TimeSpan.FromSeconds(5));
        return response switch
        {
            Created data => Created("/", data),
            _ => Problem("Something went wrong")
        };
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Activity>> Read(string id)
    {
        var gateway = await _registry.GetAsync<TodoActor>();
        var response = await gateway.Ask(new Find(id), TimeSpan.FromSeconds(5));
        return response switch
        {
            IModels data when data != NullModel => Ok(data),
            _ => NotFound(id)
        };
    }

    
    [HttpPut]
    public async Task<ActionResult<Completed>> Update([FromBody] Complete complete)
    {
        var gateway = await _registry.GetAsync<TodoActor>();
        var response = await gateway.Ask(complete, TimeSpan.FromSeconds(5));
        return response switch
        {
            Completed data => Ok(data),
            _ => Problem("Something went wrong")
        };
    }
}