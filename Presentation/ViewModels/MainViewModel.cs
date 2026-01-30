using System.Collections.ObjectModel;
using System.Windows.Input;
using Postgirl.Common;
using Postgirl.Domain.Http;
using Postgirl.Services;

namespace Postgirl.Presentation.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly HttpService _httpService;


    public MainViewModel(HttpService httpService)
    {
        _httpService = httpService;

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
        var doc = new RequestDocumentViewModel(_httpService, domainModel);
        Documents.Add(doc);
        ActiveDocument = doc;
    }
}