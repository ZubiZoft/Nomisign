using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace CfdiService.Model
{
    public class Employee
    {
        public int EmployeeId { set; get; }
        public int UserId { get; set; }
        public string EmailAddress { get; set; }
        public string PasswordHash { get; set; }
        public virtual User User { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public string FirstName { get; set; }
        public string LastName1 { get; set; }
        public string LastName2 { get; set; }
        public string CURP { get; set; }
        public string RFC { get; set; }
        public string CellPhoneNumber { get; set; }
        public string CellPhoneCarrier { get; set; }
        public int CreatedByUserId{ get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; }
        public DateTime LastLogin { get; set; }
    }
}