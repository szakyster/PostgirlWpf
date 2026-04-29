using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Postgirl.Domain.Authentication;
using Postgirl.Domain.Http.Body;

namespace Postgirl.Domain.Http;

public class HttpRequestModel
{
    public HttpRequestModel()
    {
    }

    public HttpRequestModel(HttpRequestModel other)
    {
        ArgumentNullException.ThrowIfNull(other);

        Method = other.Method;
        Url = other.Url;
        Headers = other.Headers.Select(h => h.Copy()).ToList();
        Parameters = other.Parameters.Select(p => p.Copy()).ToList();
        Body = CloneBody(other.Body);
        AuthType = other.AuthType;
        BearerToken = other.BearerToken;
        Timeout = other.Timeout;
        FollowRedirects = other.FollowRedirects;
        IgnoreSslErrors = other.IgnoreSslErrors;
    }

    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public string Url { get; set; } = string.Empty;

    public IList<RequestHeader> Headers { get; set; } =[];
    public IList<RequestParameter> Parameters { get; set; } =[];
    public HttpBody Body { get; set; } = new TextBody();

    public AuthType AuthType { get; set; }
    public string BearerToken { get; set; } = string.Empty;

    public TimeSpan? Timeout { get; set; }
    public bool FollowRedirects { get; set; } = true;
    public bool IgnoreSslErrors { get; set; }

    private static HttpBody CloneBody(HttpBody body) => body switch
    {
        TextBody text => new TextBody
        {
            Content = text.Content,
            ContentType = text.ContentType
        },
        JsonBody json => new JsonBody
        {
            Json = json.Json
        },
        FormUrlEncodedBody form => new FormUrlEncodedBody
        {
            Items = form.Items.Select(i => i.Copy()).ToList()
        },
        _ => body ?? new TextBody()
    };
}