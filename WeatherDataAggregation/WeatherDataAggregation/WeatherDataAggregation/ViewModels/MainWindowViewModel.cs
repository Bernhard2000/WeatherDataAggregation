using Avalonia.Controls;
using ReactiveUI;

namespace WeatherDataAggregation.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, (value as ListBoxItem).Tag);
        }
    }
}
