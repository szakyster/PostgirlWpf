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

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                using var response = await Client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                stopwatch.Stop();

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
            var request = new HttpRequestMessage(model.Method, model.Url);

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
