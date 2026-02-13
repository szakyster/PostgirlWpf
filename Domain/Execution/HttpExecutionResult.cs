using Postgirl.Domain.Http;

namespace Postgirl.Domain.Execution
{
    internal class HttpExecutionResult
    {
        public bool IsSuccess { get; init; }
        public HttpResponseResult? Response { get; init; }
        public HttpError? Error { get; init; }
        public long ElapsedMilliseconds { get; init; }
    }
}
