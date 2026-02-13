using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System;
using System.Threading;
using System.Threading.Tasks;
using Postgirl.Domain.Execution;
using Postgirl.Domain.Http;

namespace Postgirl.Services
{
    internal class HttpExecutor : IHttpExecutor
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

                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                stopwatch.Stop();

                return new HttpExecutionResult
                {
                    IsSuccess = true,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Response = new HttpResponseResult
                    {
                        StatusCode = (int)response.StatusCode,
                        Headers = ExtractHeaders(response),
                        Body = body,
                        ResponseSize = Encoding.UTF8.GetByteCount(body)
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
            var request = new HttpRequestMessage(model.Method, model.Url);

            foreach (var header in model.Headers.Where(h => h.IsEnabled))
            {
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    request.Content ??= new StringContent("");
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (model.Body != null && !string.IsNullOrWhiteSpace(model.Body.ToString()))
            {
                request.Content = new StringContent(
                    model.Body.ToString() ?? string.Empty,
                    Encoding.UTF8,
                    "application/json"); // később BodyType alapján
            }

            return request;
        }

        private List<string> ExtractHeaders(HttpResponseMessage response)
        {
            return response.Headers
                .Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")
                .Concat(response.Content.Headers
                    .Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))
                .ToList();
        }
    }
}
