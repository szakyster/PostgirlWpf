using System.Collections.Generic;
using System.Linq;
using Postgirl.Domain.Http;
using Postgirl.Domain.History;
using Postgirl.Domain.Http.Body;
using Postgirl.Domain.SavedRequests;
using Postgirl.Presentation.ViewModels.Authentication;

namespace Postgirl.Services;

public static class HistoryMapper
{
    public static HttpRequestModel ToHttpRequestModel(this RequestHistoryEntry entry)
    {
        var requestModel = new HttpRequestModel
        {
            Method = entry.Method,
            Url = entry.Url,
            Headers = MapHeaders(entry),
            Parameters = entry.Parameters == null ? [] : entry.Parameters.Select(p => p.Copy()).ToList(),
        };

        switch (entry.BodyType)
        {
            case BodyType.Text:
                requestModel.Body = new TextBody
                {
                    Content = entry.BodyText ?? ""
                };
                break;

            case BodyType.Json:
                requestModel.Body = new JsonBody
                {
                    Json = entry.BodyJson ?? ""
                };
                break;

            case BodyType.FormUrlEncoded:
                var form = new FormUrlEncodedBody();

                if (entry.FormItems != null)
                {
                    foreach (var item in entry.FormItems)
                        form.Items.Add(item.Copy());
                }

                requestModel.Body = form;
                break;

            default:
                requestModel.Body = new TextBody { Content = "" };
                break;
        }

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
            Headers = entry.ResponseHeaders ?? new List<string>()

        };
        return responseModel;
    }

    // =========================
    // BODY: REQUEST -> SAVED
    // =========================
    public static void AddMapBodyFromRequest(
        this RequestHistoryEntry entry, HttpRequestModel request)
    {
        if (request.Body == null)
            return;

        switch (request.Body)
        {
            case TextBody text:
                entry.BodyType = BodyType.Text;
                entry.BodyText = text.Content ?? "";
                break;

            case JsonBody json:
                entry.BodyType = BodyType.Json;
                entry.BodyJson = json.Json ?? "";
                break;

            case FormUrlEncodedBody form:
                entry.BodyType = BodyType.FormUrlEncoded;
                entry.FormItems = form.Items
                    .Select(i => i.Copy())
                    .ToList();
                break;

            default:
                entry.BodyType = BodyType.None;
                break;
        }

    }
 
    // =========================
    // SAVED ENTRY -> AUTH VM
    // =========================
    public static void ApplyAuth(
        RequestHistoryEntry entry,
        RequestAuthViewModel authVm)
    {
        authVm.AuthType = entry.AuthType;
        authVm.BearerToken = entry.BearerToken;
    }
}