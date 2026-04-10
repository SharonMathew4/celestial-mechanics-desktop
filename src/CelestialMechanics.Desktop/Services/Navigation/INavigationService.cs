using System.ComponentModel;

namespace CelestialMechanics.Desktop.Services.Navigation;

public interface INavigationService : INotifyPropertyChanged
{
    object? CurrentViewModel { get; }
    void NavigateTo(object viewModel);
}
