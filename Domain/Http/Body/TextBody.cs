namespace Postgirl.Domain.Http.Body;

public class TextBody : HttpBody
{
    public override BodyType Type => BodyType.Text;
    public string Text { get; set; }

    public override string ToString()
    {
        return Text;
    }
}