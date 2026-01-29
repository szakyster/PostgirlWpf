namespace Postgirl.Domain.Http;

public class RequestHeader(string key, string value, bool isSystem)
{
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;

    /// <summary>
    /// System header: nem szerkeszthető, mindig aktív
    /// </summary>
    public bool IsSystem { get; } = isSystem;

    /// <summary>
    /// User header esetén jelzi, hogy ténylegesen csatoljuk-e
    /// System headernél mindig true
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}