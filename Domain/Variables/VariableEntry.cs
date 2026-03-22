namespace Postgirl.Domain.Variables;

public class VariableEntry
{
    public VariableEntry() { }

    public VariableEntry(string key) => Key = key;

    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

