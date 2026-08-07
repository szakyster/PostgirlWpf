using Postgirl.Common;
using Postgirl.Domain.Configuration;
using Postgirl.Domain.History;
using Postgirl.Domain.Http;
using Postgirl.Domain.Persistence;
using Postgirl.Domain.SavedRequests;
using Postgirl.Services;
using Postgirl.Services.Execution;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Postgirl.Presentation.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IHttpExecutor _httpExecutor;
    private readonly HistoryService _historyService;
    private readonly SavedRequestService _savedRequestService;
    private readonly ConfigurationService _configurationService;
    private readonly ProjectService _projectService;

    public HistoryViewModel HistoryViewModel { get; }
    public VariablesViewModel VariablesViewModel { get; }
    public ObservableCollection<SavedRequestEntry> SavedRequests
        => _savedRequestService.Items;

    private string _activeSidebarPanel = "SavedExpander";

    public string ActiveSidebarPanel
    {
        get => _activeSidebarPanel;
        set
        {
            var panel = value;

            if (!IsVariablesPanelVisible && string.Equals(value, "VariablesExpander", StringComparison.Ordinal))
            {
                panel = "SavedExpander";
            }

            SetProperty(ref _activeSidebarPanel, panel);
        }
    }

    private bool _isVariablesPanelVisible;

    public bool IsVariablesPanelVisible
    {
        get => _isVariablesPanelVisible;
        private set => SetProperty(ref _isVariablesPanelVisible, value);
    }

    public string AppVersion
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version == null ? "v?.?.?" : $"v{version.Major}.{version.Minor}.{version.Build}-alpha";
        }
    }

    public MainViewModel(
        IHttpExecutor httpExecutor,
        HistoryService historyService,
        SavedRequestService savedRequestService,
        VariablesService variablesService,
        ConfigurationService configurationService,
        ProjectService projectService)
    {
        _savedRequestService = savedRequestService;
        _configurationService = configurationService;
        _httpExecutor = httpExecutor;
        _historyService = historyService;
        _projectService = projectService;
        IsVariablesPanelVisible = configurationService.GetVariablesEnabled();
        _configurationService.ConfigurationChanged += OnConfigurationChanged;

        HistoryViewModel = new HistoryViewModel(historyService, this);
        VariablesViewModel = new VariablesViewModel(variablesService);

        NewTabCommand = new RelayCommand(AddNewDocument);
        CloseDocumentCommand = new RelayCommand<RequestDocumentViewModel>(CloseDocument);
        DeleteSavedRequestCommand = new RelayCommand<SavedRequestEntry>(DeleteSavedRequest);
        SwitchProjectCommand = new RelayCommand<string>(async id => await SwitchProjectAsync(id));
        CreateProjectCommand = new RelayCommand<string>(name => CreateProject(name));
        DeleteProjectCommand = new RelayCommand<string>(id => DeleteProject(id));
        RenameProjectCommand = new RelayCommand<(string id, string name)>(args => RenameProject(args.id, args.name));
    }

    private void OnConfigurationChanged(string key)
    {
        if (!string.Equals(key, ConfigurationKeys.VariablesEnabled, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        IsVariablesPanelVisible = _configurationService.GetVariablesEnabled();

        if (!IsVariablesPanelVisible && string.Equals(ActiveSidebarPanel, "VariablesExpander", StringComparison.Ordinal))
        {
            ActiveSidebarPanel = "SavedExpander";
        }
    }

    // ── Project ───────────────────────────────────────────────────────────────

    public ICommand SwitchProjectCommand { get; }
    public ICommand CreateProjectCommand { get; }
    public ICommand DeleteProjectCommand { get; }
    public ICommand RenameProjectCommand { get; }

    public string ActiveProjectName => _projectService.ActiveProject?.Name ?? string.Empty;

    public ObservableCollection<ProjectSummary> Projects => _projectService.Projects;

    public async Task DuplicateProjectAsync(string sourceId, string name)
    {
        await _projectService.DuplicateProjectAsync(sourceId, name);
        OnPropertyChanged(nameof(Projects));
    }

    private async Task SwitchProjectAsync(string id)
    {
        ExportOpenedDocumentsToActiveProject();
        await _projectService.SwitchProjectAsync(id);
        ReloadDocuments();
        OnPropertyChanged(nameof(ActiveProjectName));
        OnPropertyChanged(nameof(Projects));
    }

    private void CreateProject(string name)
    {
        _projectService.CreateProject(name);
        OnPropertyChanged(nameof(Projects));
    }

    private void DeleteProject(string id)
    {
        var isActive = _projectService.ActiveProject?.Id == id;
        _projectService.DeleteProject(id);

        if (isActive)
        {
            var defaultProject = _projectService.Projects.First(p => p.IsDefault);
            _ = SwitchProjectAsync(defaultProject.Id);
        }

        OnPropertyChanged(nameof(Projects));
    }

    private void RenameProject(string id, string newName)
    {
        _projectService.RenameProject(id, newName);
        OnPropertyChanged(nameof(ActiveProjectName));
    }

    private void ExportOpenedDocumentsToActiveProject()
    {
        if (_projectService.ActiveProject is null)
            return;

        _projectService.ActiveProject.OpenedDocuments = ExportOpenedDocuments();
        _projectService.ActiveProject.ActiveSidebarPanel = ActiveSidebarPanel;
    }

    private void ReloadDocuments()
    {
        var openedDocs = _projectService.ActiveProject?.OpenedDocuments;
        Documents.Clear();

        if (openedDocs != null && openedDocs.Count > 0)
        {
            ImportOpenedDocuments(openedDocs);
        }
        else
        {
            AddNewDocument();
        }
    }

    public ObservableCollection<RequestDocumentViewModel> Documents { get; }
        = new();

    private RequestDocumentViewModel _activeDocument;

    public RequestDocumentViewModel ActiveDocument
    {
        get => _activeDocument;
        set => SetProperty(ref _activeDocument, value);
    }

    public ICommand NewTabCommand { get; }
    public ICommand CloseDocumentCommand { get; }
    public ICommand DeleteSavedRequestCommand { get; }

    private RequestDocumentViewModel CreateDocument(HttpRequestModel request, HttpResponseResult response = null)
        => new RequestDocumentViewModel(_httpExecutor, _historyService, _savedRequestService, request, response);

    private void AddNewDocument()
    {
        var doc = CreateDocument(new HttpRequestModel());
        Documents.Add(doc);
        ActiveDocument = doc;
    }

    public void OpenHistoryEntry(RequestHistoryEntry entry)
    {
        var vm = CreateDocument(entry.ToHttpRequestModel(), entry.ToHttpResponseModel());
        HistoryMapper.ApplyAuth(entry, vm.Auth);

        Documents.Add(vm);
        ActiveDocument = vm;
    }

    public void OpenSaved(SavedRequestEntry entry)
    {
        var vm = CreateDocument(SavedRequestMapper.ToRequestModel(entry));
        SavedRequestMapper.ApplyAuth(entry, vm.Auth);

        Documents.Add(vm);
        ActiveDocument = vm;
    }

    private void CloseDocument(RequestDocumentViewModel doc)
    {
        if (doc == null) return;

        var index = Documents.IndexOf(doc);
        Documents.Remove(doc);
        doc.CancelRequest();

        if (ActiveDocument == doc)
        {
            if (Documents.Count == 0)
            {
                ActiveDocument = null;
            }
            else
            {
                var newIndex = Math.Max(0, index - 1);
                ActiveDocument = Documents[newIndex];
            }
        }
    }

    private void DeleteSavedRequest(SavedRequestEntry entry)
    {
        if (entry == null) return;
        _savedRequestService.Remove(entry);
    }

    public void OpenDocument(OpenedDocumentEntry entry)
    {
        var vm = CreateDocument(
            OpenedDocumentMapper.ToRequestModel(entry),
            OpenedDocumentMapper.ToResponseModel(entry));

        OpenedDocumentMapper.ApplyAuth(entry, vm.Auth);

        Documents.Add(vm);
    }

    public List<OpenedDocumentEntry> ExportOpenedDocuments()
    {
        return Documents
            .Select(doc => OpenedDocumentMapper.FromViewModel(doc))
            .ToList();
    }

    public void ImportOpenedDocuments(List<OpenedDocumentEntry> entries)
    {
        if (entries == null || entries.Count == 0)
            return;

        Documents.Clear();

        foreach (var entry in entries)
        {
            OpenDocument(entry);
        }

        if (Documents.Count > 0)
        {
            ActiveDocument = Documents[0];
        }
    }

    public void CancelAllRequests()
    {
        foreach (var document in Documents)
        {
            document.CancelRequest();
        }
    }
}