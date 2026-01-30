using System;

namespace Postgirl.Domain.History
{
    public class RequestHistoryEntry
    {
        public string Method { get; set; } = "GET";
        public string Url { get; set; } = string.Empty;

        public int StatusCode { get; set; }
        public int DurationMs { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string? AuthToken { get; set; }
    }
}
