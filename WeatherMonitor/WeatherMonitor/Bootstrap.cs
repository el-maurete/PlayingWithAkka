using Akka.DependencyInjection;
using Akka.Hosting;
using Akka.Persistence.Hosting;
using Akka.Persistence.PostgreSql;
using Akka.Persistence.PostgreSql.Hosting;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using WeatherMonitor.Engine;
using WeatherMonitor.Queries;
using WeatherMonitor.Shared;

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
                var allEvents = PersistenceQuery.Get(system)
                    .ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
                
                registry.TryRegister<WeatherSupervisorActor>(
                    system.ActorOf(DependencyResolver.For(system).Props<WeatherSupervisorActor>(),
                        "weathers"));

                registry.TryRegister<QueryActor>(
                    system.ActorOf(DependencyResolver.For(system).Props<QueryActor>(allEvents),
                        "read-model"));
            });
        });
        return services;
    }

    private static string DbConnectionString(IConfiguration configuration) =>
        $"Host={configuration.GetValue<string>("DbHost")};" +
        $"Port=5432;" +
        $"Database=postgres;" +
        $"Username=postgres;" +
        $"Password=postgres";
}
