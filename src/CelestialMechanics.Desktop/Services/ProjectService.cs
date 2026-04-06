using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CelestialMechanics.Desktop.Models;

namespace CelestialMechanics.Desktop.Services;

/// <summary>
/// Manages project creation, discovery, and persistence.
/// Projects are stored as directories containing a project.json metadata file.
/// </summary>
public class ProjectService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Returns the default root directory for projects.
    /// </summary>
    public static string GetDefaultProjectsRoot()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "CelestialMechanics");
    }

    /// <summary>
    /// Creates a new project directory and writes a project.json metadata file.
    /// </summary>
    public ProjectInfo CreateProject(string name, string location)
    {
        Directory.CreateDirectory(location);

        var project = new ProjectInfo
        {
            Name = name,
            Path = location,
            CreatedDate = DateTime.Now,
            LastModifiedDate = DateTime.Now
        };

        var json = JsonSerializer.Serialize(project, JsonOptions);
        File.WriteAllText(Path.Combine(location, "project.json"), json);

        return project;
    }

    /// <summary>
    /// Scans the default projects root for saved projects.
    /// Returns them sorted by last modified date (most recent first).
    /// </summary>
    public List<ProjectInfo> GetRecentProjects()
    {
        var root = GetDefaultProjectsRoot();
        var projects = new List<ProjectInfo>();

        if (!Directory.Exists(root))
            return projects;

        foreach (var dir in Directory.GetDirectories(root))
        {
            var metaPath = Path.Combine(dir, "project.json");
            if (!File.Exists(metaPath))
                continue;

            try
            {
                var json = File.ReadAllText(metaPath);
                var project = JsonSerializer.Deserialize<ProjectInfo>(json);
                if (project != null)
                {
                    project.Path = dir; // ensure path is current
                    projects.Add(project);
                }
            }
            catch
            {
                // Skip corrupted project files
            }
        }

        return projects.OrderByDescending(p => p.LastModifiedDate).ToList();
    }

    /// <summary>
    /// Opens a project from a specific directory path.
    /// </summary>
    public ProjectInfo? OpenProject(string path)
    {
        var metaPath = Path.Combine(path, "project.json");
        if (!File.Exists(metaPath))
            return null;

        try
        {
            var json = File.ReadAllText(metaPath);
            var project = JsonSerializer.Deserialize<ProjectInfo>(json);
            if (project != null)
            {
                project.Path = path;
                project.LastModifiedDate = DateTime.Now;
                // Update the last modified time
                var updatedJson = JsonSerializer.Serialize(project, JsonOptions);
                File.WriteAllText(metaPath, updatedJson);
            }
            return project;
        }
        catch
        {
            return null;
        }
    }
}
