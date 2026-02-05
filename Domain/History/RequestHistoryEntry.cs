using Postgirl.Domain.Http;
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

        //response
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public string ResponseBody { get; init; }
    }
}
