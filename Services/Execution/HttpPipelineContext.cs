using System.Collections.Generic;
using Postgirl.Domain.Execution;
using Postgirl.Domain.Http;

namespace Postgirl.Services.Execution;

public sealed class HttpPipelineContext
{
    public required HttpRequestModel Request { get; set; }
    public HttpExecutionResult? Result { get; set; }

    /// <summary>Arbitrary data shared between steps within a single execution.</summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();
}
