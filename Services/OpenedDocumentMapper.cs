using Postgirl.Domain.Http;
using Postgirl.Domain.Http.Body;
using Postgirl.Domain.Persistence;
using Postgirl.Presentation.ViewModels;
using System.Linq;

namespace Postgirl.Services;

public static class OpenedDocumentMapper
{
    // =========================
    // VIEWMODEL -> OPENED DOCUMENT ENTRY
    // =========================
    public static OpenedDocumentEntry FromViewModel(RequestDocumentViewModel vm)
    {
        vm.SyncToDomain();
        var request = vm.Domain;

        var entry = new OpenedDocumentEntry
        {
            Method = request.Method,
            Url = request.Url,

            Headers = request.Headers
                .Select(h => h.Copy())
                .ToList(),

            Parameters = request.Parameters
                .Select(p => p.Copy())
                .ToList(),

            AuthType = vm.Auth.AuthType,
            BearerToken = vm.Auth.BearerToken
        };

        MapBodyFromRequest(request, entry);

        // Response data if available
        if (vm.StatusCode > 0)
        {
            entry.HasResponse = true;
            entry.StatusCode = vm.StatusCode;
            entry.DurationMs = vm.ElapsedMilliseconds;
            entry.ResponseBody = vm.ResponseBody ?? string.Empty;
            entry.ResponseHeaders = vm.ResponseHeaders?.ToList() ?? [];
        }

        return entry;
    }

    // =========================
    // OPENED DOCUMENT ENTRY -> REQUEST MODEL
    // =========================
    public static HttpRequestModel ToRequestModel(OpenedDocumentEntry entry)
    {
        var model = new HttpRequestModel
        {
            Method = entry.Method,
            Url = entry.Url,
            Headers = entry.Headers
                .Select(h => h.Copy())
                .ToList(),
            Parameters = entry.Parameters
                .Select(p => p.Copy())
                .ToList(),
            AuthType = entry.AuthType,
            BearerToken = entry.BearerToken
        };

        MapBodyToRequest(entry, model);

        return model;
    }

    // =========================
    // OPENED DOCUMENT ENTRY -> RESPONSE MODEL
    // =========================
    public static HttpResponseResult ToResponseModel(OpenedDocumentEntry entry)
    {
        if (!entry.HasResponse)
            return null;

        return new HttpResponseResult
        {
            StatusCode = entry.StatusCode,
            Body = entry.ResponseBody,
            ElapsedMilliseconds = entry.DurationMs,
            Headers = entry.ResponseHeaders ?? []
        };
    }

    // =========================
    // OPENED DOCUMENT ENTRY -> AUTH VM
    // =========================
    public static void ApplyAuth(OpenedDocumentEntry entry, Presentation.ViewModels.Authentication.RequestAuthViewModel authVm)
    {
        authVm.AuthType = entry.AuthType;
        authVm.BearerToken = entry.BearerToken;
    }

    // =========================
    // BODY: REQUEST -> OPENED DOCUMENT
    // =========================
    private static void MapBodyFromRequest(HttpRequestModel request, OpenedDocumentEntry entry)
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
    // BODY: OPENED DOCUMENT -> REQUEST
    // =========================
    private static void MapBodyToRequest(OpenedDocumentEntry entry, HttpRequestModel model)
    {
        switch (entry.BodyType)
        {
            case BodyType.Text:
                model.Body = new TextBody
                {
                    Content = entry.BodyText ?? ""
                };
                break;

            case BodyType.Json:
                model.Body = new JsonBody
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

                model.Body = form;
                break;

            default:
                model.Body = new TextBody { Content = "" };
                break;
        }
    }
}
