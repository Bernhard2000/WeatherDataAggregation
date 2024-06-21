using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using DynamicData;
using LiveChartsCore;
using LiveChartsCore.Defaults;
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
    
    private DateTimeOffset _dateFrom = DateTimeOffset.Now - new TimeSpan(19, 0, 0, 0);
    public DateTimeOffset DateFrom
    {
        get => _dateFrom;
        set => this.RaiseAndSetIfChanged(ref _dateFrom, value);
    }

    private DateTimeOffset _dateTo = DateTimeOffset.Now - TimeSpan.FromDays(5);
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



    public bool SelectedOpenMeteo { get; set; }    
    public bool SelectedOpenWeatherMap { get; set; }
    public Axis[] XAxes { get; set; } =
    {
        new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yyyy"))
    };
    
    
    public Axis[] XAxesAverages { get; set; } =
    {
        new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yyyy"))
    };

    private ObservableCollection<string> _compareYears = new ObservableCollection<string>
    {
        "Dieses Jahr",
        "2 Jahre",
        "3 Jahre",
        "4 Jahre",
        "5 Jahre",
        "10 Jahre",
        "15 Jahre",
        "20 Jahre",
        "30 Jahre",
        "40 Jahre",
        "50 Jahre"
    };
    public ObservableCollection<string> CompareYears
    {
        get => _compareYears;
        set => this.RaiseAndSetIfChanged(ref _compareYears, value);
    }

    private string _compareYearsSelection = "Dieses Jahr";
    public string CompareYearsSelection
    {
        get => _compareYearsSelection;
        set
        {
            this.RaiseAndSetIfChanged(ref _compareYearsSelection, value);
            CompareYearsCommand.Execute().Subscribe();
        }
    }

    public ReactiveCommand<Unit, Unit> CompareYearsCommand { get; }

    private async Task CompareYearsAsync()
    {
        // Update the TemperatureLines and AverageTemperatures properties based on the selected year
        // This is just a placeholder. You need to implement the actual logic.
    }
    
    public ObservableCollection<Location> LocationSearchResults { get; set; } = new ObservableCollection<Location>();

    
    public MainViewModel()
    {
        RefreshCommand = ReactiveCommand.Create(AddLocation);
        SearchLocationCommand = ReactiveCommand.Create(SearchLocation);
        CompareYearsCommand = ReactiveCommand.CreateFromTask(CompareYearsAsync);
        
        this.WhenAnyValue(x => x.DateFrom, x => x.DateTo)
            .Subscribe(_ => OnDateChanged());
    }
    
    private async Task OnDateChanged()
    {
        TemperatureLines = new ISeries[] { };
        AverageTemperatures = new ISeries[] { };

        var tempTemperatureLines = new ConcurrentBag<ISeries>();
        var tempAverageTemperatures = new ConcurrentBag<ISeries>();

        await Parallel.ForEachAsync(Locations, async (location, cancellationToken) =>
        {
            var historicData = await FetchWeatherData(location);
            if (historicData == null)
            {
                return;
            }

            var color = GenerateRandomColor();
            var temperatureSeries = CalculateTemperatureSeries(historicData, color);
            var averageTemperatureColumns = CalculateAverageTemperatureColumns(historicData, color);

            tempTemperatureLines.Add(temperatureSeries);
            tempAverageTemperatures.Add(averageTemperatureColumns);
        });

        TemperatureLines = tempTemperatureLines.ToArray();
        AverageTemperatures = tempAverageTemperatures.ToArray();
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
    
    public ObservableCollection<Location> Locations { get; set; } = new ObservableCollection<Location>();
    
    public void AddLocation()
    {
        Locations.Add(SelectedLocation);
        GetWeatherData(SelectedLocation);
    }

    public async Task<WeatherData[]> FetchWeatherData(Location location)
    {
        WeatherData[] historicData;
        DateTime from = DateFrom.DateTime;
        DateTime to = DateTo.DateTime;

        try
        {
            historicData = await Open_Meteo.fetchHistoricDataHourly(location, from, to);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }

        return historicData;
    }
    public ISeries CalculateTemperatureSeries(WeatherData[] historicData, SKColor color)
    {
        var temperatures = historicData.Select(wd => double.Parse(wd.Temperature, CultureInfo.InvariantCulture)).ToArray();
        var times = historicData.Select(wd => wd.Time).ToArray();

        var timepoints = new ObservableCollection<DateTimePoint>();
        for (int i = 0; i < times.Length; i++)
        {
            timepoints.Add(new DateTimePoint
            {
                DateTime = DateTime.Parse(times[i]),
                Value = temperatures[i],
            });
        }

        var series = new LineSeries<DateTimePoint>
        {
            Values = timepoints,
            Stroke = new SolidColorPaint(color),
            Fill = null,
            GeometrySize = 0,
            GeometryStroke = new SolidColorPaint(color),
            GeometryFill = new SolidColorPaint(color),
            Name = SelectedLocation.ShortName
        };

        return series;
    }
    public ISeries CalculateAverageTemperatureColumns(WeatherData[] historicData, SKColor color)
    {
        var dailyAverages = historicData
            .GroupBy(wd => new DateTime(DateTime.Parse(wd.Time).Year, DateTime.Parse(wd.Time).Month, DateTime.Parse(wd.Time).Day))
            .Select(g => new
            {
                Day = g.Key,
                AverageTemperature = g.Average(wd => double.Parse(wd.Temperature, CultureInfo.InvariantCulture))
            });

        var timepointsAverages = new ObservableCollection<DateTimePoint>();

        foreach (var average in dailyAverages)
        {
            timepointsAverages.Add(new DateTimePoint
            {
                DateTime = average.Day,
                Value = average.AverageTemperature
            });
        }

        var columnSeries = new ColumnSeries<DateTimePoint>
        {
            Name = SelectedLocation.ShortName,
            Values = timepointsAverages,
            Stroke = new SolidColorPaint(color),
            Fill = new SolidColorPaint(color),
            MaxBarWidth = 10,
        };

        return columnSeries;
    }
    
    public async void GetWeatherData(Location location)
    {
        var historicData = await FetchWeatherData(location);
        if (historicData == null)
        {
            return;
        }

        var color = GenerateRandomColor();
        var temperatureSeries = CalculateTemperatureSeries(historicData, color);
        var averageTemperatureColumns = CalculateAverageTemperatureColumns(historicData, color);

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var linesList = TemperatureLines.ToList();
            linesList.Add(temperatureSeries);
            TemperatureLines = linesList.ToArray();

            var averagesList = AverageTemperatures.ToList();
            averagesList.Add(averageTemperatureColumns);
            AverageTemperatures = averagesList.ToArray();
        });
    }
    public async void GetWeatherDataOld(Location location)
    {
        XAxes.First().Name = "Datum";
        WeatherDataList.Clear();
        //foreach (var weatherData in WeatherData.GetWeatherDataHourly(json))
       
        WeatherData[] historicData;
        int years = 0;
        int.TryParse(CompareYearsSelection.Split(' ')[0], out years);
        int j= 0;

        DateTime from = DateFrom.DateTime;
        DateTime to = DateTo.DateTime;


        try
        {
            historicData = await Open_Meteo.fetchHistoricDataHourly(location, from, to);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return;
        }

        foreach (var weatherData in historicData)
        {
            if (!double.TryParse(weatherData.Temperature, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                continue;
            }

            WeatherDataList.Add(weatherData);
        }
        SKColor randomColor = GenerateRandomColor();

        // Plot the temperature data
        var temperatures = WeatherDataList.Select(wd => double.Parse(wd.Temperature, CultureInfo.InvariantCulture))
            .ToArray();
        var times = WeatherDataList.Select(wd => wd.Time).ToArray();
        
        //XAxis = new DateTimeAxis(TimeSpan.FromDays(1), time => time.ToString("dd.MM.yyyy"));
        

        /*while (temperatures.Length > 10000)
        {
            var averagedTemperatures = new List<double>();
            for (int i = 0; i < temperatures.Length -2; i += 3)
            {
                double average = (temperatures[i] + temperatures[i + 1] + temperatures[i + 2]) / 3;
                averagedTemperatures.Add(average);
            }
            temperatures = averagedTemperatures.ToArray();
        }*/
        
        var timepoints = new ObservableCollection<DateTimePoint>();
        for (int i = 0; i < times.Length; i++)
        {
            timepoints.Add(new DateTimePoint
            {
                DateTime = DateTime.Parse(times[i]),
                Value = temperatures[i],
            });
        }
        var series = new LineSeries<DateTimePoint>
        {
            Values =timepoints,
            Stroke = new SolidColorPaint(randomColor),
            Fill = null,
            GeometrySize = 0,
            GeometryStroke = new SolidColorPaint(randomColor),
            GeometryFill = new SolidColorPaint(randomColor),
            Name = SelectedLocation.ShortName
        };
        
      
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var linesList = TemperatureLines.ToList();
            linesList.Add(series);
            TemperatureLines = linesList.ToArray();
        });
        
        
        /*
        for (int i = 0; i < times.Length; i++)
        {
            timepointsAverages.Add(new DateTimePoint
            {
                DateTime = DateTime.Parse(times[i]),
                Value = temperatures.Average()
            });
        }

        var columnSeries = new ColumnSeries<DateTimePoint>
        {
            Values = new ObservableCollection<DateTimePoint> { timepointsAverages },
            Stroke = new SolidColorPaint(new SKColor(0, 0, 255)),
            Fill = new SolidColorPaint(new SKColor(0, 0, 255, 100)),
            MaxBarWidth = 50,
        };
        
        var averageList = AverageTemperatures.ToList()

        AverageTemperatures = new ISeries[] { columnSeries };*/
        
        var timepointsAverages = new ObservableCollection<DateTimePoint>();
