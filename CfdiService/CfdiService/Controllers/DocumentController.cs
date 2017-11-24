using CfdiService.Model;
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
        private ModelDbContext db = new ModelDbContext();

        // GET: api/employees
        [HttpGet]
        [Route("documents/{eid}")]
        public IHttpActionResult GetDocuments(int eid)
        {
            var result = new List<DocumentListShape>();
            var docListResult = db.Documents.Where(x => x.EmployeeId == eid).ToList();
            foreach (Document doc in docListResult)
            {
                result.Add(DocumentListShape.FromDataModel(doc, Request));
            }
            return Ok(result);
        }

        // GET: api/companydocs
        [HttpGet]
        [Route("documents/rejected/{cid}")]
        public IHttpActionResult GetRejectedCompanyDocuments(int cid)
        {
            var result = new List<DocumentListShape>();
            var docListResult = db.Documents.Where(x => x.CompanyId == cid && x.SignStatus == SignStatus.Refused).ToList();
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
            var docListResult = db.Documents.Where(x => x.CompanyId == cid && x.SignStatus == SignStatus.Unsigned).ToList();
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

        // GET: api/companyusers/5
        [HttpGet]
        [Route("documents/{eid}/{id}")]
        public IHttpActionResult GetDocument(int id)
        {
            Document document = db.Documents.Find(id);
            db.Entry(document).Reference(b => b.Batch).Load();
            if (document == null)
            {
                return NotFound();
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