using System.Collections.Generic;

namespace Postgirl.Domain.Models
{
    public class HttpResponseResult
    {
        public int StatusCode { get; set; }
        public IReadOnlyList<string> Headers { get; set; }
        public string Body { get; set; }

        public long ElapsedMilliseconds { get; set; }
        public long ResponseSize { get; set; }
    }
}
