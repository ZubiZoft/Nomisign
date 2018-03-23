using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CfdiService.Shapes
{
    public class DateRangeRequest
    {
        public string InitDate { get; set; }
        public string EndDate { get; set; }
        public string Rfc { get; set; }
        public string Curp { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
    }
}