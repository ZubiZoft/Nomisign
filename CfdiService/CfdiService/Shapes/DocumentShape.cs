﻿using CfdiService.Model;
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
        public Nullable<int> ClientCompanyId { get; set; }
        public String UploadTime { get; set; }
        public String PayperiodDate { get; set; }
        public int SignStatus { get; set; }
        public String SignStatusText { get; set; }
        public string DocumentBytes { get; set; }
        public string EmployeeConcern { get; set; }
        public Nullable<int> BatchId { get; set; }
        public decimal PayAmount { get; set; }
        public int AlwaysShow { get; set; }
        public string NomCert { get; set; }
        public DateTime? StartingPeriod { get; set; }
        public DateTime? EndPeriod { get; set; }
        public string UUID { get; set; }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }
        public LinksClass Links { get; set; }

        public static DocumentShape FromDataModel(Document document, HttpRequestMessage request)
        {
            // untill i get file IO working, do this
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("sample sign doc info");
            string httpDomain = System.Configuration.ConfigurationManager.AppSettings["signingAppDomain"];
            string httpDomainPrefix = System.Configuration.ConfigurationManager.AppSettings["DomainHttpPrefix"];

        var documentShape = new DocumentShape
            {
                DocumentId = document.DocumentId,
                ClientCompanyId = document.ClientCompanyId,
                EmployeeId = document.EmployeeId,
                BatchId = document.BatchId,
                DocumentBytes = string.Format("{2}://{0}/nomiservice/document-handler/viewer.cshtml?docid={1}", httpDomain, document.DocumentId, httpDomainPrefix),
                EmployeeConcern = document.EmployeeConcern,
                AlwaysShow = document.AlwaysShow,
                PayAmount = document.PayAmount,
                UploadTime = document.UploadTime.ToShortDateString(),
                PayperiodDate = document.PayperiodDate.ToShortDateString(),
                SignStatusText = document.SignStatus == Model.SignStatus.SinFirma ? "Sin Firma" : document.SignStatus.ToString(), // this Sin Firma needs addressed and not hard coded
                SignStatus = (int)document.SignStatus,
                NomCert = document.Nom151Cert,
                StartingPeriod = document.StartingPeriod,
                EndPeriod = document.EndPeriod,
                UUID = document.UUID,
                Links = new LinksClass()
            };

            documentShape.Links.SelfUri = request.GetLinkUri($"documents/{documentShape.DocumentId}");
            return documentShape;
        }

        public static Document ToDataModel(DocumentShape documentShape, Document document = null)
        {
            if (document == null)
                document = new Document();

            document.PayAmount = documentShape.PayAmount;
            document.ClientCompanyId = documentShape.ClientCompanyId;
            document.DocumentId = documentShape.DocumentId;
            document.EmployeeId = documentShape.EmployeeId;
            document.EmployeeConcern = documentShape.EmployeeConcern;
            document.UploadTime = DateTime.Parse(documentShape.UploadTime);
            document.PayperiodDate = DateTime.Parse(documentShape.PayperiodDate);
            SignStatus status = Model.SignStatus.Invalid;
            Enum.TryParse<SignStatus>(documentShape.SignStatus.ToString(), out status);
            document.SignStatus = status;
            if (null != documentShape.BatchId)
            {
                document.BatchId = documentShape.BatchId;
            }
            return document;
        }
    }
}