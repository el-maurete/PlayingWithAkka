namespace WeatherMonitor;

public enum WindLevel { Calm, Breezy, Stormy }

public record Weather(string City, int Temperature, WindLevel WindLevel)
{
    public override string ToString() => $"{Temperature}' and {WindLevel}";
};

public record StartMonitoring(string City);
public record StopMonitoring(string City);
public record StartAlert;
public record StopAlert;

public record CityEvent(string City);
public record MonitoringStarted(string City) : CityEvent(City);
public record MonitoringStopped(string City) : CityEvent(City);
public record AlertStarted(string City) : CityEvent(City);
public record AlertNotified(string City) : CityEvent(City);
public record AlertStopped(string City) : CityEvent(City);
