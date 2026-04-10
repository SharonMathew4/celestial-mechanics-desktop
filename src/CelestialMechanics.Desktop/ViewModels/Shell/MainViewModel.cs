using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CelestialMechanics.Desktop.Models;
using CelestialMechanics.Desktop.Services.Navigation;
using CelestialMechanics.Desktop.ViewModels;

namespace CelestialMechanics.Desktop.ViewModels.Shell;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly SimulationViewModel _simulationViewModel;
    private readonly ObservationViewModel _observationViewModel;
    private readonly ModeSelectionViewModel _modeSelectionViewModel;
    private readonly SimulationMenuViewModel _simulationMenuViewModel;
    private readonly ProjectsListViewModel _projectsListViewModel;
    private readonly NewProjectViewModel _newProjectViewModel;
    private readonly FileMenuViewModel _fileMenuViewModel;

    [ObservableProperty]
    private object? _currentViewModel;

    [ObservableProperty]
    private bool _showTopNavigation;

    [ObservableProperty]
    private string _windowTitle = "Celestial Mechanics";

    public MainViewModel(
        INavigationService navigationService,
        SimulationViewModel simulationViewModel,
        ObservationViewModel observationViewModel,
        ModeSelectionViewModel modeSelectionViewModel,
        SimulationMenuViewModel simulationMenuViewModel,
        ProjectsListViewModel projectsListViewModel,
        NewProjectViewModel newProjectViewModel,
        FileMenuViewModel fileMenuViewModel)
    {
        _navigationService = navigationService;
        _simulationViewModel = simulationViewModel;
        _observationViewModel = observationViewModel;
        _modeSelectionViewModel = modeSelectionViewModel;
        _simulationMenuViewModel = simulationMenuViewModel;
        _projectsListViewModel = projectsListViewModel;
        _newProjectViewModel = newProjectViewModel;
        _fileMenuViewModel = fileMenuViewModel;

        _modeSelectionViewModel.SimulationSelected += OnSimulationSelected;
        _modeSelectionViewModel.ExitRequested += OnExitRequested;

        _simulationMenuViewModel.NewProjectRequested += OnNewProjectRequested;
        _simulationMenuViewModel.ProjectsRequested += OnProjectsRequested;
        _simulationMenuViewModel.FileRequested += OnFileRequested;
        _simulationMenuViewModel.BackRequested += () => _navigationService.NavigateTo(_modeSelectionViewModel);

        _projectsListViewModel.ProjectOpened += OnProjectOpened;
        _projectsListViewModel.CancelRequested += () => _navigationService.NavigateTo(_simulationMenuViewModel);

        _newProjectViewModel.ProjectCreated += OnProjectOpened;
        _newProjectViewModel.CancelRequested += () => _navigationService.NavigateTo(_simulationMenuViewModel);

        _fileMenuViewModel.BackRequested += () => _navigationService.NavigateTo(_simulationMenuViewModel);

        _navigationService.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(INavigationService.CurrentViewModel))
            {
                CurrentViewModel = _navigationService.CurrentViewModel;
            }
        };

        _navigationService.NavigateTo(_modeSelectionViewModel);
        ShowTopNavigation = false;
    }

    private void OnSimulationSelected()
    {
        ShowTopNavigation = false;
        _navigationService.NavigateTo(_simulationMenuViewModel);
    }

    private static void OnExitRequested()
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void OnNewProjectRequested()
    {
        _newProjectViewModel.Reset();
        _navigationService.NavigateTo(_newProjectViewModel);
    }

    private void OnProjectsRequested()
    {
        _projectsListViewModel.RefreshProjects();
        _navigationService.NavigateTo(_projectsListViewModel);
    }

    private void OnFileRequested()
    {
        _navigationService.NavigateTo(_fileMenuViewModel);
    }

    private void OnProjectOpened(ProjectInfo project)
    {
        WindowTitle = $"Celestial Mechanics - {project.Name}";
        ShowTopNavigation = true;
        _simulationViewModel.EnsureStarted();
        _navigationService.NavigateTo(_simulationViewModel);
    }

    [RelayCommand]
    private void NavigateSimulation()
    {
        if (!ShowTopNavigation)
        {
            return;
        }

        _navigationService.NavigateTo(_simulationViewModel);
    }

    [RelayCommand]
    private void NavigateObservation()
    {
        if (!ShowTopNavigation)
        {
            return;
        }

        _navigationService.NavigateTo(_observationViewModel);
    }
}
