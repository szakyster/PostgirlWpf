
using System.Collections.Generic;

namespace Postgirl.Domain.Http
{
    public class HttpResponseResult
    {
        public int StatusCode { get; set; }
        public IReadOnlyList<string> Headers { get; set; }
        public string Body { get; set; }
        public string ContentType { get; set; }

        public long ElapsedMilliseconds { get; set; }
        public long ResponseSize { get; set; }

        #nullable enable
        public ResponseFile? File { get; set; }
        public bool IsFile => File != null;
    }
}
