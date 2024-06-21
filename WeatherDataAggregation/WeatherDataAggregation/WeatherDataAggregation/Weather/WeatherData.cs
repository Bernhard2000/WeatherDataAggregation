using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WeatherDataAggregation.Weather;

public class WeatherData
{
    public string Temperature { get; set; }
    
    public string MaximumTemperature { get; set; }

    public string MinimumTemperature { get; set; }

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
        MaximumTemperature = "Loading";
        MinimumTemperature = "Loading";
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
}