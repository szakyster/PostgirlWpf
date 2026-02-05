namespace Postgirl.Domain.Http.Body;

public class FormUrlEncodedItem
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public FormUrlEncodedItem Copy()
        => new() { Key = Key, Value = Value };
}