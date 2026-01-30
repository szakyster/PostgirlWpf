using System.Collections.Generic;

namespace Postgirl.Domain.Http.Body;

public class FormUrlEncodedBody : HttpBody
{
    public override BodyType Type => BodyType.FormUrlEncoded;
    public IDictionary<string, string> Fields { get; } = new Dictionary<string, string>();
}