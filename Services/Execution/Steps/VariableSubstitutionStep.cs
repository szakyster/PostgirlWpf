using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Postgirl.Services;

namespace Postgirl.Services.Execution.Steps;

public sealed class VariableSubstitutionStep : IHttpPipelineStep
{
    public string Name => "VariableSubstitution";
    public int Order => PipelineOrder.VariableSubstitution;

    private readonly VariablesService _variablesService;

    public VariableSubstitutionStep(VariablesService variablesService)
    {
        _variablesService = variablesService;
    }

    public Task InvokeAsync(
        HttpPipelineContext context,
        Func<Task> next,
        CancellationToken cancellationToken = default)
    {
        // TODO: substitute {{variable}} placeholders in context.Request
        return next();
    }

    private string Substitute(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        return PlaceholderRegex.Replace(input, match =>
        {
            var key = match.Groups[1].Value;
            var entry = _variablesService.Items.FirstOrDefault(e => e.Key == key);
            return entry is not null ? entry.Value : match.Value;
        });
    }

    // Matches {{key}} where key may contain word chars, hyphens and dots (see VariableKeyValidator)
    private static readonly Regex PlaceholderRegex =
        new(@"\{\{([\w\-\.]+)\}\}", RegexOptions.Compiled);
}
