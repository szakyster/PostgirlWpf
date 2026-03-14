namespace Postgirl.Domain.Http;

public class RequestParameter(string key, string value)
{
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;

    /// <summary>
    /// Jelzi, hogy a paraméter csatolva legyen-e az URL-hez
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    public RequestParameter Copy()
    {
        var copy = new RequestParameter(Key, Value)
        {
            IsEnabled = IsEnabled
        };
        return copy;
    }
}
