using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net.Http;
using System.Reactive;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using WeatherDataAggregation.Weather;

namespace WeatherDataAggregation.ViewModels;

public class MainViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
    
    private string api_request2 = "https://api.open-meteo.com/v1/forecast?latitude=48.3064&longitude=14.2861&hourly=temperature_2m,relative_humidity_2m,dew_point_2m,apparent_temperature,precipitation_probability,precipitation";

    private string api_request =
        "https://api.open-meteo.com/v1/forecast?latitude=48.3064&longitude=14.2861&hourly=temperature_2m&past_days=1&forecast_days=1";
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    private string _locationSearchQuery = "";

    public string LocationSearchQuery
    {
        get => _locationSearchQuery;

        set => this.RaiseAndSetIfChanged(ref _locationSearchQuery, value);
    }
    
    private Location _selectedLocation = new Location { Name = "Linz", Latitude = 48.3064, Longitude = 14.2861 };
    public Location SelectedLocation
    {
        get => _selectedLocation;
        set => this.RaiseAndSetIfChanged(ref _selectedLocation, value);
    }
    public ObservableCollection<Location> LocationSearchResults { get; set; } = new ObservableCollection<Location>();

    
    public MainViewModel()
    {
        RefreshCommand = ReactiveCommand.CreateFromTask(GetWeatherDataAsync);
    }
    

    public ObservableCollection<WeatherData> WeatherDataList { get; set; } = new ObservableCollection<WeatherData>();
    
    public async Task GetWeatherDataAsync()
    {
        WeatherDataList.Clear();
        using HttpClient client = new HttpClient();
        //HttpResponseMessage response = await client.GetAsync("https://api.open-meteo.com/v1/forecast?latitude=48.3064&longitude=14.2861&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,rain,showers,snowfall,weather_code,cloud_cover,pressure_msl,surface_pressure,wind_speed_10m,wind_direction_10m,wind_gusts_10m&timezone=Europe%2FBerlin");
        HttpResponseMessage response = await client.GetAsync(api_request);

        response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonNode json = JsonNode.Parse(responseBody);
            if (json != null)
            {
                foreach (var weatherData in WeatherData.GetWeatherDataHourly(json))
                {
                    WeatherDataList.Add(weatherData);
                }
            }
        }
    }
#pragma warning restore CA1822 // Mark members as static
}