using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CfdiService.Shapes
{
    public class FileUpload
    {
        public string FileName { get; set; } 
        public string XMLContent { get; set; }
        public string PDFContent { get; set; }
        public string EmployeeCURP { get; set; }
        public string FileHash { get; set; }
    }
}