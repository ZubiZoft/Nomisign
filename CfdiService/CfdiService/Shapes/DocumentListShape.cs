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
        public string PayperiodDate { get; set; }
        public string UploadTime { get; set; }
        public string SignStatus { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeConcern { get; set; }
        public string PayAmount { get; set; }
        public int AlwaysShow { get; set; }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }

        public LinksClass Links { get; set; }

        public static DocumentListShape FromDataModel(Document document, HttpRequestMessage request)
        {
            var documentListShape = new DocumentListShape
            {
                EmployeeName = string.Format("{0} {1} {2}", document.Employee.FirstName, document.Employee.LastName1, document.Employee.LastName2),
                EmployeeConcern = document.EmployeeConcern,
                EmployeeId = document.EmployeeId,
                UploadTime = document.UploadTime.ToShortDateString(),
                DocumentId = document.DocumentId,
                PayperiodDate = document.PayperiodDate.ToShortDateString(),
                AlwaysShow = document.AlwaysShow,
                PayAmount = document.PayAmount.ToString(),
                SignStatus = document.SignStatus == Model.SignStatus.SinFirma ? "Sin Firma" : document.SignStatus.ToString(),
                Links = new LinksClass()
            };

            documentListShape.Links.SelfUri = request.GetLinkUri($"documents/{documentListShape.DocumentId}");
            return documentListShape;
        }
    }
}