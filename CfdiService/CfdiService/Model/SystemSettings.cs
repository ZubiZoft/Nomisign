using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CfdiService.Model
{
    public class SystemSettings
    {
        public SystemSettings()
        {

        }
        public int SystemSettingsId { get; set; }
        public string ProductName { get; set; }
        public string ProductIcon { get; set; }
        public string SupportTelephoneNumber { get; set; }
        public string SystemFilePath1 { get; set; }
        public string SystemFilePath2 { get; set; }
    }
}