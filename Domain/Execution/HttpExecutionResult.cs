using Postgirl.Domain.Http;

namespace Postgirl.Domain.Execution
{
    public class HttpExecutionResult
    {
        public bool IsSuccess { get; init; }
        #nullable enable
        public HttpResponseResult? Response { get; init; }
        #nullable enable
        public HttpError? Error { get; init; }
        public long ElapsedMilliseconds { get; init; }
    }
}
