namespace Postgirl.Domain.Http.Body;

public class JsonBody : HttpBody
{
    public override BodyType Type => BodyType.Json;
    public string Content { get; set; }
}