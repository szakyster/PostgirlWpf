namespace Postgirl.Domain.Http;

public class ResponseHeader(string key, string value)
{
    public string Key { get; } = key;
    public string Value { get; } = value;
}