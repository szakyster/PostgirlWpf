using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Postgirl.Common;
using Postgirl.Domain.History;
using Postgirl.Domain.Http;
using Postgirl.Domain.Persistence;
using Postgirl.Domain.SavedRequests;
using Postgirl.Services;

namespace Postgirl.Presentation.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IHttpExecutor _httpExecutor;
    private readonly HistoryService _historyService;
    private readonly StorageService _storageService;
    private readonly SavedRequestService _savedRequestService;

    public HistoryViewModel HistoryViewModel { get; }
    public VariablesViewModel VariablesViewModel { get; }
    public ObservableCollection<SavedRequestEntry> SavedRequests
        => _savedRequestService.Items;

    public string ActiveSidebarPanel { get; set; } = "SavedExpander";

    public string AppVersion
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version == null ? "v?.?.?-alpha" : $"v{version.Major}.{version.Minor}.{version.Build}-alpha";
        }
    }

    public MainViewModel(IHttpExecutor httpExecutor, HistoryService historyService, StorageService storageService, SavedRequestService savedRequestService, VariablesService variablesService)
    {
        _storageService = storageService;
        _savedRequestService = savedRequestService;
        _httpExecutor = httpExecutor;
        _historyService = historyService;

        HistoryViewModel = new HistoryViewModel(historyService, this);
        VariablesViewModel = new VariablesViewModel(variablesService);

        NewTabCommand = new RelayCommand(AddNewDocument);
        CloseDocumentCommand = new RelayCommand<RequestDocumentViewModel>(CloseDocument);
        DeleteSavedRequestCommand = new RelayCommand<SavedRequestEntry>(DeleteSavedRequest);

        LoadState();
    }

    private async void LoadState()
    {
        var state = await _storageService.LoadAsync();
        _historyService.Import(state.History);
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

    private void AddNewDocument()
    {
        var domainModel = new HttpRequestModel();
        var doc = new RequestDocumentViewModel(_httpExecutor, _historyService, _savedRequestService, domainModel);
        Documents.Add(doc);
        ActiveDocument = doc;
    }

    public void OpenHistoryEntry(RequestHistoryEntry entry)
    {
        var request = entry.ToHttpRequestModel();
        var vm = new RequestDocumentViewModel(
            _httpExecutor,
            _historyService, _savedRequestService, request, entry.ToHttpResponseModel());
        HistoryMapper.ApplyAuth(entry, vm.Auth);

        Documents.Add(vm);
        ActiveDocument = vm;
    }

    public void OpenSaved(SavedRequestEntry entry)
    {
        var request = SavedRequestMapper.ToRequestModel(entry);

        var vm = new RequestDocumentViewModel(
            _httpExecutor, _historyService, _savedRequestService,
            request);
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
        var request = OpenedDocumentMapper.ToRequestModel(entry);
        var response = OpenedDocumentMapper.ToResponseModel(entry);

        var vm = new RequestDocumentViewModel(
            _httpExecutor,
            _historyService,
            _savedRequestService,
            request,
            response);

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