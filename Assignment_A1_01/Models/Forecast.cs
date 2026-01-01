namespace Assignment_A1_01.Models;

public class Forecast
{
    public string CityName { get; set; } = "";
    public List<ForecastItem> Items { get; set; } = new();
}
