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
using LiveChartsCore.SkiaSharpView.VisualElements;
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
    
    private ISeries[] _temperatureForecastLines = new ISeries[] { };
    public ISeries[] TemperatureForecastLines
    {
        get => _temperatureForecastLines;
        set  => this.RaiseAndSetIfChanged(ref _temperatureForecastLines, value);
    }
    
    private ISeries[] _averageTemperatures = new ISeries[] { };
    public ISeries[] AverageTemperatures
    {
        get => _averageTemperatures;
        set => this.RaiseAndSetIfChanged(ref _averageTemperatures, value);
    }
    
    private ISeries[] _maximumTemperatures = new ISeries[] { };
    public ISeries[] MaximumTemperatures
    {
        get => _maximumTemperatures;
        set => this.RaiseAndSetIfChanged(ref _maximumTemperatures, value);
    }
    
    private ISeries[] _minimumTemperatures = new ISeries[] { };
    public ISeries[] MinimumTemperatures
    {
        get => _minimumTemperatures;
        set => this.RaiseAndSetIfChanged(ref _minimumTemperatures, value);
    }

    private ISeries[] _averageTemperaturesMonthly = new ISeries[] { };
    public ISeries[] AverageTemperaturesMonthly
    {
        get => _averageTemperaturesMonthly;
        set => this.RaiseAndSetIfChanged(ref _averageTemperaturesMonthly, value);
    }
    
    
    private Location _selectedLocation = new Location { Name = "Linz", Latitude = 48.3064, Longitude = 14.2861 };
    public Location SelectedLocation
    {
        get => _selectedLocation;
        set => this.RaiseAndSetIfChanged(ref _selectedLocation, value);
    }
    
    public Axis[] XAxes { get; set; } =
    {
        new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yyyy hh:mm"))
    };
    
    public Axis[] XAxesForecast { get; set; } =
    {
        new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yyyy hh:mm"))
    };
    
    
    public Axis[] XAxesAverages { get; set; } =
    {
        new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yyyy"))
    };

    public ReactiveCommand<Unit, Unit> CompareYearsCommand { get; }
    
    public ObservableCollection<Location> LocationSearchResults { get; set; } = new ObservableCollection<Location>();

    
    public MainViewModel()
    {
        RefreshCommand = ReactiveCommand.Create(AddLocation);
        SearchLocationCommand = ReactiveCommand.Create(SearchLocation);
        
        this.WhenAnyValue(x => x.DateFrom, x => x.DateTo)
            .Subscribe(_ => OnDateChanged());
    }
    
    private async Task OnDateChanged()
    {
        TemperatureLines = new ISeries[] { };
        AverageTemperatures = new ISeries[] { };
        MaximumTemperatures = new ISeries[] { };
        MinimumTemperatures = new ISeries[] { };


        var tempTemperatureLines = new ConcurrentBag<ISeries>();
        var tempAverageTemperatures = new ConcurrentBag<ISeries>();
        var tempMaxTemperatures = new ConcurrentBag<ISeries>();
        var tempMinTemperatures = new ConcurrentBag<ISeries>();

        var tempAverageTemperaturesMonthly = new ConcurrentBag<ISeries>();


        await Parallel.ForEachAsync(Locations, async (location, cancellationToken) =>
        {
            var fetchHourlyTask = FetchWeatherDataHourly(location);
            var fetchDailyTask = FetchWeatherDataDaily(location);
            var fetchForecastTask = Open_Meteo.FetchForecastData(location);

            //await Task.WhenAll(fetchHourlyTask, fetchDailyTask);
            
            var historicDataHourly = await fetchHourlyTask;
            var historicDataDaily = await fetchDailyTask;
            var forecastData = await fetchForecastTask;


            
            var color = GenerateRandomColor();
           
           var temperatureSeriesTask = Task.Run(() => CalculateTemperatureHourlySeries(historicDataHourly, color, location));
           var averageTemperatureColumnsTask = Task.Run(() => CalculateMeanTemperatureColumns(historicDataDaily, color, location));
           var minTemperatureColumnsTask = Task.Run(() => CalculateMinTemperatureColumns(historicDataDaily, color, location));
           var maxTemperatureColumnsTask = Task.Run(() => CalculateMaxTemperatureColumns(historicDataDaily, color, location));
           var averageTemperatureMonthlyColumnsTask = Task.Run(() => CalculateAverageTemperatureByMonthColumns(historicDataDaily, color, location));
            var temperatureForecastTask = Task.Run(() => CalculateTemperatureHourlySeries(forecastData, color, location));
           //await Task.WhenAll(temperatureSeriesTask, averageTemperatureColumnsTask, minTemperatureColumnsTask, maxTemperatureColumnsTask, averageTemperatureMonthlyColumnsTask);

                
            tempTemperatureLines.Add(await temperatureSeriesTask);
            tempAverageTemperatures.Add(await averageTemperatureColumnsTask);
            tempMaxTemperatures.Add(await maxTemperatureColumnsTask);
            tempMinTemperatures.Add(await minTemperatureColumnsTask);
            tempAverageTemperaturesMonthly.Add(await  averageTemperatureMonthlyColumnsTask);
        });

        TemperatureLines = tempTemperatureLines.ToArray();
        AverageTemperatures = tempAverageTemperatures.ToArray();
        MaximumTemperatures = tempMaxTemperatures.ToArray();
        MinimumTemperatures = tempMinTemperatures.ToArray();
        AverageTemperaturesMonthly = tempAverageTemperaturesMonthly.ToArray();
    }
    
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
    
    public async Task<WeatherData[]> FetchWeatherDataDaily(Location location)
    {
        WeatherData[] historicData;
        DateTime from = DateFrom.DateTime;
        DateTime to = DateTo.DateTime;

        try
        {
            historicData = await Open_Meteo.fetchHistoricDataDaily(location, from, to);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }

        return historicData;
    }

    public async Task<WeatherData[]> FetchWeatherDataHourly(Location location)
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
    public ISeries CalculateTemperatureHourlySeries(WeatherData[] historicData, SKColor color, Location location)
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
            Name= location.ShortName
        };

        return series;
    }
    
    public ISeries CalculateMeanTemperatureColumns(WeatherData[] historicData, SKColor color, Location location)
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

        var columnSeries = new ColumnSeries<DateTimePoint>
        {
            Name = location.ShortName,
            Values = timepoints,
            Stroke = new SolidColorPaint(color),
            Fill = new SolidColorPaint(color),
            MaxBarWidth = 10,
        };
        return columnSeries;
    }
    
    public ISeries CalculateMaxTemperatureColumns(WeatherData[] historicData, SKColor color, Location location)
    {
        var temperatures = historicData.Select(wd => double.Parse(wd.MaximumTemperature, CultureInfo.InvariantCulture)).ToArray();
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

        var columnSeries = new ColumnSeries<DateTimePoint>
        {
            Name = location.ShortName,
            Values = timepoints,
            Stroke = new SolidColorPaint(color),
            Fill = new SolidColorPaint(color),
            MaxBarWidth = 10,
        };
        return columnSeries;
    }
    
    public ISeries CalculateMinTemperatureColumns(WeatherData[] historicData, SKColor color, Location location)
    {
        var temperatures = historicData.Select(wd => double.Parse(wd.MinimumTemperature, CultureInfo.InvariantCulture)).ToArray();
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

        var columnSeries = new ColumnSeries<DateTimePoint>
        {
            Name = location.ShortName,
            Values = timepoints,
            Stroke = new SolidColorPaint(color),
            Fill = new SolidColorPaint(color),
            MaxBarWidth = 10,
        };
        return columnSeries;
    }
    
    public ISeries CalculateAverageTemperatureByMonthColumns(WeatherData[] historicData, SKColor color, Location location)
    {
        var monthlyAverages = historicData
            .GroupBy(wd => new DateTime(DateTime.Parse(wd.Time).Year, DateTime.Parse(wd.Time).Month, 1))
            .Select(g => new
            {
                Month = g.Key,
                AverageTemperature = g.Average(wd => double.Parse(wd.Temperature, CultureInfo.InvariantCulture))
            });

        var timepointsAverages = new ObservableCollection<DateTimePoint>();

        foreach (var average in monthlyAverages)
        {
            timepointsAverages.Add(new DateTimePoint
            {
                DateTime = average.Month,
                Value = average.AverageTemperature
            });
        }

        var columnSeries = new ColumnSeries<DateTimePoint>
        {
            Name = SelectedLocation.ShortName,
            Values = timepointsAverages,
            Stroke = new SolidColorPaint(color),
            Fill = new SolidColorPaint(color),
            MaxBarWidth = 20,
        };

        return columnSeries;
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
    
    public LabelVisual TitleAverageTemperature { get; set; } =
        new LabelVisual
        {
            Text = "Average Temperature",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
            Paint = new SolidColorPaint(SKColors.DarkSlateGray)
        };
    
    public LabelVisual TitleMinimumTemperature { get; set; } =
        new LabelVisual
        {
            Text = "Minimum Temperature",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
            Paint = new SolidColorPaint(SKColors.DarkSlateGray)
        };
    
    public LabelVisual TitleMaximumTemperature { get; set; } =
        new LabelVisual
        {
            Text = "Maximum Temperature",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
            Paint = new SolidColorPaint(SKColors.DarkSlateGray)
        };


    
    public LabelVisual TitleTemperature { get; set; } =
        new LabelVisual
        {
            Text = "Temperature",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
            Paint = new SolidColorPaint(SKColors.DarkSlateGray)
        };

    public async void GetWeatherData(Location location)
    {
        var fetchHourlyTask = FetchWeatherDataHourly(location);
        var fetchDailyTask = FetchWeatherDataDaily(location);
        var fetchForecastTask = Open_Meteo.FetchForecastData(location);

        //await Task.WhenAll(fetchHourlyTask, fetchDailyTask);

        var historicDataHourly = await fetchHourlyTask;
        var historicDataDaily = await fetchDailyTask;
        var forecastData = await fetchForecastTask;

        var color = GenerateRandomColor();
        var temperatureSeriesTask = Task.Run(() => CalculateTemperatureHourlySeries(historicDataHourly, color, location));
        var averageTemperatureColumnsTask = Task.Run(() => CalculateMeanTemperatureColumns(historicDataDaily, color, location));
        var minTemperatureColumnsTask = Task.Run(() => CalculateMinTemperatureColumns(historicDataDaily, color, location));
        var maxTemperatureColumnsTask = Task.Run(() => CalculateMaxTemperatureColumns(historicDataDaily, color, location));
        var averageTemperatureMonthlyColumnsTask = Task.Run(() => CalculateAverageTemperatureByMonthColumns(historicDataDaily, color, location));
        var temperatureForecastTask = Task.Run(() => CalculateTemperatureHourlySeries(forecastData, color, location));
        //await Task.WhenAll(temperatureSeriesTask, averageTemperatureColumnsTask, minTemperatureColumnsTask, maxTemperatureColumnsTask, averageTemperatureMonthlyColumnsTask);

                
        var temperatureSeries = await temperatureSeriesTask;
        var averageTemperatureColumns = await averageTemperatureColumnsTask;
        var minimumTemperatureColumns = await minTemperatureColumnsTask;
        var maximumTemperatureColumns = await maxTemperatureColumnsTask;
        var averageTemperatureMonthColumns = await  averageTemperatureMonthlyColumnsTask;
        var temperatureForecastSeries = await temperatureForecastTask;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var linesList = TemperatureLines.ToList();
            linesList.Add(temperatureSeries);
            TemperatureLines = linesList.ToArray();

            var averagesList = AverageTemperatures.ToList();
            averagesList.Add(averageTemperatureColumns);
            AverageTemperatures = averagesList.ToArray();
            
            var maxList = MaximumTemperatures.ToList();
            maxList.Add(maximumTemperatureColumns);
            MaximumTemperatures = maxList.ToArray();
            
            var minList = MinimumTemperatures.ToList();
            minList.Add(minimumTemperatureColumns);
            MinimumTemperatures = minList.ToArray();

            var monthlyAverageTempList = AverageTemperaturesMonthly.ToList();
            monthlyAverageTempList.Add(averageTemperatureMonthColumns);
            AverageTemperaturesMonthly = monthlyAverageTempList.ToArray();
            
            var forecastList = TemperatureForecastLines.ToList();
            forecastList.Add(temperatureForecastSeries);
            TemperatureForecastLines = forecastList.ToArray();
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

