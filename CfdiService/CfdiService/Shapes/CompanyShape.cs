using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class CompanyShape
    {
        public int CompanyId { set; get; }
        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public int PayPeriod { get; set; }
        public string DocStoragePath1 { get; set; }
        public string DocStoragePath2 { get; set; }
        public string CompanyRFC { get; set; }
        public string BillingEmailAddress { get; set; }
        //public string CorporateEmailDomain { get; set; }
        public long TotalSignaturesPurchased { get; set; }
        public long SignatureBalance { get; set; }
        public int AccountStatus { get; set; }
        public bool NewEmployeeGetDoc { get; set; }
        public string NewEmployeeDocument { get; set; }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }
        public LinksClass Links { get; set; }

        public static CompanyShape FromDataModel(Company company, HttpRequestMessage request)
        {
            var companyShape = new CompanyShape
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                Address1 = company.Address1,
                Address2 = company.Address2,
                City = company.City,
                State = company.State,
                PostalCode = company.PostalCode,
                PayPeriod = (int)company.PayPeriod,
                DocStoragePath1 = company.DocStoragePath1,
                DocStoragePath2 = company.DocStoragePath2,
                CompanyRFC = company.CompanyRFC,
                BillingEmailAddress = company.BillingEmailAddress,
                TotalSignaturesPurchased = company.TotalSignaturesPurchased,
                SignatureBalance = company.SignatureBalance,
                AccountStatus = (int)company.AccountStatus,
                NewEmployeeGetDoc = company.NewEmployeeGetDoc == NewEmployeeGetDocType.AddDocument,
                NewEmployeeDocument = company.NewEmployeeDocument
            };
            return companyShape;
        }

        public static Company ToDataModel(CompanyShape companyShape, Company company = null)
        {
            if (company == null)
                company = new Company();

            company.CompanyName = companyShape.CompanyName;
            company.Address1 = companyShape.Address1;
            company.Address2 = companyShape.Address2;
            company.City = companyShape.City;
            company.State = companyShape.State;
            company.PostalCode = companyShape.PostalCode;
            company.PayPeriod = (PayPeriodType)companyShape.PayPeriod;
            company.DocStoragePath1 = String.IsNullOrEmpty(companyShape.DocStoragePath1) ?  Guid.NewGuid().ToString(): companyShape.DocStoragePath1; 
            company.DocStoragePath2 = String.IsNullOrEmpty(companyShape.DocStoragePath2) ? Guid.NewGuid().ToString() : companyShape.DocStoragePath2; 
            company.CompanyRFC = companyShape.CompanyRFC;
            company.BillingEmailAddress = companyShape.BillingEmailAddress;
            company.AccountStatus = (AccountStatusType)companyShape.AccountStatus;
            company.NewEmployeeGetDoc = companyShape.NewEmployeeGetDoc ? NewEmployeeGetDocType.AddDocument : NewEmployeeGetDocType.None;
            company.NewEmployeeDocument = companyShape.NewEmployeeDocument;
            // these should not be directly overwritten
            // company.TotalSignaturesPurchased,
            // company.SignatureBalance,

            return company;
        }
    }
}