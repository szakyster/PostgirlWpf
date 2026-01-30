using Postgirl.Domain.Http;
using Postgirl.Domain.Http.Body;

namespace Postgirl.Domain.Models;

public class JsonBody : HttpBody
{
    public override BodyType Type => BodyType.Json;
    public string Content { get; set; }
}