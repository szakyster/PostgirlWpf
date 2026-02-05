using System.Collections.Generic;

namespace Postgirl.Domain.Http.Body;

public class FormUrlEncodedBody : HttpBody
{
    public override BodyType Type => BodyType.FormUrlEncoded;
    public IList<FormUrlEncodedItem> Items { get; } = new List<FormUrlEncodedItem>();
}