using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class DocumentShape
    {
        public int DocumentId { set; get; }
        public int EmployeeId { get; set; }
        //public virtual Employee Employee { get; set; }
        public DateTime UploadTime { get; set; }
        public DateTime PayperiodDate { get; set; }
        public int SignStatus { get; set; }
        public string DocumentBytes { get; set; }
        public Nullable<int> BatchId { get; set; }
        //public virtual Batch Batch { get; set; }

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
                BatchId  = document.BatchId,
                //Batch = document.Batch,
                //Employee = document.Employee,
                DocumentBytes = System.Convert.ToBase64String(plainTextBytes),
                UploadTime = document.UploadTime,
                PayperiodDate = document.PayperiodDate,
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
            //document.Employee = documentShape.Employee;
            document.UploadTime = documentShape.UploadTime;
            document.PayperiodDate = documentShape.PayperiodDate;
            document.SignStatus = (SignStatus)documentShape.SignStatus;
            document.BatchId = documentShape.BatchId;
            //document.Batch = documentShape.Batch;

            return document;
        }
    }
}