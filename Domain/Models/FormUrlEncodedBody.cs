using System.Collections.Generic;
using Postgirl.Domain.Http;

namespace Postgirl.Domain.Models;

public class FormUrlEncodedBody : HttpBody
{
    public override BodyType Type => BodyType.FormUrlEncoded;
    public IDictionary<string, string> Fields { get; } = new Dictionary<string, string>();
}