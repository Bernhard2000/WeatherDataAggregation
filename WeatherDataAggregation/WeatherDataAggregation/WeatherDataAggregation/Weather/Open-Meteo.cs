using System;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace WeatherDataAggregation.Weather;

public static class Open_Meteo
{
    private static Uri base_url = new Uri("https://api.open-meteo.com/v1/");
    public static WeatherData fetchCurrentData(Location location)
    {
        using HttpClient client = new HttpClient();
        var uri = new Uri(base_url, $"forecast?latitude={location.Latitude}&longitude={location.Longitude}&current=Temperature");
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