using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Postgirl.Domain.Http.Body;

public class FormUrlEncodedBody : HttpBody
{
    public override BodyType Type => BodyType.FormUrlEncoded;
    public IList<FormUrlEncodedItem> Items { get; set; } = new List<FormUrlEncodedItem>();

    public override HttpContent? ToHttpContent()
    {
        var validItems = Items
            .Where(i => !string.IsNullOrWhiteSpace(i.Key))
            .Select(i => new KeyValuePair<string, string>(i.Key, i.Value ?? ""))
            .ToList();
        return new FormUrlEncodedContent(validItems);
    }
}