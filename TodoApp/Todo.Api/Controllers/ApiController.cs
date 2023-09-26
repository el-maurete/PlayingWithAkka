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
}