using Postgirl.Domain.Authentication;
using Postgirl.Domain.Execution;
using Postgirl.Domain.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Postgirl.Services.Execution;

public class HttpExecutor : IHttpExecutor
{
    private readonly ConfigurationService _configurationService;

    private static readonly HttpClient Client = new();
    private static readonly HttpClient WindowsAuthenticationClient = new(new HttpClientHandler
    {
        UseDefaultCredentials = true
    });

    public HttpExecutor(ConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public async Task<HttpExecutionResult> ExecuteAsync(
        HttpRequestModel model,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var requestCancellationTokenSource = CreateRequestCancellationTokenSource(model, cancellationToken);
            var requestCancellationToken = requestCancellationTokenSource.Token;
            using var request = BuildRequestMessage(model);
            var client = GetHttpClient(model);

            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                requestCancellationToken);

            stopwatch.Stop();

            var responseBytes = await ReadResponseBytesAsync(response.Content, requestCancellationToken);

            if (IsFileResponse(response))
            {
                var fileName = ExtractFileName(response);

                return new HttpExecutionResult
                {
                    IsSuccess = true,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Response = new HttpResponseResult
                    {
                        StatusCode = (int)response.StatusCode,
                        Headers = ExtractHeaders(response),
                        Body = $"[File: {fileName} — {responseBytes.Length:N0} bytes]",
                        File = new ResponseFile { FileName = fileName, Bytes = responseBytes },
                        ResponseSize = responseBytes.Length,
                        ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
                    }
                };
            }

            var responseBody = GetResponseEncoding(response.Content).GetString(responseBytes);
            var contentType = response.Content.Headers.ContentType?.MediaType;

            return new HttpExecutionResult
            {
                IsSuccess = true,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Response = new HttpResponseResult
                {
                    StatusCode = (int)response.StatusCode,
                    Headers = ExtractHeaders(response),
                    Body = responseBody,
                    ContentType = contentType,
                    ResponseSize = responseBytes.Length,
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
        ApplyDefaultUserAgent(model, request);

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

    private static HttpClient GetHttpClient(HttpRequestModel model)
    {
        return model.AuthType == AuthType.WindowsAuthentication
            ? WindowsAuthenticationClient
            : Client;
    }

    private CancellationTokenSource CreateRequestCancellationTokenSource(HttpRequestModel model, CancellationToken cancellationToken)
    {
        var requestCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var timeout = model.Timeout ?? GetConfiguredTimeout();

        if (timeout > TimeSpan.Zero)
        {
            requestCancellationTokenSource.CancelAfter(timeout);
        }

        return requestCancellationTokenSource;
    }

    private TimeSpan GetConfiguredTimeout()
    {
        var requestTimeoutSeconds = _configurationService.GetHttpRequestTimeoutSeconds();

        if (requestTimeoutSeconds <= 0)
        {
            return Timeout.InfiniteTimeSpan;
        }

        return TimeSpan.FromSeconds(requestTimeoutSeconds);
    }

    private void ApplyDefaultUserAgent(HttpRequestModel model, HttpRequestMessage request)
    {
        if (model.Headers.Any(h => h.IsEnabled && string.Equals(h.Key, "User-Agent", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var defaultUserAgent = _configurationService.GetHttpDefaultUserAgent();

        if (string.IsNullOrWhiteSpace(defaultUserAgent))
        {
            return;
        }

        request.Headers.TryAddWithoutValidation("User-Agent", defaultUserAgent);
    }

    private async Task<byte[]> ReadResponseBytesAsync(HttpContent content, CancellationToken cancellationToken)
    {
        var maxResponseBodySizeInBytes = GetMaxResponseBodySizeInBytes();

        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        await using var buffer = new MemoryStream();
        var chunk = new byte[81920];

        while (true)
        {
            var bytesRead = await stream.ReadAsync(chunk.AsMemory(0, chunk.Length), cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            if (maxResponseBodySizeInBytes > 0 && buffer.Length + bytesRead > maxResponseBodySizeInBytes)
            {
                throw new InvalidOperationException($"Response body exceeds the configured limit of {_configurationService.GetHttpMaxResponseBodySizeKb()} KB.");
            }

            await buffer.WriteAsync(chunk.AsMemory(0, bytesRead), cancellationToken);
        }

        return buffer.ToArray();
    }

    private long GetMaxResponseBodySizeInBytes()
    {
        var maxResponseBodySizeKb = Math.Max(0, _configurationService.GetHttpMaxResponseBodySizeKb());
        return maxResponseBodySizeKb * 1024L;
    }

    private static Encoding GetResponseEncoding(HttpContent content)
    {
        var charset = content.Headers.ContentType?.CharSet;

        if (string.IsNullOrWhiteSpace(charset))
        {
            return Encoding.UTF8;
        }

        try
        {
            return Encoding.GetEncoding(charset);
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
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

        if (!string.IsNullOrEmpty(uriBuilder.Query))
        {
            queryParams.Add(uriBuilder.Query.TrimStart('?'));
        }

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
        {
            return true;
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

        if (string.IsNullOrEmpty(mediaType))
        {
            return false;
        }

        if (mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (mediaType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
            mediaType.Contains("text/json", StringComparison.OrdinalIgnoreCase) ||
            mediaType.Contains("+json", StringComparison.OrdinalIgnoreCase)) return false;

        if (mediaType.Contains("xml", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static string ExtractFileName(HttpResponseMessage response)
    {
        var name = response.Content.Headers.ContentDisposition?.FileNameStar
                   ?? response.Content.Headers.ContentDisposition?.FileName;

        if (!string.IsNullOrWhiteSpace(name))
        {
            return name.Trim('"');
        }

        var ext = response.Content.Headers.ContentType?.MediaType?.Split('/').LastOrDefault() ?? "bin";
        return $"download.{ext}";
    }

}
