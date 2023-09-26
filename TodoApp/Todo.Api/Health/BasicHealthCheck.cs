using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Todo.Api.Health;

public class BasicHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(HealthCheckResult.Healthy("All good here!",
            new Dictionary<string, object> {{"hello", "world"}}));
    }
}