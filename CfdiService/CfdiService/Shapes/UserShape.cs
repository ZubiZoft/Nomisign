using CfdiService.Model;
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
        public string LastLogin { get; set; }
        public DateTime LastPasswordChange { get; set; }
        public bool ForcePasswordReset { get; set; }
        public int CompanyId { get; set; }
        public string CreatedByUserName { get; set; }

        public static UserShape FromDataModel(User user, HttpRequestMessage request)
        {
            var userShape = new UserShape
            {
                UserId = user.UserId,
                //EmployeeId = user.EmployeeId,
                CompanyId = user.CompanyId,
                EmailAddress = user.EmailAddress,
                DisplayName = user.DisplayName,
                PhoneNumber = user.PhoneNumber,
                PasswordHash = user.PasswordHash,
                UserStatus = user.UserStatus.ToString(),
                LastLogin = user.LastLogin.ToShortDateString(),
                LastPasswordChange = user.LastPasswordChange,
                ForcePasswordReset = user.ForcePasswordReset,
                //CreatedByUserName = user.CreatedByUser.DisplayName,
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
            //user.EmployeeId = userShape.EmployeeId;
            user.CompanyId = userShape.CompanyId;
            user.EmailAddress = userShape.EmailAddress;
            user.DisplayName = userShape.DisplayName;
            user.PhoneNumber = userShape.PhoneNumber;
            user.PasswordHash = userShape.PasswordHash;

            UserStatusType status = UserStatusType.Active;
            Enum.TryParse<UserStatusType>(userShape.UserStatus, out status);
            user.UserStatus = status;
            var now = DateTime.Now;
            if (DateTime.TryParse(userShape.LastLogin, out now))
            { user.LastLogin = now; }
            user.LastPasswordChange = userShape.LastPasswordChange;
            user.ForcePasswordReset = userShape.ForcePasswordReset;

            return user;
        }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }
        public LinksClass Links { get; set; }
    }
}