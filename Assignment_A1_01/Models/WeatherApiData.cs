namespace Assignment_A1_01.Models;

// Matchar OpenWeather /forecast JSON (minsta som behövs)
public class WeatherApiData
{
    public City city { get; set; } = new();
    public List<ListItem> list { get; set; } = new();
}

public class City
{
    public string name { get; set; } = "";
}

public class ListItem
{
    public long dt { get; set; }
    public MainInfo main { get; set; } = new();
    public List<WeatherInfo> weather { get; set; } = new();
    public WindInfo wind { get; set; } = new();
}

public class MainInfo
{
    public double temp { get; set; }
}

public class WeatherInfo
{
    public string description { get; set; } = "";
}

public class WindInfo
{
    public double speed { get; set; }
}
