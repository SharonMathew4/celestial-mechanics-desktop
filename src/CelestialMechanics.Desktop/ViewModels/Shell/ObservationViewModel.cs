using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CelestialMechanics.Desktop.Services.Data;

namespace CelestialMechanics.Desktop.ViewModels.Shell;

public sealed partial class ObservationViewModel : ObservableObject
{
    private readonly DataService _dataService;

    [ObservableProperty]
    private string _status = "Observation mode ready";

    public ObservationViewModel(DataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        Status = "Fetching JPL Horizons data...";
        Status = await _dataService.FetchHorizonsSummaryAsync();
    }
}
