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


    
}