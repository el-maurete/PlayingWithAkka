using Prometheus;
using WeatherMonitor;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddWeatherMonitor(builder.Configuration);

var app = builder.Build();
app.MapControllers();
app.MapMetrics();
app.Run();