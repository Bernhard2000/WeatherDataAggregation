using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using WeatherDataAggregation.ViewModels;

namespace WeatherDataAggregation.Views;

public partial class MainView : UserControl
{
    private MainViewModel viewModel;
    public MainView()
    {
        InitializeComponent();
    }


    private void SearchLocation(object? sender, RoutedEventArgs e)
    {
        viewModel = DataContext as MainViewModel;
        GeoCoding.GetLocationAsync(viewModel.LocationSearchQuery).ContinueWith((result) =>
        {
            if(result.Status == TaskStatus.Faulted)
            {
                return;
            }
            viewModel.LocationSearchResults.Clear();
            foreach (var location in result.Result)
            {
                viewModel.LocationSearchResults.Add(location);
            }
            viewModel.SelectedLocation = viewModel.LocationSearchResults[0];
        });
        viewModel.LocationSearchQuery = "";
    }

    private void FetchOpenWeatherMapTest(object? sender, RoutedEventArgs e)
    {
        viewModel = DataContext as MainViewModel;
        OpenWeatherMap.fetchCurrentData(viewModel.SelectedLocation);
    }
}