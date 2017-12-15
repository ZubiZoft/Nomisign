using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CfdiService.Model
{
    [Table("EmployeesCode")]
    public class EmployeesCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EmployeeId { get; set; }
        public string Prefix { get; set; }
        //public string X2 { get; set; }
        public string Vcode { get; set; }
        public DateTime GeneratedDate { get; set; }
    }
}