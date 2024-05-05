using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WeatherDataAggregation.Weather;

public class WeatherData
{
    public string Temperature { get; set; }
    
    public string Precipitation { get; set; }
    public string WindSpeed { get; set; }
    public string WeatherDescription { get; set; }
    public string WeatherIcon { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string Time { get; set; }
    public string Date { get; set; }
    public string Sunrise { get; set; }
    public string Sunset { get; set; }
    public string Humidity { get; set; }
    public string Pressure { get; set; }
    public string SurfacePressure { get; set; }
    public string Visibility { get; set; }
    public string Cloudiness { get; set; }
    public string UvIndex { get; set; }
    public string DewPoint { get; set; }
    public string FeelsLike { get; set; }
    public string WindGust { get; set; }
    
    
    public WeatherData()
    {
        Temperature = "Loading...";
        WindSpeed = "Loading...";
        WeatherDescription = "Loading...";
        WeatherIcon = "Loading...";
        City = "Loading...";
        Country = "Loading...";
        Time = "Loading...";
        Date = "Loading...";
        Sunrise = "Loading...";
        Sunset = "Loading...";
        Humidity = "Loading...";
        Pressure = "Loading...";
        Visibility = "Loading...";
        Cloudiness = "Loading...";
        UvIndex = "Loading...";
        DewPoint = "Loading...";
        FeelsLike = "Loading...";
        WindGust = "Loading...";
    }
    
    //https://api.open-meteo.com/v1/forecast?latitude=48.3064&longitude=14.2861&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,rain,showers,snowfall,weather_code,cloud_cover,pressure_msl,surface_pressure,wind_speed_10m,wind_direction_10m,wind_gusts_10m&timezone=Europe%2FBerlin
    public WeatherData(JsonNode json)
    {
        Temperature = json["current"]["temperature_2m"].ToString();
        Precipitation = json["current"]["precipitation"].ToString();
        WindSpeed = json["current"]["wind_speed_10m"].ToString();
        Humidity = json["current"]["relative_humidity_2m"].ToString();
        Pressure = json["current"]["pressure_msl"].ToString();
        SurfacePressure = json["current"]["surface_pressure"].ToString();
        Cloudiness = json["current"]["cloud_cover"].ToString();
        FeelsLike = json["current"]["apparent_temperature"].ToString();
        WindGust = json["current"]["wind_gusts_10m"].ToString();
    }
}