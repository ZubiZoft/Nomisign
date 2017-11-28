using CfdiService.Model;
using CfdiService.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class DocumentShape
    {
        public int DocumentId { set; get; }
        public int EmployeeId { get; set; }
        public int CompanyId { get; set; }
        //public virtual Employee Employee { get; set; }
        public String UploadTime { get; set; }
        public String PayperiodDate { get; set; }
        public int SignStatus { get; set; }
        public String SignStatusText { get; set; }
        public string DocumentBytes { get; set; }
        
        public Nullable<int> BatchId { get; set; }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }
        public LinksClass Links { get; set; }

        public static DocumentShape FromDataModel(Document document, HttpRequestMessage request)
        {
            // untill i get file IO working, do this
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("sample sign doc info");

            var documentShape = new DocumentShape
            {
                DocumentId = document.DocumentId,
                EmployeeId = document.EmployeeId,
                BatchId = document.BatchId,
                DocumentBytes = NomiFileAccess.GetFile(document), 
                UploadTime = document.UploadTime.ToShortDateString(),
                PayperiodDate = document.PayperiodDate.ToShortDateString(),
                SignStatusText = document.SignStatus.ToString(),
                SignStatus = (int)document.SignStatus,
                Links = new LinksClass()
            };

            documentShape.Links.SelfUri = request.GetLinkUri($"documents/{documentShape.DocumentId}");
            return documentShape;
        }

        public static Document ToDataModel(DocumentShape documentShape, Document document = null)
        {
            if (document == null)
                document = new Document();

            document.DocumentId = documentShape.DocumentId;
            document.EmployeeId = documentShape.EmployeeId;
            document.UploadTime = DateTime.Parse(documentShape.UploadTime);
            document.PayperiodDate = DateTime.Parse(documentShape.PayperiodDate);
            SignStatus status = Model.SignStatus.Invalid;
            Enum.TryParse<SignStatus>(documentShape.SignStatus.ToString(), out status);
            document.SignStatus = status;
            document.BatchId = documentShape.BatchId;
            // make sure this does not change
            return document;
        }
    }
}