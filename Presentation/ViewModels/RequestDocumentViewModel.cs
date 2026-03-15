using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Postgirl.Common;
using Postgirl.Domain.Authentication;
using Postgirl.Domain.History;
using Postgirl.Domain.Http;
using Postgirl.Domain.Http.Body;
using Postgirl.Presentation.ViewModels.Authentication;
using Postgirl.Services;

namespace Postgirl.Presentation.ViewModels;

public class RequestDocumentViewModel : BaseViewModel
{
    private readonly HistoryService _historyService;
    private readonly HttpRequestModel _request;
    private readonly IHttpExecutor _httpExecutor;
    private HttpResponseResult _response;
    private CancellationTokenSource _cancellationTokenSource;


    public RequestDocumentViewModel(IHttpExecutor httpExecutor, HistoryService historyService, SavedRequestService savedRequestService, HttpRequestModel request, HttpResponseResult response = null)
    {
        _historyService = historyService;
        _request = request;
        _httpExecutor = httpExecutor;
        _savedRequestService = savedRequestService;
        _response = response;
        SendCommand = new AsyncRelayCommand(SendAsync);
        AddHeaderCommand = new RelayCommand(() => { AddUserHeader("New-Header", ""); });
        AddParameterCommand = new RelayCommand(() => { AddParameter("param", ""); });
        AddFormItemCommand = new RelayCommand(AddFormItem);
        SaveRequestCommand = new RelayCommand(SaveRequest);
        CancelCommand = new RelayCommand(CancelRequest);

        RequestHeaders = new ObservableCollection<RequestHeaderItemViewModel>(
            _request.Headers.Select(h => new RequestHeaderItemViewModel(h, RemoveHeader))
        );
        RequestParameters = new ObservableCollection<RequestParameterItemViewModel>(
            _request.Parameters.Select(p => new RequestParameterItemViewModel(p, RemoveParameter))
        );
        SyncDomainToBody();
        FormItems.CollectionChanged += (_, _) => SyncBodyToDomain();
        Auth = new RequestAuthViewModel();
        Auth.PropertyChanged += OnAuthChanged;

        SendButtonText = "Send";
    }

    public HttpRequestModel Domain => _request;

    public RequestAuthViewModel Auth { get; }

    public ObservableCollection<string> HttpMethods { get; } =
        ["GET", "POST", "PUT", "DELETE"];

    private string _sendButtonText;
    public string SendButtonText
    {
        get => _sendButtonText;
        set => SetProperty(ref _sendButtonText, value);
    }

