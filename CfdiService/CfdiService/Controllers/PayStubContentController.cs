using CfdiService.Model;
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CfdiService.Controllers
{
    [RoutePrefix("api/upload")]
    public class UploadController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();
        // temp !!!
        private const string httpRootpath = "e:\\web\\ogreancom00\\htdocs\\nomisign\\";

        [HttpPost]
        [Route("openbatch/{rfc}")]
        public IHttpActionResult OpenBatch(string rfc, [FromBody] CfdiService.Shapes.OpenBatch batchInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Company company = db.Companies.FirstOrDefault(e => e.CompanyRFC == rfc);
            if(company == null)
            {
                return NotFound();
            }

            Batch batch = new Batch();
            batch.CompanyId = company.CompanyId;
            batch.BatchOpenTime = DateTime.Now;
            batch.BatchCloseTime = DateTime.Now;
            batch.ItemCount = batchInfo.fileCount;
            batch.ActualItemCount = batchInfo.fileCount;
            batch.WorkDirectory = company.DocStoragePath1;
            
            db.Batches.Add(batch);
            db.SaveChanges();
            return Ok(new OpenBatch() { BatchId = batch.BatchId, companyId = company.CompanyId.ToString(), fileCount = batchInfo.fileCount  } );
        }

        [HttpPost]
        [Route("addfile/{batchid}")]
        public IHttpActionResult AddFile(string batchid, [FromBody] CfdiService.Shapes.FileUpload batchInfo)
        {
            Batch batch = db.Batches.FirstOrDefault(e => e.BatchId.ToString() == batchid);
            if (batch == null)
            {
                return NotFound();
            }

            Employee emp = db.Employees.FirstOrDefault(e => e.CURP == batchInfo.EmployeeCURP);
            if (emp == null)
            {
                return NotFound();
            }
            // create doc record for employee
            Document doc = new Document();
            doc.BatchId = int.Parse(batchid);
            doc.EmployeeId = emp.EmployeeId;
            doc.SignStatus = SignStatus.Unsigned;
            doc.PathToSignatureFile = batch.WorkDirectory;
            doc.PathToFile = Path.Combine(httpRootpath + batch.WorkDirectory);
            doc.PayperiodDate = DateTime.Now;
            doc.UploadTime = DateTime.Now;
            doc.SignatureFileHash = "not sure";
            db.Documents.Add(doc);
            db.SaveChanges();

            try
            {
                // write file to working directory for company
                SaveByteArrayAsImage(Path.Combine(doc.PathToFile, doc.DocumentId + ".jpg"), batchInfo.Content);
            }
            catch(Exception ex)
            {
                // what to do if file write fails
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        private void SaveByteArrayAsImage(string fullOutputPath, string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);

            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
                image.Save(fullOutputPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
           
        }

        [HttpPost]
        [Route("closebatch/{batchid}")]
        public IHttpActionResult CloseBatch(string batchid)
        {
            Batch batch = db.Batches.FirstOrDefault(e => e.BatchId.ToString() == batchid);
            if (batch == null)
            {
                return NotFound();
            }

            batch.BatchCloseTime = DateTime.Now;
            db.SaveChanges();
            return Ok();
        }

/*
        [HttpGet]
        [Route("companies")]
        public IHttpActionResult GetCompanies()
        {
            IList<Company> Companys = new List<Company>();
            Companys.Add(new Company() { CompanyID = 1, DocStoragePath1 = @"C:\cdfi\acme\filestore1", DocStoragePath2 = @"C:\cdfi\acme\filestore2", CompanyRFC = "CAAI951203PR6" });
            Companys.Add(new Company() { CompanyID = 2, DocStoragePath1 = @"C:\cdfi\amazon\filestore1", DocStoragePath2 = @"C:\cdfi\amazon\filestore2", CompanyRFC = "CAAK8833774Z0" });
            Companys.Add(new Company() { CompanyID = 3, DocStoragePath1 = @"C:\cdfi\ibm\filestore1", DocStoragePath2 = @"C:\cdfi\ibm\filestore2", CompanyRFC = "CAAI776644PP1" });
            return Ok<IList<Company>>(Companys);
        }
*/
    }

}
