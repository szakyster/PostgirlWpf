using System.Collections.Generic;
using System.Net.Http;
using Postgirl.Domain.Http.Body;

namespace Postgirl.Domain.Http;

public class HttpRequestModel
{
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public string Url { get; set; } = string.Empty;

    public IList<RequestHeader> Headers { get; set; } =[];
    public HttpBody Body { get; set; } = new TextBody();
}