namespace Postgirl.Domain.Configuration;

public class ConfigurationEntry
{
    public string Key { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public ConfigurationValueType ValueType { get; init; }
    public string Value { get; set; } = string.Empty;
}
