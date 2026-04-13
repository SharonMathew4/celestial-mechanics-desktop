using System.IO;
using System.Text.Json;
using CelestialMechanics.Desktop.Models;

namespace CelestialMechanics.Desktop.Services;

/// <summary>
/// Persists and resolves local simulation projects.
/// </summary>
public sealed class ProjectService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private static readonly object Sync = new();

    public static string GetDefaultProjectsRoot()
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "CelestialMechanicsProjects");
        Directory.CreateDirectory(root);
        return root;
    }

    private static string GetRecentProjectsFilePath() =>
        Path.Combine(GetDefaultProjectsRoot(), ".recent-projects.json");

    public ProjectInfo CreateProject(string name, string location)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name is required.", nameof(name));
        }

        var projectPath = string.IsNullOrWhiteSpace(location)
            ? Path.Combine(GetDefaultProjectsRoot(), SanitizeName(name))
            : location;

        Directory.CreateDirectory(projectPath);

        var project = new ProjectInfo
        {
            Name = name.Trim(),
            Path = projectPath,
            CreatedAtUtc = DateTime.UtcNow,
            LastOpenedAtUtc = DateTime.UtcNow,
        };

        WriteManifest(project);
        AddOrUpdateRecent(project);
        return project;
    }

    public List<ProjectInfo> GetRecentProjects()
    {
        lock (Sync)
        {
            return ReadRecentProjectsUnsafe();
        }
    }

    public ProjectInfo? OpenProject(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            return null;
        }

        var manifestPath = GetManifestPath(path);
        ProjectInfo? project = null;

        if (File.Exists(manifestPath))
        {
            try
            {
                var json = File.ReadAllText(manifestPath);
                project = JsonSerializer.Deserialize<ProjectInfo>(json);
            }
            catch
            {
                project = null;
            }
        }

        project ??= new ProjectInfo
        {
            Name = new DirectoryInfo(path).Name,
            Path = path,
            CreatedAtUtc = DateTime.UtcNow,
            LastOpenedAtUtc = DateTime.UtcNow,
        };

        project.LastOpenedAtUtc = DateTime.UtcNow;
        WriteManifest(project);
        AddOrUpdateRecent(project);
        return project;
    }

    private static string GetManifestPath(string projectPath) =>
        Path.Combine(projectPath, "project.json");

    private static void WriteManifest(ProjectInfo project)
    {
        var manifestPath = GetManifestPath(project.Path);
        var json = JsonSerializer.Serialize(project, JsonOptions);
        File.WriteAllText(manifestPath, json);
    }

    private static void AddOrUpdateRecent(ProjectInfo project)
    {
        lock (Sync)
        {
            var recent = ReadRecentProjectsUnsafe();
            recent.RemoveAll(p => string.Equals(p.Path, project.Path, StringComparison.OrdinalIgnoreCase));
            recent.Insert(0, project);

            const int maxRecent = 25;
            if (recent.Count > maxRecent)
            {
                recent = recent.Take(maxRecent).ToList();
            }

            var json = JsonSerializer.Serialize(recent, JsonOptions);
            File.WriteAllText(GetRecentProjectsFilePath(), json);
        }
    }

    private static List<ProjectInfo> ReadRecentProjectsUnsafe()
    {
        var file = GetRecentProjectsFilePath();
        if (!File.Exists(file))
        {
            return new List<ProjectInfo>();
        }

        try
        {
            var json = File.ReadAllText(file);
            var projects = JsonSerializer.Deserialize<List<ProjectInfo>>(json) ?? new List<ProjectInfo>();
            return projects
                .Where(p => !string.IsNullOrWhiteSpace(p.Path) && Directory.Exists(p.Path))
                .OrderByDescending(p => p.LastOpenedAtUtc)
                .ToList();
        }
        catch
        {
            return new List<ProjectInfo>();
        }
    }

    private static string SanitizeName(string input)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(input.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "Project" : cleaned;
    }
}
