using System.Collections.Generic;

namespace Postgirl.Services.Execution;

/// <summary>
/// Extends IHttpExecutor — callers only need IHttpExecutor,
/// IHttpPipeline is used solely for step registration.
/// </summary>
public interface IHttpPipeline : IHttpExecutor
{
    IReadOnlyList<IHttpPipelineStep> Steps { get; }
    void Register(IHttpPipelineStep step);
    void Unregister(string name);
}
