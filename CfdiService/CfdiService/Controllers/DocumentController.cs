using CfdiService.Model;
using CfdiService.Services;
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CfdiService.Controllers
{
    [RoutePrefix("api")]
    public class DocumentController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string httpDomain = System.Configuration.ConfigurationManager.AppSettings["signingAppDomain"];
        private ModelDbContext db = new ModelDbContext();

        // GET: api/employees
        [HttpGet]
        [Route("documents/{eid}")]
        public IHttpActionResult GetDocuments(int eid)
        {
            var result = new List<DocumentListShape>();
            try {
                
                var docListResult = db.Documents.Where(x => x.EmployeeId == eid).ToList();
                foreach (Document doc in docListResult)
                {
                    result.Add(DocumentListShape.FromDataModel(doc, Request));
                }
            }
            catch(Exception ex)
            {
                log.Error("Error getting documents for user Id:  " + eid, ex);
                return BadRequest(ex.Message);
            }

            return Ok(result);
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
        [Route("documents/unsigned/notify/{cid}")]
        public IHttpActionResult NotifyUnsignedCompanyDocuments(int cid)
        {
            //var result = new List<DocumentListShape>();
            var docListResult = db.Documents.Where(x => x.CompanyId == cid && x.SignStatus == SignStatus.SinFirma).ToList();
            foreach (Document doc in docListResult)
            {
                try {
                    if (doc.Employee != null && !string.IsNullOrEmpty(doc.Employee.CellPhoneNumber))
                    {
                        string smsBody = String.Format(Strings.visitSiteTosignDocumentMessage + ", http://{0}/nomisign", httpDomain);
                        SendSMS.SendSMSMsg(doc.Employee.CellPhoneNumber, smsBody);
                    }
                }
                catch(Exception ex)
                {
                    log.Error("Error sending SMS", ex);
                }
            }
            return Ok("Success");
        }

        // GET: api/companyusers/5
        [HttpGet]
        [Route("documents/{eid}/{id}")]
        public IHttpActionResult GetDocument(int id)
        {
            Document document = null;
            try {
                document = db.Documents.Find(id);
                db.Entry(document).Reference(b => b.Batch).Load();
                if (document == null)
                {
                    return NotFound();
                }
            }
            catch(Exception ex)
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
            if(documentShape.SignStatus == 2 && document.SignStatus != SignStatus.Firmado)
            {
                // sign document
                DigitalSignatures.SignPdfDocument(document);
            }
            DocumentShape.ToDataModel(documentShape, document);
            db.SaveChanges();
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
        [Route("documents/{id}") ]
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