using Prometheus;
using WeatherMonitor;
using WeatherMonitor.Engine;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddWeatherMonitor(builder.Configuration);

var app = builder.Build();
app.MapControllers();
app.MapMetrics();
app.MapHub<WeatherHub>("/hub/weather");
app.UseSwagger();
app.UseSwaggerUI();
app.Run();