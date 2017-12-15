using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CfdiService.Model
{
    [Table("ClientCompanies")]
    public class Client
    {
        [Key]
        public int ClientCompanyID { get; set; }
        public string ClientCompanyName { get; set; }
        public string ClientCompanyRFC { get; set; } 
    }
}