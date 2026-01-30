using Postgirl.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Postgirl.Domain.Http;

namespace Postgirl.Services;

public class HttpService
{
    private static readonly HttpClient Client = new();

    public async Task<HttpResponseResult> SendAsync(HttpRequestModel model)
    {
        var request = new HttpRequestMessage(model.Method, model.Url);

        foreach (var header in model.Headers.Where(h => !string.IsNullOrWhiteSpace(h.Key) && h.IsEnabled))
        {
            if (request.Headers.TryAddWithoutValidation(header.Key, header.Value)) { continue;}
            request.Content ??= new StringContent("", Encoding.UTF8);
            request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (model.Method != HttpMethod.Get && !string.IsNullOrWhiteSpace( model.Body.ToString() ))
        {
            request.Content = new StringContent(model.Body.ToString(), Encoding.UTF8, "application/json");
        }

        var sw = Stopwatch.StartNew();
        var response = await Client.SendAsync(request);
        sw.Stop();

        var responseText = await response.Content.ReadAsStringAsync();

        // JSON formázás
        if (response.Content.Headers.ContentType?.MediaType?.Contains("json") == true)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseText);
                responseText =
                    JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                // invalid JSON → raw text marad
            }
        }

        var responseHeaders = response.Headers
            .Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")
            .Concat(response.Content.Headers
                .Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))
            .ToList();

        return new HttpResponseResult
        {
            StatusCode = (int)response.StatusCode,
            Headers = responseHeaders,
            Body = responseText,
            ElapsedMilliseconds = sw.ElapsedMilliseconds,
            ResponseSize = Encoding.UTF8.GetByteCount(responseText)
        };
    }

    

}