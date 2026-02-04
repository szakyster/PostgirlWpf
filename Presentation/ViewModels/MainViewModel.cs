using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Postgirl.Common;
using Postgirl.Domain.History;
using Postgirl.Domain.Http;
using Postgirl.Services;

namespace Postgirl.Presentation.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly HttpService _httpService;
    private readonly HistoryService _historyService;
    public HistoryViewModel HistoryViewModel { get; }

    public MainViewModel(HttpService httpService, HistoryService historyService)
    {
        _httpService = httpService;
        _historyService = historyService;
        HistoryViewModel = new HistoryViewModel(historyService, this);

        NewTabCommand = new RelayCommand(AddNewDocument);

        AddNewDocument(); // első fül
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

    private void AddNewDocument()
    {
        var domainModel = new HttpRequestModel();
        var doc = new RequestDocumentViewModel(_httpService, _historyService, domainModel);
        Documents.Add(doc);
        ActiveDocument = doc;
    }

    public void OpenHistoryEntry(RequestHistoryEntry entry)
    {
        

        var vm = new RequestDocumentViewModel(
            _httpService,
            _historyService,
            entry.ToHttpRequestModel(), entry.ToHttpResponseModel());
        
        Documents.Add(vm);
        ActiveDocument = vm;
        
    }
}