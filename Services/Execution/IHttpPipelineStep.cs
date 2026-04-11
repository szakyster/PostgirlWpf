using System;
using System.Threading;
using System.Threading.Tasks;

namespace Postgirl.Services.Execution;

public interface IHttpPipelineStep
{
    string Name { get; }
    int Order { get; }
    Task InvokeAsync(HttpPipelineContext context, Func<Task> next, CancellationToken cancellationToken = default);
}
