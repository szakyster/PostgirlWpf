using System;
using System.Net.Http;

namespace Postgirl.Domain.History
{
    public class RequestHistoryEntry
    {
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public string Url { get; set; } = string.Empty;

        public int StatusCode { get; set; }
        public long DurationMs { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string? AuthToken { get; set; }
    }
}
