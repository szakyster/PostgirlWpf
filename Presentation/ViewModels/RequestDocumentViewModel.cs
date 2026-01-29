using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Postgirl.Common;
using Postgirl.Domain.Http;
using Postgirl.Domain.Models;
using Postgirl.Presentation.ViewModels.Authentication;
using Postgirl.Services;

namespace Postgirl.Presentation.ViewModels;

public class RequestDocumentViewModel : BaseViewModel
{
    private readonly HttpService _httpService;
    private readonly HttpRequestModel _request;

    public RequestDocumentViewModel(HttpService httpService, HttpRequestModel request)
    {
        _httpService = httpService;
        _request = request;
        SendCommand = new AsyncRelayCommand(SendAsync);
        AddHeaderCommand = new RelayCommand(() => { AddUserHeader("New-Header", ""); });

        RequestHeaders = new ObservableCollection<RequestHeaderItemViewModel>(
            _request.Headers.Select(h => new RequestHeaderItemViewModel(h, RemoveHeader))
        );

        Auth = new RequestAuthViewModel();
    }

    public RequestAuthViewModel Auth { get; }

    public ObservableCollection<string> HttpMethods { get; } =
        ["GET", "POST", "PUT", "DELETE"];

    public string Url
    {
        get => _request.Url;
        set
        {
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

    public string RequestBody
    {
        get => _request.Body.ToString();
        set
        {
            if (_request.Body.ToString() != value) return;
            var body = new TextBody();
            body.Content = value;
            _request.Body = body;
        }
    }
    

    public ObservableCollection<RequestHeaderItemViewModel> RequestHeaders { get; }

    private BodyType _selectedBodyType;
    public BodyType SelectedBodyType
    {
        get => _selectedBodyType;
        set
        {
            if (_selectedBodyType != value)
            {
                _selectedBodyType = value;
                OnPropertyChanged(nameof(SelectedBodyType));
                UpdateSystemHeaders();
            }
        }
    }


    #region response
    public string ResponseBody
    {
        get => _responseBody;
        set => SetProperty(ref _responseBody, value);
    }
    private string _responseBody;

    public ObservableCollection<string> ResponseHeaders { get; }
        = new();

    private int _statusCode;
    public int StatusCode
    {
        get => _statusCode;
        set => SetProperty(ref _statusCode, value);
    }

    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private long _elapsedMilliseconds;
    public long ElapsedMilliseconds
    {
        get => _elapsedMilliseconds;
        set => SetProperty(ref _elapsedMilliseconds, value);
    }

    private long _responseSize;

    public long ResponseSize
    {
        get => _responseSize;
        set => SetProperty(ref _responseSize, value);
    }


    private Brush _statusColor;
    public Brush StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }
    #endregion

   

    public Brush StatusColor2 =>
        StatusCode switch
        {
            >= 200 and < 300 => Brushes.LightGreen,
            >= 400 and < 500 => Brushes.Orange,
            >= 500 => Brushes.Red,
            _ => Brushes.Gray
        }
    ;

    public ICommand SendCommand { get; }
    public ICommand AddHeaderCommand { get; }

    private async Task SendAsync()
    {
        var toRemove = RequestHeaders
            .Where(h => !h.IsSystem && !h.HasValidKey())
            .ToList();

        foreach (var header in toRemove)
        {
            RemoveHeader(header);
        }

        var result = await _httpService.SendAsync(_request);

        ResponseBody = result.Body;

        ResponseHeaders.Clear();
        foreach (var header in result.Headers)
        {
            ResponseHeaders.Add(header);
        }

        StatusCode = result.StatusCode;
        StatusText = result.StatusCode switch
        {
            >= 200 and < 300 => "OK",
            >= 300 and < 400 => "Redirect",
            >= 400 and < 500 => "Client Error",
            >= 500 => "Server Error",
            _ => "Unknown"
        };
        ElapsedMilliseconds = result.ElapsedMilliseconds;
        ResponseSize = result.ResponseSize;
        StatusColor = StatusCode switch
        {
            >= 200 and < 300 => Brushes.LightGreen,
            >= 400 and < 500 => Brushes.Orange,
            >= 500 => Brushes.Red,
            _ => Brushes.Gray
        };
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
        _request.Headers.Add(header);
        RequestHeaders.Add(new RequestHeaderItemViewModel(header, RemoveHeader));
    }

    public void AddUserHeader(string key, string value)
    {
        var header = new RequestHeader(key, value, isSystem: false);
        _request.Headers.Add(header);
        RequestHeaders.Add(new RequestHeaderItemViewModel(header, RemoveHeader));
    }

    private void RemoveHeader(RequestHeaderItemViewModel headerVm)
    {
        RequestHeaders.Remove(headerVm);
        var header = _request.Headers
            .FirstOrDefault(h => h.Key == headerVm.Key && h.Value == headerVm.Value);
        if (header != null)
        {
            _request.Headers.Remove(header);
        }
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


}