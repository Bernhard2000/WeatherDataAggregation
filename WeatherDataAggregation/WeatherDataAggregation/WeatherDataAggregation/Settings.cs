using System.IO;
using System.Text.Json;
using WeatherDataAggregation.ViewModels;

namespace WeatherDataAggregation;

public static class Settings
{
    public static string SelectedWeatherService { get; set; }
    public static string OpenMeteoApiKey { get; set; }
    public static string OpenweathermapApiKey { get; set; }

    public static void Load()
    {
        if (File.Exists("settings.json"))
        {
            string json = File.ReadAllText("settings.json");
            var settings = JsonSerializer.Deserialize<SettingsViewModel>(json);

            SelectedWeatherService = settings.SelectedWeatherService;
            OpenMeteoApiKey = settings.OpenMeteoApiKey;
            OpenweathermapApiKey = settings.OpenweathermapApiKey;
        }
    }
}    
