namespace Postgirl.Domain.Http.Body;

public class TextBody : HttpBody
{
    public override BodyType Type => BodyType.Text;
    public string Content { get; set; }
}