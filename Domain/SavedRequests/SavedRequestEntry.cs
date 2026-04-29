using Postgirl.Domain.Authentication;
using Postgirl.Domain.Http;
using Postgirl.Domain.Http.Body;
using System.Collections.Generic;
using System.Net.Http;

namespace Postgirl.Domain.SavedRequests;

public class SavedRequestEntry
{
    public string Name { get; set; } = "Noname request";

    public HttpMethod Method { get; set; }
    public string Url { get; set; } = string.Empty;

    public List<RequestHeader> Headers { get; set; } = new();
    public List<RequestParameter> Parameters { get; set; } = new();

    public BodyType BodyType { get; set; }
    public string BodyText { get; set; } = string.Empty;
    public string BodyJson { get; set; } = string.Empty;
    public List<FormUrlEncodedItem> FormItems { get; set; } = new();

    public AuthType AuthType { get; set; }
    public string BearerToken { get; set; } = string.Empty;
}