using System.Net.Http;
using System.Text;

namespace Postgirl.Domain.Http.Body;

public class JsonBody : HttpBody
{
    public override BodyType Type => BodyType.Json;
    public string Json { get; set; } = "";

    public override HttpContent ToHttpContent()
    {
        if (string.IsNullOrWhiteSpace(Json))
        { 
            return new StringContent(string.Empty, Encoding.UTF8, "application/json");
        }
        return new StringContent(Json, Encoding.UTF8, "application/json");
    }

    public override string ToString()
    {
        return Json;
    }
}
