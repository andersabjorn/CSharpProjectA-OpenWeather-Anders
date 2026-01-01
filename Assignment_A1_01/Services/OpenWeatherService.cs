using System.Collections.Concurrent;
using System.Net;
using Newtonsoft.Json;
using Assignment_A1_01.Models;
using Assignment_A1_01.Utils;

namespace Assignment_A1_01.Services;

public class OpenWeatherService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient = new HttpClient();

    // Cache: key -> (time, forecast)
    private readonly ConcurrentDictionary<string, (DateTime Time, Forecast Forecast)> _cache = new();

    public OpenWeatherService(string apiKey)
    {
        _apiKey = apiKey;
    }

    // A1.2/A1.3: Event
    public event EventHandler<string>? ForecastReceived;

    private void OnForecastReceived(string message)
        => ForecastReceived?.Invoke(this, message);

    private static string BuildGeoKey(double lat, double lon)
        => $"geo:{lat:F4},{lon:F4}";

    private static string BuildCityKey(string city)
        => $"city:{city.ToLowerInvariant()}";

    // A1.1/A1.2/A1.3 – geolocation
    public Task<Forecast> GetForecastAsync(double lat, double lon)
    {
        string cacheKey = BuildGeoKey(lat, lon);
        string url =
            $"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&appid={_apiKey}&units=metric&lang=se";

        return GetForecastWithCacheAsync(cacheKey, url, $"lat {lat}, lon {lon}");
    }

    // A1.2/A1.3 – city
    public Task<Forecast> GetForecastByCityAsync(string city)
    {
        string cacheKey = BuildCityKey(city);
        string url =
            $"https://api.openweathermap.org/data/2.5/forecast?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric&lang=se";

        return GetForecastWithCacheAsync(cacheKey, url, city);
    }

    private async Task<Forecast> GetForecastWithCacheAsync(string cacheKey, string url, string identifier)
    {
        // A1.3 – cache i 1 minut
        if (_cache.TryGetValue(cacheKey, out var cached) &&
            DateTime.Now - cached.Time < TimeSpan.FromMinutes(1))
        {
            OnForecastReceived($"Cached weather forecast for {identifier} available");
            return cached.Forecast;
        }

        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            // Om stad ej hittas: ge ett tydligt fel
            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new Exception($"City not found: {identifier}");

            // 401 osv
            throw new Exception($"Weather service error ({(int)response.StatusCode} {response.StatusCode}): {json}");
        }

        var data = JsonConvert.DeserializeObject<WeatherApiData>(json)
                   ?? throw new Exception("Could not parse weather response.");

        var items = data.list
            .Select(x => new ForecastItem
            {
                DateTime = UnixTimeStampHelper.UnixTimeStampToDateTime(x.dt),
                Description = x.weather.FirstOrDefault()?.description ?? "",
                Temperature = x.main.temp,
                WindSpeed = x.wind.speed
            })
            .ToList();

        var forecast = new Forecast
        {
            CityName = data.city.name,
            Items = items
        };

        _cache[cacheKey] = (DateTime.Now, forecast);

        OnForecastReceived($"New weather forecast for {identifier} available");
        return forecast;
    }
}
