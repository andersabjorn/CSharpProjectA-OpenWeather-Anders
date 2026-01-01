using Assignment_A1_01.Models;
using Assignment_A1_01.Services;
using System.Text.Json;


namespace Assignment_A1_01;

internal class Program
{
    static async Task Main(string[] args)
    {
        var apiKey = ReadApiKey() ?? "PUT_YOUR_API_KEY_HERE";


        var service = new OpenWeatherService(apiKey);

        // Event-utskrift
        service.ForecastReceived += (sender, message) =>
        {
            Console.WriteLine($"Event message from weather service: {message}");
        };

        // Geo: Stockholm (valfri)
        double lat = 59.3293;
        double lon = 18.0686;

        // Cities
        string city1 = "Edsbyn";
        string city2 = "Miami";
        string city3 = "Sydney";

        // A1.2 – parallella anrop
        var geoTask = service.GetForecastAsync(lat, lon);
        var c1Task = service.GetForecastByCityAsync(city1);
        var c2Task = service.GetForecastByCityAsync(city2);
        var c3Task = service.GetForecastByCityAsync(city3);

        try
        {
            await Task.WhenAll(geoTask, c1Task, c2Task, c3Task);

            Console.WriteLine();
            PrintForecast("Weather forecast for geo location", geoTask.Result);

            Console.WriteLine();
            PrintForecast($"Weather forecast for {c1Task.Result.CityName}", c1Task.Result);

            Console.WriteLine();
            PrintForecast($"Weather forecast for {c2Task.Result.CityName}", c2Task.Result);

            Console.WriteLine();
            PrintForecast($"Weather forecast for {c3Task.Result.CityName}", c3Task.Result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Weather service error.");
            Console.WriteLine(ex.Message);
        }

        // A1.3 – cache-demo (samma stad två gånger)
        Console.WriteLine();
        Console.WriteLine($"Requesting {city2} again to demonstrate cache...");
        Console.WriteLine();

        try
        {
            var first = await service.GetForecastByCityAsync(city2);
            PrintForecast("First call (should be NEW)", first);

            var second = await service.GetForecastByCityAsync(city2);
            PrintForecast("Second call (should be CACHED)", second);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Weather service error during cache demo.");
            Console.WriteLine(ex.Message);
        }
    }

    private static void PrintForecast(string title, Forecast forecast)
    {
        Console.WriteLine(title);
        Console.WriteLine();

        var grouped = forecast.Items
            .GroupBy(i => i.DateTime.Date)
            .OrderBy(g => g.Key);

        foreach (var day in grouped)
        {
            Console.WriteLine(day.Key.ToString("yyyy-MM-dd"));

            foreach (var item in day)
            {
                Console.WriteLine(
                    $"  {item.DateTime:HH:mm}  {item.Description}, temperature: {item.Temperature:F1} degC, wind: {item.WindSpeed:F1} m/s");
            }

            Console.WriteLine();
        }
    }
        private static string? ReadApiKey()
    {
        try
        {
            // Project root: .../Assignment_A1_01/appsettings.local.json
            var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "appsettings.local.json");
            path = Path.GetFullPath(path);

            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            using var doc = System.Text.Json.JsonDocument.Parse(json);

            return doc.RootElement
                .GetProperty("OpenWeather")
                .GetProperty("ApiKey")
                .GetString();
        }
        catch
        {
            return null;
        }
    }

}