/*
        // Group WeatherDataList by month and calculate the average temperature for each month
        var monthlyAverages = WeatherDataList
            .GroupBy(wd => new DateTime(DateTime.Parse(wd.Time).Year, DateTime.Parse(wd.Time).Month, 1))
            .Select(g => new
            {
                Month = g.Key,
                AverageTemperature = g.Average(wd => double.Parse(wd.Temperature, CultureInfo.InvariantCulture))
            });
        
// Clear the existing timepointsAverages
        timepointsAverages.Clear();

// Add a new DateTimePoint for each month to timepointsAverages
        foreach (var monthlyAverage in monthlyAverages)
        {
            timepointsAverages.Add(new DateTimePoint
            {
                DateTime = monthlyAverage.Month,
                Value = monthlyAverage.AverageTemperature
            });
        }
        */

var dailyAverages = WeatherDataList
    .GroupBy(wd => new DateTime(DateTime.Parse(wd.Time).Year, DateTime.Parse(wd.Time).Month, DateTime.Parse(wd.Time).Day))
    .Select(g => new
    {
        Day = g.Key,
        AverageTemperature = g.Average(wd => double.Parse(wd.Temperature, CultureInfo.InvariantCulture))
    });

timepointsAverages.Clear();

foreach (var average in dailyAverages)
{
    timepointsAverages.Add(new DateTimePoint
    {
        DateTime = average.Day,
        Value = average.AverageTemperature
    });
}
//TODO replace datetimepoint with only month
// Create a new ColumnSeries with timepointsAverages
        var columnSeries = new ColumnSeries<DateTimePoint>
        {
            Name = location.ShortName,
            Values = timepointsAverages,
            Stroke = new SolidColorPaint(randomColor),
            Fill = new SolidColorPaint(randomColor),
            MaxBarWidth = 10,
        };

           
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var averagesList = AverageTemperatures.ToList();
                averagesList.Add(columnSeries);
                AverageTemperatures = averagesList.ToArray();        
            });
    }
    
    public static SKColor GenerateRandomColor()
    {
        Random random = new Random();
        byte red = (byte)random.Next(256);
        byte green = (byte)random.Next(256);
        byte blue = (byte)random.Next(256);
        return new SKColor(red, green, blue);
    }
    
    public async Task RemoveLocation(string name)
    {
        var location = Locations.FirstOrDefault(l => l.ShortName == name);
        if (location != null)
        {
            Locations.Remove(location);                                                                            
            var toremove = AverageTemperatures.Where(t => t.Name == location.ShortName).First();                   
            var averageTemperaturesList = AverageTemperatures.ToList();                                            
            averageTemperaturesList.Remove(toremove);                                                              
            AverageTemperatures = averageTemperaturesList.ToArray();                 
            
            var toremove2 = TemperatureLines.Where(t => t.Name == location.ShortName).First();
            var temperatureLinesList = TemperatureLines.ToList();
            temperatureLinesList.Remove(toremove2);
            TemperatureLines = temperatureLinesList.ToArray();
        }
    }
#pragma warning restore CA1822 // Mark members as static
}

