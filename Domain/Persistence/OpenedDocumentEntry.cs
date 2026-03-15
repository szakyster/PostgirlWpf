using System.Collections.Generic;
using System.Net.Http;
using Postgirl.Domain.Authentication;
using Postgirl.Domain.Http;
using Postgirl.Domain.Http.Body;

namespace Postgirl.Domain.Persistence;

public class OpenedDocumentEntry
{
    // Request
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public string Url { get; set; } = string.Empty;

    public List<RequestHeader> Headers { get; set; } = [];
    public List<RequestParameter> Parameters { get; set; } = [];

    public BodyType BodyType { get; set; }
    public string BodyText { get; set; } = string.Empty;
    public string BodyJson { get; set; } = string.Empty;
    public List<FormUrlEncodedItem> FormItems { get; set; } = [];

    public AuthType AuthType { get; set; }
    public string BearerToken { get; set; } = string.Empty;

    // Response (optional, if exists)
    public bool HasResponse { get; set; }
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public string ResponseBody { get; set; } = string.Empty;
    public List<string> ResponseHeaders { get; set; } = [];
}
