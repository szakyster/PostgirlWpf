using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Postgirl.Common;
using Postgirl.Domain.History;
using Postgirl.Domain.Http;
using Postgirl.Domain.SavedRequests;
using Postgirl.Services;

namespace Postgirl.Presentation.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly HttpService _httpService;
    private readonly HistoryService _historyService;
    private readonly StorageService _storageService;
    private readonly SavedRequestService _savedRequestService;
    public HistoryViewModel HistoryViewModel { get; }
    public ObservableCollection<SavedRequestEntry> SavedRequests
        => _savedRequestService.Items;

    public MainViewModel(HttpService httpService, HistoryService historyService, StorageService storageService, SavedRequestService savedRequestService)
    {
        _storageService = storageService;
        _savedRequestService = savedRequestService;
        _httpService = httpService;
        _historyService = historyService;

        HistoryViewModel = new HistoryViewModel(historyService, this);

        NewTabCommand = new RelayCommand(AddNewDocument);
        CloseDocumentCommand = new RelayCommand<RequestDocumentViewModel>(CloseDocument);

        LoadState();

        AddNewDocument(); // első fül
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

    private void AddNewDocument()
    {
        var domainModel = new HttpRequestModel();
        var doc = new RequestDocumentViewModel(_httpService, _historyService, _savedRequestService, domainModel);
        Documents.Add(doc);
        ActiveDocument = doc;
    }

    public void OpenHistoryEntry(RequestHistoryEntry entry)
    {

        var request = entry.ToHttpRequestModel();
        var vm = new RequestDocumentViewModel(
            _httpService,
            _historyService, _savedRequestService, request, entry.ToHttpResponseModel());
        HistoryMapper.ApplyAuth(entry, vm.Auth);

        Documents.Add(vm);
        ActiveDocument = vm;
    }

    public void OpenSaved(SavedRequestEntry entry)
    {
        var request = SavedRequestMapper.ToRequestModel(entry);

        var vm = new RequestDocumentViewModel(
            _httpService, _historyService, _savedRequestService,
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
}