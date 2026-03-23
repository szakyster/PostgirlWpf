using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Postgirl.Domain.Execution;
using Postgirl.Domain.Http;

namespace Postgirl.Services
{
    public class HttpExecutor : IHttpExecutor
    {
        private static readonly HttpClient Client = new();

        public async Task<HttpExecutionResult> ExecuteAsync(
            HttpRequestModel model,
            CancellationToken cancellationToken = default)
        {
                var stopwatch = Stopwatch.StartNew();

            try
            {
                using var request = BuildRequestMessage(model);

                using var response = await Client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                stopwatch.Stop();

                if (IsFileResponse(response))
                {
                    var fileBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                    var fileName = ExtractFileName(response);

                    return new HttpExecutionResult
                    {
                        IsSuccess = true,
                        ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                        Response = new HttpResponseResult
                        {
                            StatusCode = (int)response.StatusCode,
                            Headers = ExtractHeaders(response),
                            Body = $"[File: {fileName} — {fileBytes.Length:N0} bytes]",
                            File = new ResponseFile { FileName = fileName, Bytes = fileBytes },
                            ResponseSize = fileBytes.Length,
                            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
                        }
                    };
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                // Format JSON if content type is application/json
                if (IsJsonContentType(response.Content.Headers.ContentType?.MediaType))
                {
                    responseBody = FormatJson(responseBody);
                }

                return new HttpExecutionResult
                {
                    IsSuccess = true,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Response = new HttpResponseResult
                    {
                        StatusCode = (int)response.StatusCode,
                        Headers = ExtractHeaders(response),
                        Body = responseBody,
                        ResponseSize = Encoding.UTF8.GetByteCount(responseBody),
                        ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                return new HttpExecutionResult
                {
                    IsSuccess = false,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Error = HttpError.FromException(ex)
                };
            }
        }

        private HttpRequestMessage BuildRequestMessage(HttpRequestModel model)
        {
            var request = new HttpRequestMessage(model.Method, BuildUrlWithParameters(model.Url, model.Parameters));

            foreach (var header in model.Headers.Where(h => h.IsEnabled))
            {
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    request.Content ??= new StringContent("");
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (model.Method == HttpMethod.Get || model.Method == HttpMethod.Head)
            {
                return request; // ignore body
            }

            var content = model.Body?.ToHttpContent();
            if (content != null)
            {
                request.Content = content;
            }

            return request;
        }

        private string BuildUrlWithParameters(string baseUrl, IList<RequestParameter> parameters)
        {
            var enabledParams = parameters?.Where(p => p.IsEnabled && !string.IsNullOrWhiteSpace(p.Key)).ToList();

            if (enabledParams == null || enabledParams.Count == 0)
            {
                return baseUrl;
            }

            var uriBuilder = new UriBuilder(baseUrl);
            var queryParams = new List<string>();

            // Meglévő query paramétereket megtartjuk
            if (!string.IsNullOrEmpty(uriBuilder.Query))
            {
                queryParams.Add(uriBuilder.Query.TrimStart('?'));
            }

            // Új paramétereket hozzáadjuk
            foreach (var param in enabledParams)
            {
                var encodedKey = Uri.EscapeDataString(param.Key);
                var encodedValue = Uri.EscapeDataString(param.Value ?? "");
                queryParams.Add($"{encodedKey}={encodedValue}");
            }

            uriBuilder.Query = string.Join("&", queryParams);
            return uriBuilder.Uri.ToString();
        }

        private List<string> ExtractHeaders(HttpResponseMessage response)
        {
            return response.Headers
                .Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")
                .Concat(response.Content.Headers
                    .Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))
                .ToList();
        }

        private static bool IsFileResponse(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentDisposition?.DispositionType
                    ?.Equals("attachment", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

            if (string.IsNullOrEmpty(mediaType)) return false;
            if (mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)) return false;
            if (IsJsonContentType(mediaType)) return false;
            if (mediaType.Contains("xml", StringComparison.OrdinalIgnoreCase)) return false;

            return true;
        }

        private static string ExtractFileName(HttpResponseMessage response)
        {
            var name = response.Content.Headers.ContentDisposition?.FileNameStar
                       ?? response.Content.Headers.ContentDisposition?.FileName;

            if (!string.IsNullOrWhiteSpace(name))
                return name.Trim('"');

            var ext = response.Content.Headers.ContentType?.MediaType?.Split('/').LastOrDefault() ?? "bin";
            return $"download.{ext}";
        }

        private static bool IsJsonContentType(string mediaType)
        {
            if (string.IsNullOrWhiteSpace(mediaType))
                return false;

            return mediaType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
                   mediaType.Contains("text/json", StringComparison.OrdinalIgnoreCase) ||
                   mediaType.Contains("+json", StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            try
            {
                using var document = JsonDocument.Parse(json);
                return JsonSerializer.Serialize(document, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch
            {
                // If parsing fails, return original
                return json;
            }
        }
    }
}
