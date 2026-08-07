using Postgirl.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Postgirl.Services;

public class ProjectService
{
    private readonly StorageService _storage;
    private readonly HistoryService _history;
    private readonly SavedRequestService _savedRequests;
    private readonly VariablesService _variables;
    private readonly ConfigurationService _configuration;

    public ProjectState ActiveProject { get; private set; } = null!;
    public ObservableCollection<ProjectSummary> Projects { get; private set; } = [];

    public ProjectService(
        StorageService storage,
        HistoryService history,
        SavedRequestService savedRequests,
        VariablesService variables,
        ConfigurationService configuration)
    {
        _storage = storage;
        _history = history;
        _savedRequests = savedRequests;
        _variables = variables;
        _configuration = configuration;
    }

    // ── Initialization ────────────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        await _storage.MigrateIfNeededAsync();

        var index = await _storage.LoadIndexAsync();
        if (index is null || index.Projects.Count == 0)
        {
            index = CreateFreshIndex();
            var defaultProject = CreateDefaultProject(index.ActiveProjectId);
            _storage.SaveProject(defaultProject);
            _storage.SaveIndex(index);
        }

        SanitizeIndex(index);
        _storage.SaveIndex(index);

        Projects = new ObservableCollection<ProjectSummary>(index.Projects);

        var active = await _storage.LoadProjectAsync(index.ActiveProjectId)
                     ?? CreateDefaultProject(index.ActiveProjectId);

        // Ensure the ProjectState name is in sync with the index summary
        var activeSummary = Projects.FirstOrDefault(p => p.Id == active.Id);
        if (activeSummary is not null && string.IsNullOrWhiteSpace(active.Name))
        {
            active.Name = activeSummary.Name;
        }

