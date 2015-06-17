using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LinkChecker
{
    [DelimitedRecord(",")] 
    class UrlTransform
    {
        public string OldUrl { get; set; }
        public string NewUrl { get; set; }
    }
}
