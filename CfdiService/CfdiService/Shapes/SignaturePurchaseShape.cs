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
        public Decimal Price { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime PaidDate { get; set; }

        public static SignaturePurchaseShape FromDataModel(SignaturePurchase signaturePurchase, HttpRequestMessage request)
        {
            var employeeShape = new SignaturePurchaseShape
            {
                SignaturePurchaseId = signaturePurchase.SignaturePurchaseId,
                CompanyId = signaturePurchase.CompanyId,
                LicensesPurchased = signaturePurchase.LicensesPurchased,
                Price = signaturePurchase.Price,
                PurchaseDate = signaturePurchase.PurchaseDate,
                PaidDate = signaturePurchase.PaidDate,
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
            signaturePurchase.PurchaseDate = signaturePurchaseShape.PurchaseDate;
            signaturePurchase.PaidDate = signaturePurchaseShape.PaidDate;

            return signaturePurchase;
        }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }
        public LinksClass Links { get; set; }
    }
}