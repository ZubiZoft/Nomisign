using CfdiService.Model;
using CfdiService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class UserShape
    {
        public int UserId { set; get; }
        //public int EmployeeId { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }  // is this needed
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string UserStatus { get; set; }
        public string UserType { get; set; }
        public string LastLogin { get; set; }
        public DateTime LastPasswordChange { get; set; }
        public bool ForcePasswordReset { get; set; }
        public int CompanyId { get; set; }
        public string CreatedByUserName { get; set; }
        public int CreatedByUserId { get; set; }

        public static UserShape FromDataModel(User user, HttpRequestMessage request)
        {
            var userShape = new UserShape
            {
                UserId = user.UserId,
                CompanyId = user.CompanyId,
                EmailAddress = user.EmailAddress,
                DisplayName = user.DisplayName,
                PhoneNumber = user.PhoneNumber,
                PasswordHash = string.Empty, // user.PasswordHash, this should always return empty, or not at all.  for login and password reset dont use the shape 
                UserStatus = user.UserStatus.ToString(),
                UserType = user.UserType.ToString(),
                LastLogin = user.LastLogin.ToShortDateString(),
                LastPasswordChange = user.LastPasswordChange,
                ForcePasswordReset = user.ForcePasswordReset,
                CreatedByUserId = user.CreatedByUserId,
                Links = new LinksClass()
            };

            userShape.Links.SelfUri = request.GetLinkUri($"companyusers/{userShape.UserId}");
            return userShape;
        }

        public static User ToDataModel(UserShape userShape, User user = null)
        {
            if (user == null)
                user = new User();

            user.UserId = userShape.UserId;
            user.CompanyId = userShape.CompanyId;
            user.CreatedByUserId = userShape.CreatedByUserId;
            user.EmailAddress = userShape.EmailAddress;
            user.DisplayName = userShape.DisplayName;
            user.PhoneNumber = userShape.PhoneNumber;
            user.LastPasswordChange = userShape.LastPasswordChange;
            user.ForcePasswordReset = userShape.ForcePasswordReset;
            if (!String.IsNullOrEmpty(userShape.PasswordHash))
            {
                user.PasswordHash = EncryptionService.Sha256_hash(userShape.PasswordHash, string.Empty);
            }

            // user status enum
            UserStatusType status = UserStatusType.Active;
            Enum.TryParse<UserStatusType>(userShape.UserStatus, out status);
            user.UserStatus = status;

            // user type enum
            UserAdminType type = UserAdminType.Invalid;
            Enum.TryParse<UserAdminType>(userShape.UserType, out type);
            user.UserType = type;

            // last login info
            var now = DateTime.Now;
            if (DateTime.TryParse(userShape.LastLogin, out now))
            { user.LastLogin = now; }

            return user;
        }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }
        public LinksClass Links { get; set; }
    }
}