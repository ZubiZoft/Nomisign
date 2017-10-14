using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class UserListShape
    {
        public int UserId { set; get; }
        public string EmailAddress { get; set; }
        public int UserStatus { get; set; }
        public DateTime LastLogin { get; set; }
        public int CompanyId { get; set; }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }

        public LinksClass Links { get; set; }

        public static UserListShape FromDataModel(User user, HttpRequestMessage request)
        {
            var companyUserShape = new UserListShape
            {
                UserId = user.UserId,
                CompanyId = user.CompanyId,
                EmailAddress = user.EmailAddress,
                UserStatus = (int)user.UserStatus,
                LastLogin = user.LastLogin,
                Links = new LinksClass()
            };

            companyUserShape.Links.SelfUri = request.GetLinkUri($"companyusers/{companyUserShape.UserId}");
            return companyUserShape;
        }
    }
}