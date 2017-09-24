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
        [Route("documents")]
        public IHttpActionResult GetDocuments()
        {
            var result = new List<DocumentListShape>();
            foreach (var c in db.Documents)
            {
                result.Add(DocumentListShape.FromDataModel(c, Request));
            }
            return Ok(result);
        }

        // GET: api/companyusers/5
        [HttpGet]
        [Route("documents/{id}")]
        public IHttpActionResult GetDocument(int id)
        {
            Document document = db.Documents.Find(id);
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