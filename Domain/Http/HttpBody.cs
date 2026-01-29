using Postgirl.Domain.Models;

namespace Postgirl.Domain.Http;

public abstract class HttpBody
{
    public abstract BodyType Type { get; }
}