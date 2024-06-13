using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using WeatherDataAggregation.ViewModels;

namespace WeatherDataAggregation.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void Save(object? sender, RoutedEventArgs e)
    {
        var settings = DataContext as SettingsViewModel;
        // Serialize the SettingsViewModel instance to a JSON string
        string json = JsonSerializer.Serialize(settings);

        // Write the JSON string to a file
        File.WriteAllText("settings.json", json);

    }
}