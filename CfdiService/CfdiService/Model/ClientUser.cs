using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CfdiService.Model
{
    public enum ClientUserAdminType { Invalid = 0, HumanResources = 1, Client = 2 }

    public class ClientUser
    {
        public int ClientUserID { get; set; }
        public int ClientCompanyID { set; get; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }  
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public UserStatusType UserStatus { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime LastPasswordChange { get; set; }
        public int FailedLoginCount { get; set; }
        public bool ForcePasswordReset { get; set; }
        public int CreatedByUserId { get; set; }
        public ClientUserAdminType ClientUserType { get; set; }
        public DateTime DateUserCreated { get; set; }
        public string SessionToken { get; set; }
        public DateTime? TokenTimeout { get; set; }
    }
}