namespace Postgirl.Domain.Http.Body;

public abstract class HttpBody
{
    public abstract BodyType Type { get; }
}