    public string Url
    {
        get => _request.Url;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            // Ha nem http:// vagy https://, egészítsd ki
            if (!value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                value = "http://" + value;
            }

            if (_request.Url == value) return;
            _request.Url = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Title));
        }
    }

    public string Title => string.IsNullOrWhiteSpace(Url) ? "Untitled request" : Url;

    public string Method
    {
        get => _request.Method.ToString();
        set
        {
            var httpMethod = GetHttpMethod(value);
            if (_request.Method != httpMethod)
            {
                _request.Method = httpMethod;
                OnPropertyChanged();
            }
        }
    }

    private string _textBodyText = "";
    public string TextBodyText
    {
        get => _textBodyText;
        set
        {
            if (SetProperty(ref _textBodyText, value))
            {
                SyncBodyToDomain();
                UpdateSystemHeaders();
            }
                
        }
    }

    private string _jsonBodyText = "";
    public string JsonBodyText
    {
        get => _jsonBodyText;
        set
        {
            if (SetProperty(ref _jsonBodyText, value))
            {
                SyncBodyToDomain();
                UpdateSystemHeaders();
            }
                
        }
    }

    public ObservableCollection<FormItemViewModel> FormItems { get; }
        = new();

    private void RemoveFormItem(FormItemViewModel item)
    {
        FormItems.Remove(item);
        UpdateSystemHeaders();
    }
    private void AddFormItem()
    {
        var domain = new FormUrlEncodedItem();
        var vm = new FormItemViewModel(domain, RemoveFormItem);
        FormItems.Add(vm);
        UpdateSystemHeaders();
    }

    public ObservableCollection<RequestHeaderItemViewModel> RequestHeaders
    {
        get;
    }

    public ObservableCollection<RequestParameterItemViewModel> RequestParameters
    {
        get;
    }

    private BodyType _selectedBodyType;
    private readonly SavedRequestService _savedRequestService;

    public BodyType SelectedBodyType
    {
        get => _selectedBodyType;
        set
        {
            if (_selectedBodyType != value)
            {
                _selectedBodyType = value;
                OnPropertyChanged();
                SyncBodyToDomain();
                UpdateSystemHeaders();
            }
        }
    }

    private void SyncBodyToDomain()
    {
        switch (SelectedBodyType)
        {
            case BodyType.Text:
                _request.Body = new TextBody
                {
                    Content = TextBodyText
                };
                break;

            case BodyType.Json:
                _request.Body = new JsonBody
                {
                    Json = JsonBodyText
                };
                break;

            case BodyType.FormUrlEncoded:
                var form = new FormUrlEncodedBody();

                foreach (var item in FormItems)
                {
                    if (!string.IsNullOrWhiteSpace(item.Key))
                        form.Items.Add(item.ToDomain());
                }

                _request.Body = form;
                break;

            case BodyType.None:
            case BodyType.Xml:
            default:
                _request.Body = null;
                break;
        }
    }

    private void SyncDomainToBody()
    {
        var requestBody = _request.Body;
        SelectedBodyType = requestBody.Type;
        switch (requestBody.Type)
        {
            case BodyType.Text:
                TextBodyText = requestBody.ToString();
                break;

            case BodyType.Json:
                JsonBodyText = requestBody.ToString();

                break;

            case BodyType.FormUrlEncoded:
                var formUrlEncodedBody = (FormUrlEncodedBody)requestBody;
                FormItems.Clear();
                foreach (var item in formUrlEncodedBody.Items)
                {
                    var wm = new FormItemViewModel(item, RemoveFormItem);
                    FormItems.Add(wm);
                }

                break;

            case BodyType.None:
            case BodyType.Xml:
            default:
                _request.Body = null;
                break;
        }
    }

    #region response
    public string ResponseBody
    {
        get => _response?.Body;
        set
        {
            if (_response != null && _response.Body != value)
            {
                _response.Body = value;
                OnPropertyChanged();
            }
        }
    }

    public IReadOnlyList<string> ResponseHeaders => _response?.Headers ?? new List<string>();

    public int StatusCode => _response?.StatusCode ?? 0;

    public string StatusText => SelectStatusName(_response?.StatusCode);

    public long ElapsedMilliseconds
    {
        get => _response?.ElapsedMilliseconds ?? 0;
        set {
            if (_response != null && _response.ElapsedMilliseconds != value)
            {
                _response.ElapsedMilliseconds = value;
                OnPropertyChanged();
            }
        }
    }

    public long ResponseSize => _response?.ResponseSize ?? 0;
    
    public Brush StatusColor => SelectStatusColor(_response?.StatusCode);

    #endregion

    private Brush SelectStatusColor(int? statusCode) =>
        statusCode switch
        {
            >= 200 and < 300 => Brushes.LightGreen,
            >= 400 and < 500 => Brushes.Orange,
            >= 500 => Brushes.Red,
            _ => Brushes.Gray
        };

    private string SelectStatusName(int? statusCode) =>
        statusCode switch
        {
            >= 200 and < 300 => "OK",
            >= 300 and < 400 => "Redirect",
            >= 400 and < 500 => "Client Error",
            >= 500 => "Server Error",
            _ => "Unknown"
        };


    public ICommand SendCommand { get; }
    public ICommand AddHeaderCommand { get; }
    public ICommand AddParameterCommand { get; }
    public ICommand AddFormItemCommand { get; }
    public ICommand SaveRequestCommand { get; }
    public ICommand CancelCommand { get; }

    public void CancelRequest()
    {
        _cancellationTokenSource?.Cancel();
    }

    private async Task SendAsync()
    {
        try
        {
            SendButtonText = "Waiting...";

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            SyncBodyToDomain();
            var toRemove = RequestHeaders
                .Where(h => !h.IsSystem && !h.HasValidKey())
                .ToList();

            foreach (var header in toRemove)
            {
                RemoveHeader(header);
            }

            var parametersToRemove = RequestParameters
                .Where(p => !p.HasValidKey())
                .ToList();

            foreach (var parameter in parametersToRemove)
            {
                RemoveParameter(parameter);
            }

            SyncToDomain();
            var executionResult = await _httpExecutor.ExecuteAsync(_request, _cancellationTokenSource.Token);

            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            if (executionResult.IsSuccess)
            {
                _response = executionResult.Response ?? throw new NullReferenceException("Execution succeeded but response is null");
            }
            else
            {
                _response = new HttpResponseResult
                {
                    StatusCode = 0,
                    Body = executionResult.Error?.Message ?? "Unknown error",
                    Headers = new List<string>(),
                    ElapsedMilliseconds = executionResult.ElapsedMilliseconds,
                    ResponseSize = 0
                };
            }

            OnPropertyChanged(nameof(StatusCode));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(ResponseBody));
            OnPropertyChanged(nameof(ElapsedMilliseconds));
            OnPropertyChanged(nameof(ResponseHeaders));

            var historyEntry = new RequestHistoryEntry
            {
                Method = _request.Method,
                Url = _request.Url,

                Headers = _request.Headers.Where(h => !string.IsNullOrWhiteSpace(h.Key))
                    .Select(h => h.Copy())
                    .ToList(),

                Parameters = _request.Parameters.Where(p => !string.IsNullOrWhiteSpace(p.Key))
                    .Select(p => p.Copy())
                    .ToList(),

                AuthType = Auth.AuthType,
                BearerToken = Auth.BearerToken,

                ResponseBody = _response.Body ?? string.Empty,
                ResponseHeaders = ResponseHeaders.ToList(),
                StatusCode = _response.StatusCode,
                DurationMs = _response.ElapsedMilliseconds,
            };
            historyEntry.AddMapBodyFromRequest(_request);

            _historyService.Add(historyEntry);
        }
        catch (OperationCanceledException)
        {
            // Request was cancelled
            _response = new HttpResponseResult
            {
                StatusCode = 0,
                Body = "Request cancelled",
                Headers = new List<string>(),
                ElapsedMilliseconds = 0,
                ResponseSize = 0
            };

            OnPropertyChanged(nameof(StatusCode));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(ResponseBody));
            OnPropertyChanged(nameof(ElapsedMilliseconds));
            OnPropertyChanged(nameof(ResponseHeaders));
        }
        finally
        {
            SendButtonText = "Send";
        }
    }

    private void UpdateSystemHeaders()
    {
        var headers = _request.Body?.ToHttpContent()?.Headers;
        if (headers != null)
        {
            foreach (var header in headers)
            {
                AddSystemHeader(header.Key, string.Join(", ", header.Value));
            }
        }
        else
        {
            this.RemoveSystemHeader("Content-Type");
        }
    }

    private void AddSystemHeader(string key, string value)
    {
        RemoveSystemHeader(key);
        var header = new RequestHeader(key, value, isSystem: true);
        RequestHeaders.Add(new RequestHeaderItemViewModel(header, RemoveHeader));
        SortHeaders();
    }

    private void RemoveSystemHeader(string key)
    {
        var existing = RequestHeaders
            .FirstOrDefault(h => h.IsSystem && h.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
            RequestHeaders.Remove(existing);
    }

    public void AddUserHeader(string key, string value)
    {
        var header = new RequestHeader(key, value, isSystem: false);
        RequestHeaders.Add(new RequestHeaderItemViewModel(header, RemoveHeader));
        SortHeaders();
    }

    private void RemoveHeader(RequestHeaderItemViewModel headerVm)
    {
        RequestHeaders.Remove(headerVm);
    }

    public void AddParameter(string key, string value)
    {
        var parameter = new RequestParameter(key, value);
        RequestParameters.Add(new RequestParameterItemViewModel(parameter, RemoveParameter));
    }

    private void RemoveParameter(RequestParameterItemViewModel parameterVm)
    {
        RequestParameters.Remove(parameterVm);
    }

    private static HttpMethod GetHttpMethod(string method)
    {
        var httpMethod = method.ToUpper() switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            _ => throw new NotSupportedException($"HTTP method not supported: {method}")
        };

        return httpMethod;
    }

    private void OnAuthChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RequestAuthViewModel.AuthType) ||
            e.PropertyName == nameof(RequestAuthViewModel.BearerToken))
        {
            SyncAuthorizationHeader();
            _request.BearerToken = Auth.AuthType == AuthType.BearerToken ? Auth.BearerToken : null;
        }
    }

    private void SyncAuthorizationHeader()
    {
        var existing = RequestHeaders
            .FirstOrDefault(h =>
                h.IsSystem &&
                string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase));

        // --- NO AUTH ---
        if (Auth.AuthType != AuthType.BearerToken || string.IsNullOrWhiteSpace(Auth.BearerToken))
        {
                if (existing != null)
                    RemoveHeader(existing);
                return;
        }

        // --- BEARER TOKEN ---
        var value = $"Bearer {Auth.BearerToken}";
        AddSystemHeader("Authorization", value);
    }

    private void SortHeaders()
    {
        var ordered = RequestHeaders
            .OrderByDescending(h => h.IsSystem) // system előre
            .ThenBy(h => h.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        RequestHeaders.Clear();

        foreach (var h in ordered)
            RequestHeaders.Add(h);
    }

    private void SaveRequest()
    {
        SyncBodyToDomain();
        _request.Headers = RequestHeaders.Select(h => h.Domain).ToList();
        _request.Parameters = RequestParameters.Select(p => p.Domain).ToList();
        var entry = SavedRequestMapper.FromViewModel(this);
        _savedRequestService.Add(entry);
    }

    public void SyncToDomain()
    {
        _request.Headers = RequestHeaders.Select(h => h.Domain).ToList();
        _request.Parameters = RequestParameters.Select(p => p.Domain).ToList();
    }
}