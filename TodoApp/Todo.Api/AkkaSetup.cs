using Akka.Cluster.Hosting;
using Akka.Cluster.Sharding;
using Akka.HealthCheck.Hosting;
using Akka.HealthCheck.Hosting.Web;
using Akka.Hosting;
using Akka.Persistence.Hosting;
using Akka.Persistence.PostgreSql;
using Akka.Persistence.PostgreSql.Hosting;
using Akka.Remote.Hosting;
using Todo.Api.Actors;

namespace Todo.Api;

public record Config(
    string DbHost = "localhost",
    string Role = "myRole",
    string SeedHost = "localhost",
    int SeedPort = 8110,
    string Host = "localhost",
    int Port = 8110);

public static class AkkaSetup
{
    public static IServiceCollection AddAkka(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var appName = "TodoApp";
        var config = configuration.Get<Config>()!;
        var maxNumberOfShards = 5;
        var role = config.Role;
        var dbHost = config.DbHost;
        var seedHost = config.SeedHost;
        var seedPort = config.SeedPort;
        var port = config.Port;
        var host = config.Host;

        var seedNode = $"akka.tcp://{appName}@{seedHost}:{seedPort}";

        return services
            .WithAkkaHealthCheck(HealthCheckType.All)
            .AddAkka(appName, (builder, serviceProvider) =>
            {
                builder.WithHealthCheck(o => o.AddProviders(HealthCheckType.All))
                    .WithWebHealthCheck(serviceProvider)
                    .WithRemoting(host, port)
                    .WithClustering(new ClusterOptions
                    {
                        Roles = new[] { role },
                        SeedNodes = new[] { seedNode }
                    })
                    .WithPostgreSqlPersistence(
                        GetConnectionString(dbHost),
                        PersistenceMode.Both,
                        autoInitialize: true,
                        schemaName: "public",
                        storedAsType: StoredAsType.JsonB,
                        sequentialAccess: true,
                        useBigintIdentityForOrderingColumn: true)
                    .WithShardRegion<TodoActor>(nameof(TodoActor),
                        (s, r, resolver) => _ => resolver.Props<TodoActor>(),
                        new ShardIdExtractor(maxNumberOfShards),
                        new ShardOptions
                        {
                            StateStoreMode = StateStoreMode.DData,
                            Role = role
                        })
                    .WithActors((system, registry, resolver) =>
                    {
                        //registry.TryRegister<CounterActor>(system.ActorOf(resolver.Props<COunterActor>()))
                    });
            });
    }

    private static string GetConnectionString(string dbHost) =>
        $"Host={dbHost};Port=5432;Database=postgres;Username=postgres;Password=postgres;";
}