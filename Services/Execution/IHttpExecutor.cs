using Postgirl.Domain.Execution;
using Postgirl.Domain.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Postgirl.Services.Execution;

public interface IHttpExecutor
{
    /// <summary>
    /// Executes the given HTTP request and returns the result of the execution, including the response or any errors that occurred during the process.
    /// </summary>
    Task<HttpExecutionResult> ExecuteAsync(HttpRequestModel request, CancellationToken cancellationToken = default);
}
