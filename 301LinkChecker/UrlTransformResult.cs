using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LinkChecker
{
    class UrlTransformResult
    {
        public UrlTransform SourceUrls { get; set; }
        public Task<HttpResponseMessage> Response { get; set; }
    }
}
