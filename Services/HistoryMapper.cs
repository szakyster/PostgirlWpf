using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Postgirl.Domain.Http;
using Postgirl.Domain.Authentication;
using Postgirl.Domain.History;
using Postgirl.Domain.Http.Body;

namespace Postgirl.Services;

public static class HistoryMapper
{
    public static HttpRequestModel ToHttpRequestModel(this RequestHistoryEntry entry)
    {
        var requestModel = new HttpRequestModel
        {
            Method = entry.Method,
            Url = entry.Url,
            Headers = MapHeaders(entry)
        };
        return requestModel;
    }

    private static IList<RequestHeader> MapHeaders(RequestHistoryEntry entry)
    {
        return entry.Headers == null ? [] : entry.Headers.Select(h => h.Copy()).ToList();
    }

    public static HttpResponseResult ToHttpResponseModel(this RequestHistoryEntry entry)
    {
        var responseModel = new HttpResponseResult
        {
            StatusCode = entry.StatusCode,
            Body = entry.ResponseBody,
            ElapsedMilliseconds = entry.DurationMs,
            Headers = new List<string>() // Placeholder, as headers are not stored in history
            
        };
        return responseModel;
    }
}