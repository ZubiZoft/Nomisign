﻿using CfdiService.Model;
using CfdiService.Services;
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using System.IO.Compression;
using CfdiService.Filters;

namespace CfdiService.Controllers
{
    [RoutePrefix("api")]
    public class DocumentController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string httpDomain = System.Configuration.ConfigurationManager.AppSettings["signingAppDomain"];
        protected readonly string tempZipPath = System.Configuration.ConfigurationManager.AppSettings["tempZipFolder"];
        private ModelDbContext db = new ModelDbContext();
        protected static readonly Dictionary<string, int> DicTypes = new Dictionary<string, int>()
        {
            { "Documento", 1 },
            { "Recibo", 2 }
        };
        protected static readonly Dictionary<string, int> DicStatuses = new Dictionary<string, int>()
        {
            { "Sin Firma", 1 },
            { "Firmado", 2 },
            { "Rechazado", 3 }
        };


        /*[HttpGet]
        [Route("documents/{id}/{file}/Download")]
        public IHttpActionResult GetReceipts(int id)
        {
            var result = new List<DocumentListShape>();
            try {

                var docListResult = db.Documents.Where(x => x.DocumentId == id).ToList();
                foreach (Document doc in docListResult)
                {
                   
                }
            }
            catch(Exception ex)
            {
                log.Error("No Reciepts found under that id");
                return BadRequest(ex.Message);
            }
            
            return Ok(result);
        }
        */


