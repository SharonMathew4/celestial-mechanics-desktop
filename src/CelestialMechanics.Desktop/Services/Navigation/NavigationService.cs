using CommunityToolkit.Mvvm.ComponentModel;

namespace CelestialMechanics.Desktop.Services.Navigation;

public sealed partial class NavigationService : ObservableObject, INavigationService
{
    [ObservableProperty]
    private object? _currentViewModel;

    public void NavigateTo(object viewModel)
    {
        CurrentViewModel = viewModel;
    }
}
