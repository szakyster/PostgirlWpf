using Postgirl.Domain.Http;
using Postgirl.Domain.Http.Body;
using Postgirl.Domain.SavedRequests;
using Postgirl.Presentation.ViewModels;
using Postgirl.Presentation.ViewModels.Authentication;
using System.Linq;

namespace Postgirl.Services;

public static class SavedRequestMapper
{
    // =========================
    // VIEWMODEL -> SAVED ENTRY
    // =========================
    public static SavedRequestEntry FromViewModel(RequestDocumentViewModel vm)
    {
        var request = vm.Domain; 

        var entry = new SavedRequestEntry
        {
            Name = vm.Title,

            Method = request.Method,
            Url = request.Url,

            Headers = request.Headers
                .Select(h => h.Copy())
                .ToList(),

            AuthType = vm.Auth.AuthType,
            BearerToken = vm.Auth.BearerToken
        };

        MapBodyFromRequest(request, entry);

        return entry;
    }

    // =========================
    // SAVED ENTRY -> REQUEST MODEL
    // =========================
    public static HttpRequestModel ToRequestModel(SavedRequestEntry entry)
    {
        var model = new HttpRequestModel
        {
            Method = entry.Method,
            Url = entry.Url,
            Headers = entry.Headers
                .Select(h => h.Copy())
                .ToList()
        };

        MapBodyToRequest(entry, model);

        return model;
    }

    // =========================
    // SAVED ENTRY -> AUTH VM
    // =========================
    public static void ApplyAuth(
        SavedRequestEntry entry,
        RequestAuthViewModel authVm)
    {
        authVm.AuthType = entry.AuthType;
        authVm.BearerToken = entry.BearerToken;
    }

    // =========================
    // BODY: REQUEST -> SAVED
    // =========================
    private static void MapBodyFromRequest(
        HttpRequestModel request,
        SavedRequestEntry entry)
    {
        if (request.Body == null)
            return;

        switch (request.Body)
        {
            case TextBody text:
                entry.BodyType = BodyType.Text;
                entry.BodyText = text.Text ?? "";
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
    // BODY: SAVED -> REQUEST
    // =========================
    private static void MapBodyToRequest(
        SavedRequestEntry entry,
        HttpRequestModel model)
    {
        switch (entry.BodyType)
        {
            case BodyType.Text:
                model.Body = new TextBody
                {
                    Text = entry.BodyText ?? ""
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
                model.Body = new TextBody { Text = "" };
                break;
        }
    }
}
