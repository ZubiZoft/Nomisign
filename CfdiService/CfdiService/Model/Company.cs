using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CfdiService.Model
{
    public enum PayPeriodType : int { Invalid = 0, Monthly = 1, TwiceMonthly = 2, Weekly = 3 }
    public enum AccountStatusType : int { Invalid = 0, WaitingApproval = 1, Active = 2, Suspended = 3, Deactivated = 4 }
    public enum NewEmployeeGetDocType : int { None = 0, AddDocument = 1 }

    public class Company
    {
        public Company()
        {
            Purchases = new HashSet<SignaturePurchase>();
            Employees = new HashSet<Employee>();
            Batches = new HashSet<Batch>();
        }
        public NewEmployeeGetDocType NewEmployeeGetDoc { get; set; }
        public string NewEmployeeDocument { get; set; }
        public int CompanyId { set; get; }
        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public PayPeriodType PayPeriod { get; set; }
        public string DocStoragePath1 { get; set; }
        public string DocStoragePath2 { get; set; }
        public string CompanyRFC { get; set; }
        public string BillingEmailAddress { get; set; }
        //public string CorporateEmailDomain { get; set; }
        public long TotalSignaturesPurchased { get; set; }
        public long SignatureBalance { get; set; }
        public AccountStatusType AccountStatus { get; set; }
        public string ApiKey { get; set; }
        public virtual ICollection<SignaturePurchase> Purchases { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<Batch> Batches { get; set; }
    }
}