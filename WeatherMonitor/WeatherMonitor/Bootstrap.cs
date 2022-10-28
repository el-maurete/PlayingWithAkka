using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Hosting;
using Akka.Pattern;
using Akka.Persistence.Hosting;
using Akka.Persistence.PostgreSql;
using Akka.Persistence.PostgreSql.Hosting;

namespace WeatherMonitor;

public static class Bootstrap
{
    public static IServiceCollection AddWeatherMonitor(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IWeatherApiClient, WeatherApiClient>();
        services.AddSingleton<IActorFactory<WeatherActor>>(new ActorFactory<WeatherActor>());
        services.AddSingleton<IActorFactory<AlertActor>>(new ActorFactory<AlertActor>());
        services.AddAkka("weather-monitor", (builder, _) =>
        {
            builder.WithPostgreSqlPersistence(DbConnectionString(configuration),
                PersistenceMode.Both,
                autoInitialize: true,
                schemaName: "public",
                storedAsType: StoredAsType.JsonB,
                sequentialAccess: false,
                useBigintIdentityForOrderingColumn: true);

            builder.WithActors((system, registry) =>
            {
                registry.TryRegister<WeatherSupervisorActor>(
                    system.ActorOf(DependencyResolver.For(system).Props<WeatherSupervisorActor>(),
                        "weathers"));
            });
        });
        return services;
    }

    private static Props BuildBackoffSupervisorProps(Props childProps, string childName)
        => BackoffSupervisor.Props(Backoff.OnFailure(
            childProps,
            childName,
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(30),
            0.2,
            10));

    private static string DbConnectionString(IConfiguration configuration) =>
        $"Host={configuration.GetValue<string>("DbHost")};" +
        $"Port=5432;" +
        $"Database=postgres;" +
        $"Username=postgres;" +
        $"Password=postgres";
}