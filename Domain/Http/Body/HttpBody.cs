using System.Net.Http;

namespace Postgirl.Domain.Http.Body;

public abstract class HttpBody
{
    public abstract BodyType Type { get; }
    public abstract HttpContent? ToHttpContent();

}