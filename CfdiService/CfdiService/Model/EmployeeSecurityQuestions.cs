using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CfdiService.Model
{
    public class EmployeeSecurityQuestions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int userID { get; set; }
        public string SecurityQuestion1 { get; set; }
        public string SecurityAnswer1 { get; set; }
        public string SecurityQuestion2 { get; set; }
        public string SecurityQuestion3 { get; set; }
        public string SecurityAnswer2 { get; set; }
        public string SecurityAnswer3 { get; set; }
    }
}