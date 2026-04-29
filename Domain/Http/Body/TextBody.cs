using System.Net.Http;
using System.Text;

namespace Postgirl.Domain.Http.Body;

public class TextBody : HttpBody
{
    public override BodyType Type => BodyType.Text;
    public string Content { get; set; } = "";
    public string ContentType { get; set; } = "text/plain";

    public override HttpContent ToHttpContent()
    {
        if (string.IsNullOrWhiteSpace(Content))
        {
            return new StringContent(string.Empty, Encoding.UTF8, ContentType);
        }

        return new StringContent(Content, Encoding.UTF8, ContentType);
    }
    public override string ToString()
    {
        return Content;
    }
}