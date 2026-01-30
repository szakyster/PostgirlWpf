using Postgirl.Domain.Http;
using Postgirl.Domain.Http.Body;

namespace Postgirl.Domain.Models;

public class TextBody : HttpBody
{
    public override BodyType Type => BodyType.Text;
    public string Content { get; set; }
}