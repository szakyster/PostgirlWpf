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

        //request
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public string Url { get; set; } = string.Empty;


        public DateTime Timestamp { get; set; } = DateTime.Now;

        public BodyType BodyType { get; set; }
        public string BodyText { get; set; }
        public string BodyJson { get; set; }
        
        public AuthType AuthType { get; set; }

        public List<RequestHeader> Headers { get; set; } = [];

        //response
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public string ResponseBody { get; init; }

    }
}
