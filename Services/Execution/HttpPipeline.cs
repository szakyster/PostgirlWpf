using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Postgirl.Domain.Execution;
using Postgirl.Domain.Http;

namespace Postgirl.Services.Execution;

public sealed class HttpPipeline : IHttpPipeline
{
    private readonly List<IHttpPipelineStep> _steps = [];
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly IHttpExecutor _rawExecutor;

    public HttpPipeline(IHttpExecutor rawExecutor) => _rawExecutor = rawExecutor;

    public IReadOnlyList<IHttpPipelineStep> Steps
    {
        get
        {
            _lock.EnterReadLock();
            try { return [.. _steps.OrderBy(s => s.Order)]; }
            finally { _lock.ExitReadLock(); }
        }
    }

    /// <summary>
    /// Registers a step. If a step with the same <see cref="IHttpPipelineStep.Name"/>
    /// already exists it is replaced, allowing runtime reconfiguration.
    /// </summary>
    public void Register(IHttpPipelineStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        _lock.EnterWriteLock();
        try
        {
            _steps.RemoveAll(s => s.Name == step.Name);
            _steps.Add(step);
        }
        finally { _lock.ExitWriteLock(); }
    }

    public void Unregister(string name)
    {
        _lock.EnterWriteLock();
        try { _steps.RemoveAll(s => s.Name == name); }
        finally { _lock.ExitWriteLock(); }
    }

    // IHttpExecutor.ExecuteAsync — callers invoke this without knowing about the pipeline
    public async Task<HttpExecutionResult> ExecuteAsync(
        HttpRequestModel model,
        CancellationToken cancellationToken = default)
    {
        var context = new HttpPipelineContext { Request = model };

        // Snapshot so in-flight pipelines are unaffected by concurrent Register/Unregister
        IReadOnlyList<IHttpPipelineStep> snapshot;
        _lock.EnterReadLock();
        try { snapshot = [.. _steps.OrderBy(s => s.Order)]; }
        finally { _lock.ExitReadLock(); }

        await BuildPipeline(snapshot, context, cancellationToken)();

        return context.Result ?? new HttpExecutionResult
        {
            IsSuccess = false,
            Error = HttpError.FromException(new InvalidOperationException("Pipeline produced no result."))
        };
    }

    private Func<Task> BuildPipeline(
        IReadOnlyList<IHttpPipelineStep> steps,
        HttpPipelineContext context,
        CancellationToken ct)
    {
        // Terminal step: the actual HTTP call
        Func<Task> terminal = async () =>
            context.Result = await _rawExecutor.ExecuteAsync(context.Request, ct);

        // Build the chain in reverse so the first step (lowest Order) is outermost
        return steps
            .Reverse()
            .Aggregate(terminal, (next, step) =>
                () => step.InvokeAsync(context, next, ct));
    }
}
