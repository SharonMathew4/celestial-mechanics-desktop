using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CelestialMechanics.Desktop.Models;
using CelestialMechanics.Desktop.Services;

namespace CelestialMechanics.Desktop.ViewModels;

/// <summary>
/// ViewModel for the New Project creation modal.
/// Collects project name and save location, then creates the project.
/// </summary>
public sealed partial class NewProjectViewModel : ObservableObject
{
    private readonly ProjectService _projectService;

    /// <summary>Raised when a project is successfully created.</summary>
    public event Action<ProjectInfo>? ProjectCreated;

    /// <summary>Raised when the user cancels.</summary>
    public event Action? CancelRequested;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateProject))]
    [NotifyCanExecuteChangedFor(nameof(CreateProjectCommand))]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _projectLocation = string.Empty;

    public bool CanCreateProject => !string.IsNullOrWhiteSpace(ProjectName);

    public NewProjectViewModel(ProjectService projectService)
    {
        _projectService = projectService;
        _projectLocation = ProjectService.GetDefaultProjectsRoot();
    }

    partial void OnProjectNameChanged(string value)
    {
        // Auto-update location when the user types a project name
        var root = ProjectService.GetDefaultProjectsRoot();
        if (!string.IsNullOrWhiteSpace(value))
        {
            // Sanitize the name for use as a directory
            var safeName = string.Join("_", value.Split(Path.GetInvalidFileNameChars()));
            ProjectLocation = Path.Combine(root, safeName);
        }
        else
        {
            ProjectLocation = root;
        }
    }

    [RelayCommand]
    private void BrowseLocation()
    {
        // Use WPF's OpenFolderDialog (.NET 8+)
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select Project Location",
            InitialDirectory = ProjectService.GetDefaultProjectsRoot()
        };

        if (dialog.ShowDialog() == true)
        {
            ProjectLocation = dialog.FolderName;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateProject))]
    private void CreateProject()
    {
        var project = _projectService.CreateProject(ProjectName, ProjectLocation);
        ProjectCreated?.Invoke(project);
    }

    [RelayCommand]
    private void Cancel()
    {
        CancelRequested?.Invoke();
    }

    /// <summary>Reset the form for a fresh new project entry.</summary>
    public void Reset()
    {
        ProjectName = string.Empty;
        ProjectLocation = ProjectService.GetDefaultProjectsRoot();
    }
}
