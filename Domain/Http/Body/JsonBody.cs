using static System.Net.Mime.MediaTypeNames;

namespace Postgirl.Domain.Http.Body;

public class JsonBody : HttpBody
{
    public override BodyType Type => BodyType.Json;
    public string Json { get; set; }

    public override string ToString()
    {
        return Json;
    }
}