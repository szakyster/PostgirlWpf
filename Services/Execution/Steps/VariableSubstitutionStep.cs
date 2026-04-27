using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Postgirl.Domain.Http;
using Postgirl.Domain.Http.Body;
using Postgirl.Services;

namespace Postgirl.Services.Execution.Steps;

public sealed class VariableSubstitutionStep : IHttpPipelineStep
{
    public string Name => "VariableSubstitution";
    public int Order => PipelineOrder.VariableSubstitution;

    private readonly ConfigurationService _configurationService;
    private readonly VariablesService _variablesService;

    public VariableSubstitutionStep(ConfigurationService configurationService, VariablesService variablesService)
    {
        _configurationService = configurationService;
        _variablesService = variablesService;
    }

    public Task InvokeAsync(
        HttpPipelineContext context,
        Func<Task> next,
        CancellationToken cancellationToken = default)
    {
        if (_configurationService.GetVariablesEnabled())
        {
            context.Request = ApplyTo(context.Request);
        }

        return next();
    }

    private HttpRequestModel ApplyTo(HttpRequestModel original)
    {
        return new HttpRequestModel(original)
        {
            Url             = Substitute(original.Url),
            Headers         = original.Headers
                                  .Select(h => new RequestHeader(h.Key, Substitute(h.Value), h.IsSystem) { IsEnabled = h.IsEnabled })
                                  .ToList(),
            Parameters      = original.Parameters
                                  .Select(p => new RequestParameter(p.Key, Substitute(p.Value)) { IsEnabled = p.IsEnabled })
                                  .ToList(),
            Body            = SubstituteBody(original.Body),
            BearerToken     = Substitute(original.BearerToken),
        };
    }

    private HttpBody SubstituteBody(HttpBody? body) => body switch
    {
        TextBody text => new TextBody
        {
            Content     = Substitute(text.Content),
            ContentType = text.ContentType
        },
        JsonBody json => new JsonBody
        {
            Json = Substitute(json.Json)
        },
        FormUrlEncodedBody form => new FormUrlEncodedBody
        {
            Items = form.Items
                        .Select(i => new FormUrlEncodedItem { Key = Substitute(i.Key), Value = Substitute(i.Value) })
                        .ToList()
        },
        _ => body ?? new TextBody()
    };

    private string Substitute(string? input) => _variablesService.Substitute(input);
}
