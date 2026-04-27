using Postgirl.Domain.Authentication;
using Postgirl.Domain.Http;
using Postgirl.Domain.Http.Body;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Postgirl.Domain.History
{
    public class RequestHistoryEntry
    {
        private const string TodayGroup = "Today";
        private const string ThisWeekGroup = "This Week";
        private const string ThisMonthGroup = "This Month";
        private const string OlderGroup = "Older";

        //request
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public string Url { get; set; } = string.Empty;

        public List<RequestHeader> Headers { get; set; } = [];
        public List<RequestParameter> Parameters { get; set; } = [];

        public BodyType BodyType { get; set; }
        public string BodyText { get; set; } = string.Empty;
        public string BodyJson { get; set; } = string.Empty;

        public List<FormUrlEncodedItem> FormItems { get; set; } = new();

        public AuthType AuthType { get; set; }
        public string BearerToken { get; set; } = string.Empty;
        public DateTimeOffset? ExecutedAt { get; set; }
        public string ExecutedAtGroup
        {
            get
            {
                if (!ExecutedAt.HasValue)
                    return OlderGroup;

                var executedDate = ExecutedAt.Value.LocalDateTime.Date;
                var today = DateTime.Today;

                if (executedDate == today)
                    return TodayGroup;

                var startOfWeek = today.AddDays(-((7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7));
                if (executedDate >= startOfWeek)
                    return ThisWeekGroup;

                if (executedDate.Year == today.Year && executedDate.Month == today.Month)
                    return ThisMonthGroup;

                return OlderGroup;
            }
        }

        //response
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public string ResponseBody { get; set; }
        public IReadOnlyList<string> ResponseHeaders { get; set; }
    }
}