        // GET: api/employees
        [HttpGet]
        [Route("documents/{eid}")]
        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetDocuments(int eid)
        {
            var result = new List<DocumentListShape>();
            try
            {
                var emp = db.Employees.FirstOrDefault(e => e.EmployeeId == eid);
                var emps = db.Employees.Where(e => e.CURP == emp.CURP).ToList();
                foreach (var e in emps)
                {
                    var docListResult = db.Documents.Where(x => x.EmployeeId == e.EmployeeId).ToList();
                    foreach (Document doc in docListResult)
                    {
                        result.Add(DocumentListShape.FromDataModel(doc, Request));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error getting documents for user Id:  " + eid, ex);
                return BadRequest(ex.Message);
            }

            return Ok(result);
        }

        // GET: api/companydocs
        [HttpGet]
        [Route("documentsByCompany/{cid}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetDocumentsByCompany(int cid)
        {
            var result = new List<DocumentListShape>();
            var docListResult = db.Documents.Where(x => x.CompanyId == cid).ToList();
            foreach (Document doc in docListResult)
            {
                var docShape = DocumentListShape.FromDataModel(doc, Request);
                // not validating company ID here
                Employee employee = db.Employees.Find(doc.EmployeeId);
                if (employee != null)
                {
                    docShape.EmployeeName = string.Format("{0} {1} {2}", employee.FirstName, employee.LastName1, employee.LastName2);
                }
                result.Add(docShape);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("documentsByCompanyDateRange/{cid}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetDocumentsByCompanyDateRange(int cid, DateRangeRequest range)
        {
            var result = new List<DocumentListShape>();
            log.Info("Init Date: " + range.InitDate);
            log.Info("End Date: " + range.EndDate);
            DateTime initdateT = DateTime.Parse(range.InitDate);
            DateTime enddateT = DateTime.Parse(range.EndDate).AddDays(1);
            List<Document> docListResult = null;
            var user = db.Users.Find(int.Parse(User.Identity.GetName()));

            if (string.IsNullOrEmpty(range.Curp) &&
                    string.IsNullOrEmpty(range.Rfc) &&
                    string.IsNullOrEmpty(range.Status) &&
                    string.IsNullOrEmpty(range.Type))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                            enddateT >= x.PayperiodDate
                        ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                            enddateT >= x.PayperiodDate &&
                            x.CompanyId == user.CompanyId
                        ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Curp) &&
                    string.IsNullOrEmpty(range.Rfc) &&
                    string.IsNullOrEmpty(range.Status))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.AlwaysShow == DicTypes[range.Type]
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.AlwaysShow == DicTypes[range.Type] &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Curp) &&
                    string.IsNullOrEmpty(range.Rfc) &&
                    string.IsNullOrEmpty(range.Type))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status]
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Curp) &&
                    string.IsNullOrEmpty(range.Status) &&
                    string.IsNullOrEmpty(range.Type))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.Employee.RFC == range.Rfc
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.Employee.RFC == range.Rfc &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Rfc) &&
                           string.IsNullOrEmpty(range.Status) &&
                           string.IsNullOrEmpty(range.Type))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.Employee.CURP == range.Curp
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.Employee.CURP == range.Curp &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Curp) &&
                    string.IsNullOrEmpty(range.Rfc))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.AlwaysShow == DicTypes[range.Type] &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status]
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.AlwaysShow == DicTypes[range.Type] &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] && 
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Curp) &&
                    string.IsNullOrEmpty(range.Type))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.Employee.RFC == range.Rfc
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.Employee.RFC == range.Rfc &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Status) &&
                    string.IsNullOrEmpty(range.Type))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.Employee.RFC == range.Rfc &&
                        x.Employee.CURP == range.Curp
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.Employee.RFC == range.Rfc &&
                        x.Employee.CURP == range.Curp &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Rfc) &&
                    string.IsNullOrEmpty(range.Type))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.Employee.CURP == range.Curp
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.Employee.CURP == range.Curp &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Curp) &&
                    string.IsNullOrEmpty(range.Status))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.AlwaysShow == DicTypes[range.Type] &&
                        x.Employee.RFC == range.Rfc
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.AlwaysShow == DicTypes[range.Type] &&
                        x.Employee.RFC == range.Rfc &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Curp))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.AlwaysShow == DicTypes[range.Type] &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.Employee.RFC == range.Rfc
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.AlwaysShow == DicTypes[range.Type] &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.Employee.RFC == range.Rfc &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Rfc))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.AlwaysShow == DicTypes[range.Type] &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.Employee.CURP == range.Curp
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.AlwaysShow == DicTypes[range.Type] &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.Employee.CURP == range.Curp &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Status))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.Employee.RFC == range.Rfc &&
                        x.Employee.CURP == range.Curp &&
                        x.AlwaysShow == DicTypes[range.Type]
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.Employee.RFC == range.Rfc &&
                        x.Employee.CURP == range.Curp &&
                        x.AlwaysShow == DicTypes[range.Type] &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else if (string.IsNullOrEmpty(range.Type))
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.Employee.RFC == range.Rfc &&
                        x.Employee.CURP == range.Curp
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.Employee.RFC == range.Rfc &&
                        x.Employee.CURP == range.Curp &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            else
            {
                if (user.UserType == UserAdminType.GlobalAdmin)
                {
                    docListResult = db.Documents.Where(x => x.CompanyId == cid &&
                        x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.AlwaysShow == DicTypes[range.Type] &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.Employee.RFC == range.Rfc &&
                        x.Employee.CURP == range.Curp
                    ).ToList();
                }
                else
                {
                    docListResult = db.Documents.Where(x => x.PayperiodDate >= initdateT &&
                        enddateT >= x.PayperiodDate &&
                        x.AlwaysShow == DicTypes[range.Type] &&
                        x.SignStatus == (SignStatus)DicStatuses[range.Status] &&
                        x.Employee.RFC == range.Rfc &&
                        x.Employee.CURP == range.Curp &&
                        x.CompanyId == user.CompanyId
                    ).ToList();
                }
            }
            foreach (Document doc in docListResult)
            {
                var docShape = DocumentListShape.FromDataModel(doc, Request);
                // not validating company ID here
                Employee employee = db.Employees.Find(doc.EmployeeId);
                if (employee != null)
                {
                    docShape.EmployeeName = string.Format("{0} {1} {2}", employee.FirstName, employee.LastName1, employee.LastName2);
                }
                result.Add(docShape);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("SendNotificationsToUnsignedDocuments/")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult SendNotificationsToUnsignedDocuments([FromBody] List<int> dids)
        {
            log.Info("SendNotificationsToUnsignedDocuments");
            foreach (int a in dids)
            {
                log.Info(string.Format("Document Id : {0}", a.ToString()));
                Document doc = db.Documents.Where(x => x.DocumentId == a).First();
                log.Info(string.Format("1"));
                Employee emp = db.Employees.Where(x => x.EmployeeId == doc.EmployeeId).First();
                log.Info(string.Format("2"));
                string msgBodySpanish = String.Format(Strings.visitSiteTosignDocumentMessage, httpDomain, emp.Company.CompanyName, doc.PayperiodDate.ToString("dd/MM/yyyy"));
                string msgBodySpanishMobile = String.Format(Strings.visitSiteTosignDocumentSMS, emp.Company.CompanyName, doc.PayperiodDate.ToString("dd/MM/yyyy"), httpDomain);
                log.Info(string.Format("3"));
                if (null != emp.CellPhoneNumber)
                {
                    log.Info(string.Format("4"));
                    //SendSMS.SendSMSMsg(emp.CellPhoneNumber, msgBodySpanish);
                    if (doc.Company.SMSBalance > 0)
                    {
                        string res = "";
                        SendSMS.SendSMSQuiubo(msgBodySpanishMobile, string.Format("+52{0}", emp.CellPhoneNumber), out res);
                        doc.Company.SMSBalance -= 1;
                        db.SaveChanges();
                    }
                    if (doc.Company.SMSBalance <= 10)
                    {
                        try { SendEmail.SendEmailMessage(doc.Company.BillingEmailAddress, string.Format(Strings.smsQuantityWarningSubject, doc.Company.CompanyName), string.Format(Strings.smsQuantityWarning, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }
                        try { SendEmail.SendEmailMessage("mariana.basto@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, doc.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }
                        try { SendEmail.SendEmailMessage("estela.gonzalez@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, doc.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }
                        try { SendEmail.SendEmailMessage("info@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, doc.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }
                        try { SendEmail.SendEmailMessage("artturobldrq@gmail.com", string.Format(Strings.smsWarningSalesMessageSubject, doc.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }

                    }
                }
                if (null != emp.EmailAddress)
                {
                    log.Info(string.Format("5"));
                    SendEmail.SendEmailMessage(emp.EmailAddress, Strings.visitSiteTosignDocumentMessageEmailSubject, msgBodySpanish);
                }
                log.Info(string.Format("6"));
            }
            return Ok("Success");
        }

        // GET: api/companydocs
        [HttpGet]
        [Route("documents/rejected/{cid}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetRejectedCompanyDocuments(int cid)
        {
            var result = new List<DocumentListShape>();
            var docListResult = db.Documents.Where(x => x.CompanyId == cid && x.SignStatus == SignStatus.Rechazado).ToList();
            foreach (Document doc in docListResult)
            {
                var docShape = DocumentListShape.FromDataModel(doc, Request);
                // not validating company ID here
                Employee employee = db.Employees.Find(doc.EmployeeId);
                if (employee != null)
                {
                    docShape.EmployeeName = string.Format("{0} {1} {2}", employee.FirstName, employee.LastName1, employee.LastName2);
                }
                result.Add(docShape);
            }
            return Ok(result);
        }

        // GET: api/companydocs
        [HttpGet]
        [Route("documents/unsigned/{cid}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetUnsignedCompanyDocuments(int cid)
        {
            var result = new List<DocumentListShape>();
            var docListResult = db.Documents.Where(x => x.CompanyId == cid && x.SignStatus == SignStatus.SinFirma).ToList();
            foreach (Document doc in docListResult)
            {
                var docShape = DocumentListShape.FromDataModel(doc, Request);
                Employee employee = db.Employees.Find(doc.EmployeeId);
                if (employee != null)
                {
                    docShape.EmployeeName = string.Format("{0} {1} {2}", employee.FirstName, employee.LastName1, employee.LastName2);
                }
                result.Add(docShape);
            }
            return Ok(result);
        }

        // GET: api/companydocs
        [HttpGet]
        [Route("documents/employeesigned/{eid}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetSignedEmployeeDocuments(int eid)
        {
            var result = new List<DocumentListShape>();
            var docListResult = db.Documents.Where(x => x.EmployeeId == eid && x.SignStatus == SignStatus.Firmado).ToList();
            foreach (Document doc in docListResult)
            {
                var docShape = DocumentListShape.FromDataModel(doc, Request);
                Employee employee = db.Employees.Find(doc.EmployeeId);
                if (employee != null)
                {
                    docShape.EmployeeName = string.Format("{0} {1} {2}", employee.FirstName, employee.LastName1, employee.LastName2);
                }
                result.Add(docShape);
            }
            return Ok(result);
        }

        // POST: api/companydocs
        [HttpPost]
        [Route("documents/rejected")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult SendDocumentsToUnsignedStatus([FromBody] List<int> dids)
        {
            if (dids == null)
            {
                BadRequest();
            }
            var docs = new List<Document>();
            foreach (int id in dids)
            {
                Document document = db.Documents.Find(id);
                if (document == null)
                {
                    return NotFound();
                }
                docs.Add(document);
            }
            foreach (var d in docs)
            {
                d.SignStatus = SignStatus.SinFirma;
                string emailBody = String.Format(Strings.visitSiteTosignDocumentMessage, httpDomain, d.Employee.Company.CompanyName, d.PayperiodDate.ToString("dd/MM/yyyy"));
                if (d.Company.SMSBalance > 0)
                {
                    //string smsBody = String.Format(Strings.visitSiteTosignDocumentSMS, doc.Employeeemp.Company.CompanyName, doc.PayperiodDate.ToString("dd/MM/yyyy"), httpDomain);
                    string smsBody = String.Format(Strings.visitSiteTosignDocumentSMS + ", http://{0}/nomisign", httpDomain);
                    //string msgBodySpanish = String.Format(Strings.visitSiteTosignDocumentMessage, httpDomain, emp.Company.CompanyName, doc.PayperiodDate.ToString("dd/MM/yyyy"));
                    //SendSMS.SendSMSMsg(doc.Employee.CellPhoneNumber, smsBody);
                    string res = "";
                    SendSMS.SendSMSQuiubo(smsBody, string.Format("+52{0}", d.Employee.CellPhoneNumber), out res);
                    d.Company.SMSBalance -= 1;
                    db.SaveChanges();
                }
                if (d.Company.SMSBalance <= 10)
                {
                    try { SendEmail.SendEmailMessage(d.Company.BillingEmailAddress, string.Format(Strings.smsQuantityWarningSubject, d.Company.CompanyName), string.Format(Strings.smsQuantityWarning, httpDomain, d.Company.CompanyName, d.Company.SMSBalance)); } catch { }
                    try { SendEmail.SendEmailMessage("mariana.basto@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, d.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, d.Company.CompanyName, d.Company.SMSBalance)); } catch { }
                    try { SendEmail.SendEmailMessage("estela.gonzalez@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, d.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, d.Company.CompanyName, d.Company.SMSBalance)); } catch { }
                    try { SendEmail.SendEmailMessage("info@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, d.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, d.Company.CompanyName, d.Company.SMSBalance)); } catch { }
                    try { SendEmail.SendEmailMessage("artturobldrq@gmail.com", string.Format(Strings.smsWarningSalesMessageSubject, d.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, d.Company.CompanyName, d.Company.SMSBalance)); } catch { }

                }
                SendEmail.SendEmailMessage(d.Employee.EmailAddress, Strings.visitSiteTosignDocumentMessageEmailSubject, emailBody);
            }
            db.SaveChanges();

            foreach (var d in docs)
                db.CreateLog(OperationTypes.DocumentUpdated, string.Format("Cambio de estatus a esperando firma {0}",
                    d.DocumentId), User, d.DocumentId, ObjectTypes.Document);

            return Ok("Success");
        }

        // GET: api/companydocs
        [HttpGet]
        [Route("documents/unsigned/notify/{cid}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult NotifyUnsignedCompanyDocuments(int cid)
        {
            //var result = new List<DocumentListShape>();
            var docListResult = db.Documents.Where(x => x.CompanyId == cid && x.SignStatus == SignStatus.SinFirma).ToList();
            foreach (Document doc in docListResult)
            {
                try
                {
                    if (doc.Employee != null && !string.IsNullOrEmpty(doc.Employee.CellPhoneNumber))
                    {
                        if (doc.Company.SMSBalance > 0)
                        {
                            //string smsBody = String.Format(Strings.visitSiteTosignDocumentSMS, doc.Employeeemp.Company.CompanyName, doc.PayperiodDate.ToString("dd/MM/yyyy"), httpDomain);
                            string smsBody = String.Format(Strings.visitSiteTosignDocumentSMS + ", http://{0}/nomisign", httpDomain);
                            //string msgBodySpanish = String.Format(Strings.visitSiteTosignDocumentMessage, httpDomain, emp.Company.CompanyName, doc.PayperiodDate.ToString("dd/MM/yyyy"));
                            //SendSMS.SendSMSMsg(doc.Employee.CellPhoneNumber, smsBody);
                            string res = "";
                            SendSMS.SendSMSQuiubo(smsBody, string.Format("+52{0}", doc.Employee.CellPhoneNumber), out res);
                            doc.Company.SMSBalance -= 1;
                            db.SaveChanges();
                        }
                        if (doc.Company.SMSBalance <= 10)
                        {
                            try { SendEmail.SendEmailMessage(doc.Company.BillingEmailAddress, string.Format(Strings.smsQuantityWarningSubject, doc.Company.CompanyName), string.Format(Strings.smsQuantityWarning, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }
                            try { SendEmail.SendEmailMessage("mariana.basto@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, doc.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }
                            try { SendEmail.SendEmailMessage("estela.gonzalez@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, doc.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }
                            try { SendEmail.SendEmailMessage("info@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, doc.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }
                            try { SendEmail.SendEmailMessage("artturobldrq@gmail.com", string.Format(Strings.smsWarningSalesMessageSubject, doc.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }

                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error sending SMS", ex);
                }
            }
            return Ok("Success");
        }

        // POST: api/companydocs
        [HttpPost]
        [Route("documents/unsigned/notify")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult NotifyUnsignedDocuments([FromBody] List<int> cid)
        {
            //var result = new List<DocumentListShape>();
            var docListResult = db.Documents.Where(x => cid.Contains(x.DocumentId) && x.SignStatus == SignStatus.SinFirma).ToList();
            foreach (Document doc in docListResult)
            {
                try
                {
                    if (doc.Employee != null && !string.IsNullOrEmpty(doc.Employee.CellPhoneNumber))
                    {
                        string smsBody = String.Format(Strings.visitSiteTosignDocumentMessage + ", http://{0}/nomisign", httpDomain);
                        //SendSMS.SendSMSMsg(doc.Employee.CellPhoneNumber, smsBody);
                        if (doc.Company.SMSBalance > 0)
                        {
                            string res = "";
                            SendSMS.SendSMSQuiubo(smsBody, string.Format("+52{0}", doc.Employee.CellPhoneNumber), out res);
                            doc.Company.SMSBalance -= 1;
                            db.SaveChanges();
                        }
                        if (doc.Company.SMSBalance <= 10)
                        {
                            try { SendEmail.SendEmailMessage(doc.Company.BillingEmailAddress, string.Format(Strings.smsQuantityWarningSubject, doc.Company.CompanyName), string.Format(Strings.smsQuantityWarning, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }
                            try { SendEmail.SendEmailMessage("mariana.basto@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, doc.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }
                            try { SendEmail.SendEmailMessage("estela.gonzalez@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, doc.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }
                            try { SendEmail.SendEmailMessage("info@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, doc.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }
                            try { SendEmail.SendEmailMessage("artturobldrq@gmail.com", string.Format(Strings.smsWarningSalesMessageSubject, doc.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, doc.Company.CompanyName, doc.Company.SMSBalance)); } catch { }

                        }
                        SendEmail.SendEmailMessage(doc.Employee.EmailAddress, Strings.visitSiteTosignDocumentMessageEmailSubject, smsBody);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error sending SMS", ex);
                }
            }
            return Ok("Success");
        }

        [HttpPost]
        [Route("documents/download/")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public HttpResponseMessage DownloadDocumentsAsZip([FromBody] List<int> cid)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            log.Info(1);
            var docListResult = db.Documents.Where(x => cid.Contains(x.DocumentId)).ToList();
            log.Info(1.1);
            var tempZip = Path.Combine(tempZipPath, System.Guid.NewGuid() + ".zip");
            try
            {
                log.Info(2);
                using (var fs = new FileStream(tempZip, FileMode.Create, FileAccess.Write))
                {
                    using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
                    {
                        log.Info(2.1);
                        int i = 0;
                        foreach (var d in docListResult)
                        {
                            log.Info("*");
                            var batch = db.Batches.Find(d.BatchId);
                            // var fName = Path.Combine(NomiFileAccess.GetFilePath(d), d.PathToFile + ".pdf");
                            var fName = NomiFileAccess.GetFilePath(d);
                            log.Info(fName);
                            archive.CreateEntryFromFile(fName, i.ToString() + ".pdf");
                            if (d.Nom151 != null)
                            {
                                var entryNom = archive.CreateEntry(i.ToString() + ".txt");
                                using (var writer = new StreamWriter(entryNom.Open()))
                                {
                                    writer.WriteLine(d.Nom151);
                                }
                            }
                            i++;
                        }

                    }
                }
                log.Info(4);
                var dataBytes = File.ReadAllBytes(tempZip);
                File.Delete(tempZip);
                var dataStream = new MemoryStream(dataBytes);

                response.Content = new StreamContent(dataStream);
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = "Nominas.zip";
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                log.Info(3);
            }
            catch (Exception ex)
            {
                log.Info(ex);
                log.Info(ex.Message);
                log.Info(ex.StackTrace);
                log.Info(ex.Source);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            log.Info(5);
            return response;

        }

        // GET: api/companyusers/5
        [HttpGet]
        [Route("documentsEmployee/{id}")]
        [Authorize(Roles = "ADMIN,EMPLOYEE,CLIENT")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetDocument(int id)
        {
            Document document = null;
            try
            {
                document = db.Documents.Find(id);
                db.Entry(document).Reference(b => b.Batch).Load();
                if (document == null)
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                log.Error("Error getting document Id:  " + id, ex);
                return BadRequest(ex.Message);
            }

            return Ok(DocumentShape.FromDataModel(document, Request));
        }

        [HttpPut]
        [Route("documents/{id}")]
        [Authorize(Roles = "EMPLOYEE,ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult UpdateDocument(int id, DocumentShape documentShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != documentShape.DocumentId)
            {
                return BadRequest();
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return NotFound();
            }
            if (document.Company.SignatureBalance > 0)
            {
                if (documentShape.SignStatus == 2 && document.SignStatus != SignStatus.Firmado)
                {
                    // sign document
                    if (document.AlwaysShow == 0)
                    {
                        document.PathToFile = Path.GetFileNameWithoutExtension(DigitalSignatures.SignPdfDocument(document));
                    }
                    else
                    {
                        document.PathToFile = Path.GetFileNameWithoutExtension(DigitalSignatures.SignPdfDocument(document));
                    }
                    db.CreateLog(OperationTypes.SignedBy, string.Format("Documento Firmado {0}", document.DocumentId), User,
                            document.DocumentId, ObjectTypes.Document);
                }
            }
            else
            {
                return BadRequest();
            }
            document.Company.SignatureBalance -= 1;
            document.SignStatus = (SignStatus)documentShape.SignStatus;
            log.Info("Employee Concern : " + documentShape.EmployeeConcern);
            if (documentShape.SignStatus == 3)
            {
                log.Info("Employee Concern : " + documentShape.EmployeeConcern);
                document.EmployeeConcern = documentShape.EmployeeConcern;
            }
            log.Info(document.PathToFile);
            db.SaveChanges();

            if (document.AlwaysShow == 0 && document.SignStatus == SignStatus.Firmado)
            {
                try
                {
                    document.Nom151 = Nom151Service.CreateNom151(NomiFileAccess.GetFilePath(document), document);
                    log.Info(document.Nom151);
                }
                catch (Exception ex) { log.Info(ex.ToString()); }
            }
            if (document.SignStatus != SignStatus.Rechazado)
            {
                db.CreateLog(OperationTypes.DocumentUpdated, string.Format("Documento actualizado {0}", document.DocumentId), User,
                        document.DocumentId, ObjectTypes.Document);
            }
            else
            {
                db.CreateLog(OperationTypes.DocumentRejected, string.Format("Documento rechazado {0}", document.DocumentId), User,
                        document.DocumentId, ObjectTypes.Document);
            }
            db.SaveChanges();
            DocumentShape.ToDataModel(documentShape, document);
            return Ok(documentShape);
        }

        // POST: api/companyusers
        [HttpPost]
        [Route("documents")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult AddDocument(DocumentShape documentShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Document document = DocumentShape.ToDataModel(documentShape);
            db.Documents.Add(document);
            db.SaveChanges();
            return Ok(DocumentShape.FromDataModel(document, Request));
        }

        // DELETE: api/companyusers/5
        [HttpDelete]
        [Route("documents/{id}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult DeleteDocument(int id)
        {
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return NotFound();
            }
            db.Documents.Remove(document);
            db.SaveChanges();
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserId == id) > 0;
        }
    }
}