        LoadProjectIntoServices(active);
    }

    // ── Switching ─────────────────────────────────────────────────────────────

    public async Task SwitchProjectAsync(string id)
    {
        if (ActiveProject?.Id == id)
            return;

        SaveActiveProject();
        var project = await _storage.LoadProjectAsync(id)
                      ?? throw new InvalidOperationException($"Project '{id}' not found.");

        var summary = Projects.FirstOrDefault(p => p.Id == project.Id);
        if (summary is not null && string.IsNullOrWhiteSpace(project.Name))
        {
            project.Name = summary.Name;
        }

        LoadProjectIntoServices(project);
        UpdateActiveInIndex(id);
    }

    // ── CRUD ──────────────────────────────────────────────────────────────────

    public string CreateProject(string name)
    {
        EnsureUniqueName(name);

        var id = Guid.NewGuid().ToString();
        var project = new ProjectState { Id = id, Name = name, IsDefault = false };
        _storage.SaveProject(project);

        Projects.Add(new ProjectSummary { Id = id, Name = name, IsDefault = false });
        SaveIndex();

        return id;
    }

    public async Task<string> DuplicateProjectAsync(string sourceId, string name)
    {
        EnsureUniqueName(name);

        ProjectState source;
        if (ActiveProject.Id == sourceId)
        {
            source = ExportActiveProject();
        }
        else
        {
            source = await _storage.LoadProjectAsync(sourceId)
                     ?? throw new InvalidOperationException($"Project '{sourceId}' not found.");
        }

        var newId = Guid.NewGuid().ToString();
        var copy = new ProjectState
        {
            Id = newId,
            Name = name,
            IsDefault = false,
            History = source.History,
            SavedRequests = source.SavedRequests,
            OpenedDocuments = source.OpenedDocuments,
            Variables = source.Variables,
            ActiveSidebarPanel = source.ActiveSidebarPanel
        };

        _storage.SaveProject(copy);
        Projects.Add(new ProjectSummary { Id = newId, Name = name, IsDefault = false });
        SaveIndex();

        return newId;
    }

    public void DeleteProject(string id)
    {
        var summary = Projects.FirstOrDefault(p => p.Id == id)
                      ?? throw new InvalidOperationException($"Project '{id}' not found.");

        if (summary.IsDefault)
            throw new InvalidOperationException("The default project cannot be deleted.");

        _storage.DeleteProject(id);
        Projects.Remove(summary);
        SaveIndex();
    }

    public void RenameProject(string id, string newName)
    {
        var summary = Projects.FirstOrDefault(p => p.Id == id)
                      ?? throw new InvalidOperationException($"Project '{id}' not found.");

        if (summary.IsDefault)
            throw new InvalidOperationException("The default project cannot be renamed.");

        EnsureUniqueName(newName, excludeId: id);

        summary.Name = newName;

        if (ActiveProject.Id == id)
            ActiveProject.Name = newName;

        SaveIndex();
    }

    // ── Persistence helpers ───────────────────────────────────────────────────

    public void SaveActiveProject()
    {
        if (ActiveProject is null)
            return;

        var snapshot = ExportActiveProject();
        _storage.SaveProject(snapshot);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void LoadProjectIntoServices(ProjectState project)
    {
        ActiveProject = project;

        _history.Import(
            _configuration.GetStorageKeepHistoryBetweenSessions()
                ? project.History
                : []);

        _savedRequests.Import(project.SavedRequests);
        _variables.Import(project.Variables);
    }

    private ProjectState ExportActiveProject()
    {
        return new ProjectState
        {
            Id = ActiveProject.Id,
            Name = ActiveProject.Name,
            IsDefault = ActiveProject.IsDefault,
            History = _configuration.GetStorageKeepHistoryBetweenSessions()
                ? _history.Export()
                : [],
            SavedRequests = _savedRequests.Export(),
            Variables = _variables.Export(),
            OpenedDocuments = ActiveProject.OpenedDocuments,
            ActiveSidebarPanel = ActiveProject.ActiveSidebarPanel
        };
    }

    private void UpdateActiveInIndex(string id)
    {
        var index = new ProjectsIndex { ActiveProjectId = id, Projects = Projects.ToList() };
        _storage.SaveIndex(index);
    }

    private void SaveIndex()
    {
        _storage.SaveIndex(new ProjectsIndex
        {
            ActiveProjectId = ActiveProject?.Id ?? Projects.FirstOrDefault()?.Id ?? string.Empty,
            Projects = Projects.ToList()
        });
    }

    private void EnsureUniqueName(string name, string? excludeId = null)
    {
        var conflict = Projects.Any(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            && p.Id != excludeId);

        if (conflict)
            throw new InvalidOperationException($"A project named '{name}' already exists.");
    }

    /// <summary>
    /// Repairs structural invariants of the project index:
    /// 1. At least one project exists (creates default if empty).
    /// 2. Exactly one project is marked as default (first default wins, rest are demoted).
    /// 3. The default project is named "Default".
    /// </summary>
    private void SanitizeIndex(ProjectsIndex index)
    {
        // Rule 1: at least one project
        if (index.Projects.Count == 0)
        {
            var id = Guid.NewGuid().ToString();
            var summary = new ProjectSummary { Id = id, Name = "Default", IsDefault = true };
            index.Projects.Add(summary);
            _storage.SaveProject(CreateDefaultProject(id));

            if (string.IsNullOrWhiteSpace(index.ActiveProjectId))
                index.ActiveProjectId = id;
        }

        // Rule 2: exactly one default
        var defaults = index.Projects.Where(p => p.IsDefault).ToList();
        if (defaults.Count == 0)
        {
            index.Projects[0].IsDefault = true;
        }
        else if (defaults.Count > 1)
        {
            foreach (var extra in defaults.Skip(1))
                extra.IsDefault = false;
        }

        // Rule 3: the default project must be named "Default"
        var defaultSummary = index.Projects.First(p => p.IsDefault);
        if (!string.Equals(defaultSummary.Name, "Default", StringComparison.Ordinal))
        {
            defaultSummary.Name = "Default";
        }

        // Ensure ActiveProjectId points to a valid project
        if (index.Projects.All(p => p.Id != index.ActiveProjectId))
            index.ActiveProjectId = index.Projects.First(p => p.IsDefault).Id;
    }

    private static ProjectsIndex CreateFreshIndex()
    {
        var id = Guid.NewGuid().ToString();
        return new ProjectsIndex
        {
            ActiveProjectId = id,
            Projects = [new ProjectSummary { Id = id, Name = "Default", IsDefault = true }]
        };
    }

    private static ProjectState CreateDefaultProject(string id) => new()
    {
        Id = id,
        Name = "Default",
        IsDefault = true
    };
}
