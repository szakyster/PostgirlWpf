using Postgirl.Domain.Persistence;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Postgirl.Services;

public class StorageService
{
    private const string LegacyFileName = "postgirl_state.json";
    private const string IndexFileName = "projects.json";
    private const string ProjectsFolder = "projects";

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    private string GetBaseDir()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Postgirl");
        Directory.CreateDirectory(dir);
        return dir;
    }

    private string GetProjectsDir()
    {
        var dir = Path.Combine(GetBaseDir(), ProjectsFolder);
        Directory.CreateDirectory(dir);
        return dir;
    }

    // Legacy path kept for migration reads
    private string GetLegacyPath() => Path.Combine(GetBaseDir(), LegacyFileName);

    private string GetIndexPath() => Path.Combine(GetBaseDir(), IndexFileName);

    private string GetProjectPath(string id) => Path.Combine(GetProjectsDir(), $"{id}.json");

    // ── Index ────────────────────────────────────────────────────────────────

    public void SaveIndex(ProjectsIndex index)
    {
        var path = GetIndexPath();
        var json = JsonSerializer.Serialize(index, _options);
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, json);
        File.Move(tmp, path, true);
    }

    public async Task<ProjectsIndex?> LoadIndexAsync()
    {
        var path = GetIndexPath();
        if (!File.Exists(path))
            return null;

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<ProjectsIndex>(json, _options);
    }

    // ── Project ──────────────────────────────────────────────────────────────

    public void SaveProject(ProjectState project)
    {
        var path = GetProjectPath(project.Id);
        var json = JsonSerializer.Serialize(project, _options);
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, json);
        File.Move(tmp, path, true);
    }

    public async Task<ProjectState?> LoadProjectAsync(string id)
    {
        var path = GetProjectPath(id);
        if (!File.Exists(path))
            return null;

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<ProjectState>(json, _options);
    }

    public void DeleteProject(string id)
    {
        var path = GetProjectPath(id);
        if (File.Exists(path))
            File.Delete(path);
    }

    // ── Configuration (global) ───────────────────────────────────────────────

    public void SaveConfiguration(AppState state)
    {
        var path = GetLegacyPath();
        var json = JsonSerializer.Serialize(state, _options);
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, json);
        File.Move(tmp, path, true);
    }

    public async Task<AppState> LoadConfigurationAsync()
    {
        var path = GetLegacyPath();
        if (!File.Exists(path))
            return new AppState();

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<AppState>(json, _options) ?? new AppState();
    }

    // ── Migration ────────────────────────────────────────────────────────────

    /// <summary>
    /// If no project index exists yet but the legacy state file does,
    /// creates a default project from the legacy data and writes the index.
    /// </summary>
    public async Task MigrateIfNeededAsync()
    {
        if (File.Exists(GetIndexPath()))
            return;

        var legacyState = await LoadConfigurationAsync();

        var defaultId = Guid.NewGuid().ToString();
        var defaultProject = new ProjectState
        {
            Id = defaultId,
            Name = "Default",
            IsDefault = true,
            History = legacyState.History,
            SavedRequests = legacyState.SavedRequests,
            OpenedDocuments = legacyState.OpenedDocuments,
            Variables = legacyState.Variables,
            ActiveSidebarPanel = legacyState.ActiveSidebarPanel
        };

        SaveProject(defaultProject);

        var index = new ProjectsIndex
        {
            ActiveProjectId = defaultId,
            Projects = [new ProjectSummary { Id = defaultId, Name = "Default", IsDefault = true }]
        };

        SaveIndex(index);
    }
}
