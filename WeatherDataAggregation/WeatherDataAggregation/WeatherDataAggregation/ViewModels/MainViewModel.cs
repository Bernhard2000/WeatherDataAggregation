using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia;
using DynamicData;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using SkiaSharp;
using WeatherDataAggregation.Views;
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
    public ReactiveCommand<Unit, Unit> SearchLocationCommand { get; }

    private string _locationSearchQuery = "";

    public string LocationSearchQuery
    {
        get => _locationSearchQuery;

        set => this.RaiseAndSetIfChanged(ref _locationSearchQuery, value);
    }
    
    private DateTimeOffset _dateFrom = DateTimeOffset.Now - new TimeSpan(7, 0, 0, 0);
    public DateTimeOffset DateFrom
    {
        get => _dateFrom;
        set => this.RaiseAndSetIfChanged(ref _dateFrom, value);
    }

    private DateTimeOffset _dateTo = DateTimeOffset.Now;
    public DateTimeOffset DateTo
    {
        get => _dateTo;
        set => this.RaiseAndSetIfChanged(ref _dateTo, value);
    }

    private ISeries[] _temperatureLines = new ISeries[] { };
    public ISeries[] TemperatureLines
    {
        get => _temperatureLines;
        set  => this.RaiseAndSetIfChanged(ref _temperatureLines, value);
        
    }
    
    private ISeries[] _averageTemperatures = new ISeries[] { };
    public ISeries[] AverageTemperatures
    {
        get => _averageTemperatures;
        set => this.RaiseAndSetIfChanged(ref _averageTemperatures, value);
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
        SearchLocationCommand = ReactiveCommand.Create(SearchLocation);
    }
    

    public ObservableCollection<WeatherData> WeatherDataList { get; set; } = new ObservableCollection<WeatherData>();
    
    private void SearchLocation()
    {
        
        GeoCoding.GetLocationAsync(LocationSearchQuery).ContinueWith((result) =>
        {
            if(result.Status == TaskStatus.Faulted)
            {
                return;
            }
            LocationSearchResults.Clear();
            foreach (var location in result.Result)
            {
                LocationSearchResults.Add(location);
            }
            SelectedLocation = LocationSearchResults[0];
        });
        LocationSearchQuery = "";
    }
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
                // Plot the temperature data
                var temperatures = WeatherDataList.Select(wd => double.Parse(wd.Temperature, CultureInfo.InvariantCulture)).ToArray();
                var times = WeatherDataList.Select(wd => wd.Time).ToArray();

                var series = new LineSeries<double>
                {
                    Values = new ObservableCollection<double>(temperatures),
                    Stroke = new SolidColorPaint(new SKColor(255, 0, 0)),
                    Fill = new SolidColorPaint(new SKColor(255, 0, 0, 100)),
                    GeometrySize = 15,
                    GeometryStroke = new SolidColorPaint(new SKColor(255, 255, 255)),
                    GeometryFill = new SolidColorPaint(new SKColor(255, 0, 0)),
                };
                var linesList = TemperatureLines.ToList();
                linesList.Add(series);
                TemperatureLines = linesList.ToArray();
                
                var columnSeries = new ColumnSeries<double>
                {
                    Values = new ObservableCollection<double> { temperatures.Average() },
                    Stroke = new SolidColorPaint(new SKColor(0, 0, 255)),
                    Fill = new SolidColorPaint(new SKColor(0, 0, 255, 100)),
                    MaxBarWidth = 50,
                };

                AverageTemperatures = new ISeries[] { columnSeries };
            }
        }
    }
#pragma warning restore CA1822 // Mark members as static
}