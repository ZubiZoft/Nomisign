using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CfdiService.Model
{
    public class SignaturePurchase
    {
        public int SignaturePurchaseId { set; get; }
        public int CompanyId { set; get; }
        public int LicensesPurchased { get; set; }
        public Decimal Price { get; set; }
        public DateTime PurchaseDate { get; set; } 
        public DateTime PaidDate { get; set; }
    }
}