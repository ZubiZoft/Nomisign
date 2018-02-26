using CfdiService.Model;
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

namespace CfdiService.Controllers
{
    [RoutePrefix("api")]
    public class DocumentController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string httpDomain = System.Configuration.ConfigurationManager.AppSettings["signingAppDomain"];
        protected readonly string tempZipPath = System.Configuration.ConfigurationManager.AppSettings["tempZipFolder"];
        private ModelDbContext db = new ModelDbContext();


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
        public IHttpActionResult GetDocumentsByCompanyDateRange(int cid, DateRangeRequest range)
        {
            var result = new List<DocumentListShape>();
            log.Info("Init Date: " + range.InitDate);
            log.Info("End Date: " + range.EndDate);
            DateTime initdateT = DateTime.Parse(range.InitDate);
            DateTime enddateT = DateTime.Parse(range.EndDate).AddDays(1);
            var docListResult = db.Documents.Where(x => x.CompanyId == cid && x.PayperiodDate >= initdateT && enddateT >= x.PayperiodDate).ToList();
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
        public IHttpActionResult SendNotificationsToUnsignedDocuments([FromBody] List<int> dids)
        {
            foreach (int a in dids)
            {
                Document doc = db.Documents.Where(x => x.DocumentId == a).First();
                Employee emp = db.Employees.Where(x => x.EmployeeId == doc.EmployeeId).First();
                string msgBodySpanish = String.Format(Strings.visitSiteTosignDocumentMessage, httpDomain);

                if (null != emp.CellPhoneNumber)
                {
                    //SendSMS.SendSMSMsg(emp.CellPhoneNumber, msgBodySpanish);
                    string res = "";
                    SendSMS.SendSMSQuiubo(msgBodySpanish, string.Format("+52{0}", emp.CellPhoneNumber), out res);
                }
                if (null != emp.EmailAddress)
                {
                    SendEmail.SendEmailMessage(emp.EmailAddress, Strings.visitSiteTosignDocumentMessageEmailSubject, msgBodySpanish);
                }
            }
            return Ok("Success");
        }

        // GET: api/companydocs
        [HttpGet]
        [Route("documents/rejected/{cid}")]
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
            }
            db.SaveChanges();
            return Ok("Success");
        }

        // GET: api/companydocs
        [HttpGet]
        [Route("documents/unsigned/notify/{cid}")]
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
                        string smsBody = String.Format(Strings.visitSiteTosignDocumentMessage + ", http://{0}/nomisign", httpDomain);
                        //SendSMS.SendSMSMsg(doc.Employee.CellPhoneNumber, smsBody);
                        string res = "";
                        SendSMS.SendSMSQuiubo(smsBody, string.Format("+52{0}", doc.Employee.CellPhoneNumber), out res);
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
                        string res = "";
                        SendSMS.SendSMSQuiubo(smsBody, string.Format("+52{0}", doc.Employee.CellPhoneNumber), out res);
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
                return response;
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
        [Route("documents/{eid}/{id}")]
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
            }
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
                    document.Nom151 = Nom151Service.CreateNom151(NomiFileAccess.GetFilePath(document));
                    log.Info(document.Nom151);
                }
                catch (Exception ex) { log.Info(ex.ToString()); }
            }
            db.SaveChanges();
            DocumentShape.ToDataModel(documentShape, document);
            return Ok(documentShape);
        }

        // POST: api/companyusers
        [HttpPost]
        [Route("documents")]
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