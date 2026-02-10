using Postgirl.Domain.Authentication;
using Postgirl.Domain.Http;
using Postgirl.Domain.Http.Body;
using System.Collections.Generic;
using System.Net.Http;

namespace Postgirl.Domain.History
{
    public class RequestHistoryEntry
    {
        //request
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public string Url { get; set; } = string.Empty;

        public List<RequestHeader> Headers { get; set; } = [];

        public BodyType BodyType { get; set; }
        public string BodyText { get; set; } = string.Empty;
        public string BodyJson { get; set; } = string.Empty;

        public List<FormUrlEncodedItem> FormItems { get; set; } = new();

        public AuthType AuthType { get; set; }
        public string BearerToken { get; set; } = string.Empty;

        //response
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public string ResponseBody { get; set; }
        public IReadOnlyList<string> ResponseHeaders { get; set; }
    }
}
