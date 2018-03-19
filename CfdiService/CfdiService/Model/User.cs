using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace CfdiService.Model
{
    public enum UserStatusType { Invalid = 0, Unverified = 1, Active = 2, PasswordFailureLocked = 3, PasswordResetLocked = 4 }
    public enum UserAdminType { Invalid = 0, HumanResources= 1, CompanyAdmin = 2, GlobalAdmin = 3 }

    public class User
    {
        public int CompanyId { get; set; }
        public int UserId { set; get; }
        //public int EmployeeId { get { return 0; } set { } }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }  // is this needed
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public UserStatusType UserStatus { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime LastPasswordChange { get; set; }
        public int FailedLoginCount { get; set; }
        public bool ForcePasswordReset { get; set; }
        public int  CreatedByUserId { get; set; }
        public UserAdminType UserType { get; set; }
        public DateTime DateUserCreated { get; set; }
        public string SessionToken { get; set; }
        public DateTime? TokenTimeout { get; set; }
    }
}