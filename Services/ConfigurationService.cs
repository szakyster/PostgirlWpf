using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Postgirl.Domain.Configuration;

namespace Postgirl.Services;

public class ConfigurationService
{
    private readonly Dictionary<string, ConfigurationEntry> _entriesByKey;

    public ReadOnlyCollection<ConfigurationEntry> Items { get; }

    public ConfigurationService()
    {
        var entries = CreateDefaultEntries();
        Items = entries.AsReadOnly();
        _entriesByKey = entries.ToDictionary(entry => entry.Key, StringComparer.OrdinalIgnoreCase);
    }

    public ConfigurationEntry Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key cannot be null or whitespace.", nameof(key));

        if (_entriesByKey.TryGetValue(key, out var entry))
            return entry;

        throw new KeyNotFoundException($"Configuration entry was not found: '{key}'.");
    }

    public string GetString(string key)
    {
        var entry = GetTypedEntry(key, ConfigurationValueType.String);
        return entry.Value;
    }

    public int GetInt(string key)
    {
        var entry = GetTypedEntry(key, ConfigurationValueType.Integer);

        if (int.TryParse(entry.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            return value;

        throw new InvalidOperationException($"Configuration entry '{key}' does not contain a valid integer value.");
    }

    public bool GetBool(string key)
    {
        var entry = GetTypedEntry(key, ConfigurationValueType.Boolean);

        if (bool.TryParse(entry.Value, out var value))
            return value;

        throw new InvalidOperationException($"Configuration entry '{key}' does not contain a valid boolean value.");
    }

    public int GetRetainedHistoryItemCount()
        => GetInt(ConfigurationKeys.RetainedHistoryItemCount);

    private ConfigurationEntry GetTypedEntry(string key, ConfigurationValueType expectedValueType)
    {
        var entry = Get(key);

        if (entry.ValueType == expectedValueType)
            return entry;

        throw new InvalidOperationException(
            $"Configuration entry '{key}' is of type '{entry.ValueType}' instead of '{expectedValueType}'.");
    }

    private static List<ConfigurationEntry> CreateDefaultEntries()
    {
        return
        [
            new ConfigurationEntry
            {
                Key = ConfigurationKeys.RetainedHistoryItemCount,
                DisplayName = "Retained history item count",
                ValueType = ConfigurationValueType.Integer,
                Value = "100"
            }
        ];
    }
}
