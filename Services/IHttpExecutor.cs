using System.Threading;
using System.Threading.Tasks;
using Postgirl.Domain.Execution;
using Postgirl.Domain.Http;

namespace Postgirl.Services;

internal interface IHttpExecutor
{
    /// <summary>
    /// Executes the given HTTP request and returns the result of the execution, including the response or any errors that occurred during the process.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpExecutionResult> ExecuteAsync(HttpRequestModel request, CancellationToken cancellationToken = default);
}