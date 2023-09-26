using Prometheus;

namespace Todo.Api.Metrics;

public static class Basket
{
    public static Gauge ActiveActors = Prometheus.Metrics.CreateGauge(
        "active_actors_total",
        "Number of currently active actors",
        new GaugeConfiguration());
}