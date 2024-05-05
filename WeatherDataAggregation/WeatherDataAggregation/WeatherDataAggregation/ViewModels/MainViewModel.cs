using System.Net.Http;
using System.Reactive;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ReactiveUI;
using WeatherDataAggregation.Weather;

namespace WeatherDataAggregation.ViewModels;

public class MainViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
    
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    public MainViewModel()
    {
        RefreshCommand = ReactiveCommand.CreateFromTask(GetWeatherDataAsync);
    }
    
    private WeatherData _weatherData = new WeatherData();

    public WeatherData WeatherData
    {
        get => _weatherData;
        set
        {
            _weatherData = value;

            this.RaisePropertyChanged(nameof(Temperature));
            this.RaisePropertyChanged(nameof(WindSpeed));
        }
    }


    public string Temperature => WeatherData.Temperature;

    
    
    public string WindSpeed => WeatherData.WindSpeed;
    
    public string Humidity => WeatherData.Humidity;
    public string Pressure => WeatherData.Pressure;
    public string Cloudiness => WeatherData.Cloudiness;
    public string FeelsLike => WeatherData.FeelsLike;
    public string WeatherDescription => WeatherData.WeatherDescription;
    public string WeatherIcon => WeatherData.WeatherIcon;
    public string City => WeatherData.City;
    public string Country => WeatherData.Country;
    public string Time => WeatherData.Time;
    public string Date => WeatherData.Date;
    public string Sunrise => WeatherData.Sunrise;
    public string Sunset => WeatherData.Sunset;
    public string Visibility => WeatherData.Visibility;
    public string UvIndex => WeatherData.UvIndex;
    public string DewPoint => WeatherData.DewPoint;
    public string WindGust => WeatherData.WindGust;
    
    
    public async Task GetWeatherDataAsync()
    {
        using HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync("https://api.open-meteo.com/v1/forecast?latitude=48.3064&longitude=14.2861&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,rain,showers,snowfall,weather_code,cloud_cover,pressure_msl,surface_pressure,wind_speed_10m,wind_direction_10m,wind_gusts_10m&timezone=Europe%2FBerlin");
        response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            WeatherData = new WeatherData(JsonNode.Parse(responseBody));
        }
    }
#pragma warning restore CA1822 // Mark members as static
}