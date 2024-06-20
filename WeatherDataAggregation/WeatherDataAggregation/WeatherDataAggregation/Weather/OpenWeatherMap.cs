using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using WeatherDataAggregation.Weather;

namespace WeatherDataAggregation;

public static class OpenWeatherMap
{
    private static string api_key = "eee42da2ff38bfd2b56e79ab96d2250a";

    private static Uri base_url = new Uri("https://api.openweathermap.org/data/2.5/");

    public static WeatherData fetchCurrentData(Location location)
    {
        using HttpClient client = new HttpClient();
        var uri = new Uri(base_url, $"weather?lat={location.Latitude}&lon={location.Longitude}&appid={api_key}");
        HttpResponseMessage response = client.GetAsync(uri).Result;
        response.EnsureSuccessStatusCode();
        WeatherData data = new WeatherData();
        if (response.IsSuccessStatusCode)
        {
            string responseBody = response.Content.ReadAsStringAsync().Result;
            JsonNode json = JsonNode.Parse(responseBody);
            data.Temperature = json["main"]["temp"].ToString();
            data.Humidity = json["main"]["humidity"].ToString();
            data.WindSpeed = json["wind"]["speed"].ToString();
            data.Pressure = json["main"]["pressure"].ToString();
            data.Cloudiness = json["clouds"]["all"].ToString();
            data.WeatherDescription = json["weather"][0]["description"].ToString();
        }

        return data;
    }
}