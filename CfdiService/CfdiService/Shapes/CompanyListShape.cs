using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class CompanyListShape
    {
        public int CompanyId { set; get; }
        public string CompanyName { get; set; }
        public string CompanyRFC { get; set; }
        public int AccountStatus { get; set; }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }
        public LinksClass Links { get; set; }

        public static CompanyListShape FromDataModel(Company company, HttpRequestMessage request)
        {
            var companyShape = new CompanyListShape
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                CompanyRFC = company.CompanyRFC,
                AccountStatus = (int)company.AccountStatus,
                Links = new LinksClass()
            };

            companyShape.Links.SelfUri = request.GetLinkUri($"companies/{companyShape.CompanyId}");
            return companyShape;
        }
    }
            
}