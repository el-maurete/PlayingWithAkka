using Microsoft.Extensions.Caching.Memory;

namespace WeatherMonitor;

public interface IWeatherApiClient
{
    Weather Get(string city);
}

class WeatherApiClient : IWeatherApiClient
{
    private static readonly WindLevel[] WindOptions =
    {
        WindLevel.Calm, WindLevel.Breezy, WindLevel.Stormy
    };

    private static readonly MemoryCache Cache = new (new MemoryCacheOptions());
    
    public Weather Get(string city)
    {
        return Cache.GetOrCreate(city, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
            return new Weather(city,
                Random.Shared.Next(-10, 45),
                WindOptions[Random.Shared.Next(1000) % WindOptions.Length]);
        });
    }
}
