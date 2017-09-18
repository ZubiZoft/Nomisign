using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class SignaturePurchaseListShape
    {
        public int SignaturePurchaseId { set; get; }
        public int CompanyId { set; get; }
        public int LicensesPurchased { get; set; }
        public Decimal Price { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime PaidDate { get; set; }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }

        public LinksClass Links { get; set; }

        public static SignaturePurchaseListShape FromDataModel(SignaturePurchase purchase, HttpRequestMessage request)
        {
            var purchaseShape = new SignaturePurchaseListShape
            {
                SignaturePurchaseId = purchase.SignaturePurchaseId,
                CompanyId = purchase.CompanyId,
                LicensesPurchased = purchase.LicensesPurchased,
                Price = purchase.Price,
                PurchaseDate = purchase.PurchaseDate,
                PaidDate = purchase.PaidDate,
                Links = new LinksClass()
            };

            purchaseShape.Links.SelfUri = request.GetLinkUri($"signaturepurchases/{purchaseShape.SignaturePurchaseId}");
            return purchaseShape;
        }
    }
}