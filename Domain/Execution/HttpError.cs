using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Postgirl.Domain.Execution;

public class HttpError
{
    public HttpErrorType Type { get; init; }
    public string Message { get; init; } = "";
    public string? Details { get; init; }

    public static HttpError FromException(Exception ex)
    {
        return ex switch
        {
            TaskCanceledException => new HttpError
            {
                Type = HttpErrorType.Timeout,
                Message = "The request timed out."
            },

            HttpRequestException httpEx => new HttpError
            {
                Type = HttpErrorType.Network,
                Message = httpEx.Message,
                Details = httpEx.InnerException?.Message
            },

            _ => new HttpError
            {
                Type = HttpErrorType.Unknown,
                Message = ex.Message
            }
        };
    }
}