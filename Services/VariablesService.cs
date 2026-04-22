using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Postgirl.Domain.Variables;

namespace Postgirl.Services;

public class VariablesService
{
    public ObservableCollection<VariableEntry> Items { get; } = [];

    public void Add(VariableEntry entry) => Items.Add(entry);

    public void Remove(VariableEntry entry) => Items.Remove(entry);

    public bool VariableExists(string key) => Items.Any(e => e.Key == key);

    public string Substitute(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        return PlaceholderRegex.Replace(input, match =>
        {
            var key = match.Groups[1].Value;
            var entry = Items.FirstOrDefault(e => e.Key == key);
            return entry is not null ? entry.Value : match.Value;
        });
    }

    // Matches {{key}} where key may contain word chars, hyphens and dots (see VariableKeyValidator)
    private static readonly Regex PlaceholderRegex =
        new(@"\{\{([\w\-\.]+)\}\}", RegexOptions.Compiled);

    public void SeedDefaults()
    {
        var defaults = new[]
        {
            new VariableEntry { Key = "base_url",        Value = "https://api.example.com" },
            new VariableEntry { Key = "api_version",     Value = "v2" },
            new VariableEntry { Key = "api_key",         Value = "sk-test-abc123xyz" },
            new VariableEntry { Key = "auth_token",      Value = "Bearer eyJhbGciOiJIUzI1NiJ9..." },
            new VariableEntry { Key = "tenant_id",       Value = "acme-corp" },
            new VariableEntry { Key = "user_id",         Value = "usr_98765" },
            new VariableEntry { Key = "timeout_seconds", Value = "30" },
            new VariableEntry { Key = "page_size",       Value = "25" },
            new VariableEntry { Key = "environment",     Value = "staging" },
            new VariableEntry { Key = "region",          Value = "eu-west-1" },
        };

        foreach (var v in defaults)
            Items.Add(v);
    }

    public List<VariableEntry> Export() => Items.ToList();

    public void Import(IEnumerable<VariableEntry> entries)
    {
        Items.Clear();
        foreach (var e in entries.Where(e => VariableKeyValidator.IsValid(e.Key)))
        {
            Items.Add(e);
        }
    }
}
