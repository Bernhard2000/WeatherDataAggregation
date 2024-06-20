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


    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        viewModel = DataContext as MainViewModel;

        string locationName = (sender as Button).Tag as string;
        viewModel.RemoveLocation(locationName);
    }
}