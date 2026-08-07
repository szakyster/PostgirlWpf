namespace Postgirl.Domain.Http;

public class RequestHeader(string key, string value, bool isSystem)
{
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;

    /// <summary>
    /// System header: read-only, always active
    /// </summary>
    public bool IsSystem { get; set; } = isSystem;

    /// <summary>
    /// For user headers: indicates whether the header is included in the request.
    /// Always true for system headers.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    public RequestHeader Copy()
    {
        var copy = new RequestHeader(Key, Value, IsSystem)
        {
            IsEnabled = IsEnabled
        };
        return copy;
    }
}