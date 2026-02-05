using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
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
    private readonly HttpService _httpService;
    private readonly HistoryService _historyService;
    private readonly HttpRequestModel _request;
    private HttpResponseResult? _response;


    public RequestDocumentViewModel(HttpService httpService, HistoryService historyService, HttpRequestModel request, HttpResponseResult response = null)
    {
        _httpService = httpService;
        _historyService = historyService;
        _request = request;
        _response = response;
        SendCommand = new AsyncRelayCommand(SendAsync);
        AddHeaderCommand = new RelayCommand(() => { AddUserHeader("New-Header", ""); });
        AddFormItemCommand = new RelayCommand(AddFormItem);

        RequestHeaders = new ObservableCollection<RequestHeaderItemViewModel>(
            _request.Headers.Select(h => new RequestHeaderItemViewModel(h, RemoveHeader))
        );

        Auth = new RequestAuthViewModel();
        Auth.PropertyChanged += OnAuthChanged;
    }


    public RequestAuthViewModel Auth { get; }

    public ObservableCollection<string> HttpMethods { get; } =
        ["GET", "POST", "PUT", "DELETE"];

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

/*    public string RequestBody
    {
        get => _request.Body.ToString();
        set
        {
            if ((_request.Body?.ToString() ?? "") == value) return;
            var body = new TextBody
            {
                Content = value
            };
            _request.Body = body;
        }
    }
*/
    private string _textBodyText = "";
    public string TextBodyText
    {
        get => _textBodyText;
        set
        {
            if (SetProperty(ref _textBodyText, value))
                SyncBodyToDomain();
        }
    }

    private string _jsonBodyText = "";
    public string JsonBodyText
    {
        get => _jsonBodyText;
        set
        {
            if (SetProperty(ref _jsonBodyText, value))
                SyncBodyToDomain();
        }
    }

    public ObservableCollection<FormItemViewModel> FormItems { get; }
        = new();

    private void RemoveFormItem(FormItemViewModel item)
    {
        FormItems.Remove(item);
    }
    private void AddFormItem()
    {
        var domain = new FormUrlEncodedItem();
        var vm = new FormItemViewModel(domain, RemoveFormItem);
        FormItems.Add(vm);
    }

    public ObservableCollection<RequestHeaderItemViewModel> RequestHeaders
    {
        get;
    }

    private BodyType _selectedBodyType;
    public BodyType SelectedBodyType
    {
        get => _selectedBodyType;
        set
        {
            if (_selectedBodyType != value)
            {
                _selectedBodyType = value;
                OnPropertyChanged();
                UpdateSystemHeaders();
                SyncBodyToDomain();
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
                    Text = TextBodyText
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
    public ICommand AddFormItemCommand { get; }

    private async Task SendAsync()
    {
        var toRemove = RequestHeaders
            .Where(h => !h.IsSystem && !h.HasValidKey())
            .ToList();

        foreach (var header in toRemove)
        {
            RemoveHeader(header);
        }

        _request.Headers = RequestHeaders.Select(h => h.Domain).ToList();

        _response = await _httpService.SendAsync(_request);
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
            StatusCode = _response.StatusCode,
            DurationMs = _response.ElapsedMilliseconds,
            Headers = _request.Headers.Where(h => !string.IsNullOrWhiteSpace(h.Key))
                .Select(h => h.Copy())
                .ToList(),
            ResponseBody = _response.Body
        };
        _historyService.Add(historyEntry);
    }

    private void UpdateSystemHeaders()
    {
        var contentType = SelectedBodyType switch
        {
            BodyType.Json => "application/json",
            BodyType.Xml => "application/xml",
            BodyType.Text => "text/plain",
            BodyType.FormUrlEncoded => "application/x-www-form-urlencoded",
            _ => null
        };

        AddSystemHeader("Content-Type", contentType);
    }

    private void AddSystemHeader(string key, string value)
    {
        var existing = RequestHeaders
            .FirstOrDefault(h => h.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
            RequestHeaders.Remove(existing);

        var header = new RequestHeader(key, value, isSystem: true);
        RequestHeaders.Add(new RequestHeaderItemViewModel(header, RemoveHeader));
    }

    public void AddUserHeader(string key, string value)
    {
        var header = new RequestHeader(key, value, isSystem: false);
        RequestHeaders.Add(new RequestHeaderItemViewModel(header, RemoveHeader));
    }

    private void RemoveHeader(RequestHeaderItemViewModel headerVm)
    {
        RequestHeaders.Remove(headerVm);
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

    private void OnAuthChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RequestAuthViewModel.AuthType) ||
            e.PropertyName == nameof(RequestAuthViewModel.BearerToken))
        {
            SyncAuthorizationHeader();
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

}