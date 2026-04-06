using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CelestialMechanics.Desktop.Models;
using CelestialMechanics.Desktop.Services;

namespace CelestialMechanics.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Projects List modal.
/// Displays saved projects and lets the user select one to open.
/// </summary>
public sealed partial class ProjectsListViewModel : ObservableObject
{
    private readonly ProjectService _projectService;

    /// <summary>Raised when a project is selected and opened.</summary>
    public event Action<ProjectInfo>? ProjectOpened;

    /// <summary>Raised when the user cancels.</summary>
    public event Action? CancelRequested;

    public ObservableCollection<ProjectInfo> Projects { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedProject))]
    [NotifyCanExecuteChangedFor(nameof(OpenSelectedProjectCommand))]
    private ProjectInfo? _selectedProject;

    public bool HasSelectedProject => SelectedProject != null;

    public ProjectsListViewModel(ProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>Refreshes the project list from disk.</summary>
    public void RefreshProjects()
    {
        Projects.Clear();
        foreach (var project in _projectService.GetRecentProjects())
        {
            Projects.Add(project);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelectedProject))]
    private void OpenSelectedProject()
    {
        if (SelectedProject == null) return;

        var opened = _projectService.OpenProject(SelectedProject.Path);
        if (opened != null)
        {
            ProjectOpened?.Invoke(opened);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CancelRequested?.Invoke();
    }
}
