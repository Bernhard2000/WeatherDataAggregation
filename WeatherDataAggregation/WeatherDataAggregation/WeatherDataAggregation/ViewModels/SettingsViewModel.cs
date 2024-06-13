using ReactiveUI;

namespace WeatherDataAggregation.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private string[] _availableAPIs = { "OpenWeatherMap", "OpenMeteo" };
    private string _selectedWeatherService = "OpenWeatherMap";
    private string _openMeteoApiKey;
    private string _openweathermapApiKey;

    public string[] AvailableAPIs => _availableAPIs;
    public string SelectedWeatherService
    {
        get => _selectedWeatherService;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedWeatherService, value); 
            Settings.SelectedWeatherService = value;
        }
    }

    public string OpenMeteoApiKey
    {
        get => _openMeteoApiKey;
        set
        {
            this.RaiseAndSetIfChanged(ref _openMeteoApiKey, value);
            Settings.OpenMeteoApiKey = value;
        }
    }

    public string OpenweathermapApiKey
    {
        get => _openweathermapApiKey;
        set
        {
            this.RaiseAndSetIfChanged(ref _openweathermapApiKey, value); 
            Settings.OpenweathermapApiKey = value;
        }
    }
}