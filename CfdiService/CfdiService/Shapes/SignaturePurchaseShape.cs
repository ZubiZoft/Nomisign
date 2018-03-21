using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class SignaturePurchaseShape
    {
        public int SignaturePurchaseId { set; get; }
        public int CompanyId { set; get; }
        public int LicensesPurchased { get; set; }
        public int SMSPurchased { get; set; }
        public Decimal Price { get; set; }
        public String PurchaseDate { get; set; }
        public String PaidDate { get; set; }

        public static SignaturePurchaseShape FromDataModel(SignaturePurchase signaturePurchase, HttpRequestMessage request)
        {
            var employeeShape = new SignaturePurchaseShape
            {
                SignaturePurchaseId = signaturePurchase.SignaturePurchaseId,
                CompanyId = signaturePurchase.CompanyId,
                LicensesPurchased = signaturePurchase.LicensesPurchased,
                SMSPurchased = signaturePurchase.SMSPurchased.Value,
                Price = signaturePurchase.Price,
                PurchaseDate = signaturePurchase.PurchaseDate.ToString("dd/MM/yyyy HH:mm:ss"),
                PaidDate = signaturePurchase.PaidDate.ToString("dd/MM/yyyy HH:mm:ss"),
                Links = new LinksClass()
            };

            employeeShape.Links.SelfUri = request.GetLinkUri($"signaturepurchases/{signaturePurchase.SignaturePurchaseId}");
            return employeeShape;
        }

        public static SignaturePurchase ToDataModel(SignaturePurchaseShape signaturePurchaseShape, SignaturePurchase signaturePurchase = null)
        {
            if (signaturePurchase == null)
                signaturePurchase = new SignaturePurchase();

            signaturePurchase.SignaturePurchaseId = signaturePurchaseShape.SignaturePurchaseId;
            signaturePurchase.LicensesPurchased = signaturePurchaseShape.LicensesPurchased;
            signaturePurchase.CompanyId = signaturePurchaseShape.CompanyId;
            signaturePurchase.Price = signaturePurchaseShape.Price;
            signaturePurchase.PurchaseDate = DateTime.Now;
            signaturePurchase.PaidDate = DateTime.Now;

            return signaturePurchase;
        }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }
        public LinksClass Links { get; set; }
    }
}