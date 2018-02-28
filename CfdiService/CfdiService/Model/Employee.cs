using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace CfdiService.Model
{
    public enum EmployeeStatusType { Invalid = 0, Unverified = 1, Active = 2, PasswordFailureLocked = 3, PasswordResetLocked = 4, PasswordAwaitingLocked = 5 }

    public class Employee
    {
        public int EmployeeId { set; get; }
        //public int UserId { get; set; }
        public string EmailAddress { get; set; }
        public string PasswordHash { get; set; }
        //public virtual User User { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public string FirstName { get; set; }
        public string LastName1 { get; set; }
        public string LastName2 { get; set; }
        public string FullName { get; set; }
        public string CURP { get; set; }
        public string RFC { get; set; }
        public string CellPhoneNumber { get; set; }
        //public string CellPhoneCarrier { get; set; }
        public int CreatedByUserId{ get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime CreatedDate { get; set; } 
        public EmployeeStatusType EmployeeStatus { get; set; }

        public string SessionToken { get; set; }

        public DateTime? TokenTimeout { get; set; }

        // error in data type here
        public int FailedLoginCount { get; set; }
    }
}