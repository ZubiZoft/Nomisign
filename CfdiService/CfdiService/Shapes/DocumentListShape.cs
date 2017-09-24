using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class DocumentListShape
    {
        public int DocumentId { set; get; }
        public int EmployeeId { get; set; }
        public DateTime PayperiodDate { get; set; }
        public int SignStatus { get; set; }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }

        public LinksClass Links { get; set; }

        public static DocumentListShape FromDataModel(Document document, HttpRequestMessage request)
        {
            var documentListShape = new DocumentListShape
            {
                EmployeeId = document.EmployeeId,
                DocumentId = document.DocumentId,
                PayperiodDate = document.PayperiodDate,
                SignStatus = (int)document.SignStatus,
                Links = new LinksClass()
            };

            documentListShape.Links.SelfUri = request.GetLinkUri($"documents/{documentListShape.DocumentId}");
            return documentListShape;
        }
    }
}