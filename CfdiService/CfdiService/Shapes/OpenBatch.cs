using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CfdiService.Shapes
{
    public class OpenBatch
    {
        public string companyId { get; set; }
        public string authToken { get; set; }
        public int fileCount { get; set; }
        public int BatchId { get; set; }
    }
}