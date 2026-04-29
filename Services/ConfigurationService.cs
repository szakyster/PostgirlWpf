using Postgirl.Domain.Configuration;
using Postgirl.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

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
        {
            return value;
        }

        throw new InvalidOperationException($"Configuration entry '{key}' does not contain a valid boolean value.");
    }

    public int GetRetainedHistoryItemCount()
    {
        return GetInt(ConfigurationKeys.RetainedHistoryItemCount);
    }

    public bool GetHistoryGroupByDateEnabled()
    {
        return GetBool(ConfigurationKeys.HistoryGroupByDateEnabled);
    }

    public int GetHttpRequestTimeoutSeconds()
    {
        return GetInt(ConfigurationKeys.HttpRequestTimeoutSeconds);
    }

    public int GetHttpMaxResponseBodySizeKb()
    {
        return GetInt(ConfigurationKeys.HttpMaxResponseBodySizeKb);
    }

    public string GetHttpDefaultUserAgent()
    {
        return GetString(ConfigurationKeys.HttpDefaultUserAgent);
    }

    public bool GetVariablesEnabled()
    {
        return GetBool(ConfigurationKeys.VariablesEnabled);
    }

    public bool GetStorageKeepHistoryBetweenSessions()
    {
        return GetBool(ConfigurationKeys.StorageKeepHistoryBetweenSessions);
    }

    public List<ConfigurationStateEntry> Export()
    {
        return Items
            .Select(entry => new ConfigurationStateEntry
            {
                Key = entry.Key,
                Value = entry.Value
            })
            .ToList();
    }

    public void Import(IEnumerable<ConfigurationStateEntry> entries)
    {
        if (entries == null)
        {
            return;
        }

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
            {
                continue;
            }

            if (!_entriesByKey.TryGetValue(entry.Key, out var existingEntry))
            {
                continue;
            }

            if (!IsValidValueForType(entry.Value, existingEntry.ValueType))
            {
                continue;
            }

            existingEntry.Value = entry.Value;
        }
    }

    private ConfigurationEntry GetTypedEntry(string key, ConfigurationValueType expectedValueType)
    {
        var entry = Get(key);

        if (entry.ValueType == expectedValueType)
            return entry;

        throw new InvalidOperationException(
            $"Configuration entry '{key}' is of type '{entry.ValueType}' instead of '{expectedValueType}'.");
    }

    private static bool IsValidValueForType(string value, ConfigurationValueType valueType)
    {
        return valueType switch
        {
            ConfigurationValueType.String => true,
            ConfigurationValueType.Integer => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            ConfigurationValueType.Boolean => bool.TryParse(value, out _),
            _ => false
        };
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
            },
            new ConfigurationEntry
            {
                Key = ConfigurationKeys.HistoryGroupByDateEnabled,
                DisplayName = "Group history by date",
                ValueType = ConfigurationValueType.Boolean,
                Value = "true"
            },
            new ConfigurationEntry
            {
                Key = ConfigurationKeys.HttpRequestTimeoutSeconds,
                DisplayName = "HTTP request timeout in seconds",
                ValueType = ConfigurationValueType.Integer,
                Value = "30"
            },
            new ConfigurationEntry
            {
                Key = ConfigurationKeys.HttpMaxResponseBodySizeKb,
                DisplayName = "Maximum HTTP response body size in KB",
                ValueType = ConfigurationValueType.Integer,
                Value = "1024"
            },
            new ConfigurationEntry
            {
                Key = ConfigurationKeys.HttpDefaultUserAgent,
                DisplayName = "Default HTTP User-Agent",
                ValueType = ConfigurationValueType.String,
                Value = "Postgirl/1.0"
            },
            new ConfigurationEntry
            {
                Key = ConfigurationKeys.VariablesEnabled,
                DisplayName = "Enable variable handling",
                ValueType = ConfigurationValueType.Boolean,
                Value = "true"
            },
            new ConfigurationEntry
            {
                Key = ConfigurationKeys.StorageKeepHistoryBetweenSessions,
                DisplayName = "Keep history between sessions",
                ValueType = ConfigurationValueType.Boolean,
                Value = "true"
            }
        ];
    }
}