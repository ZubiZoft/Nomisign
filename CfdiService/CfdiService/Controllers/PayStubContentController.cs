using CfdiService.Model;
using CfdiService.Services;
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
        private readonly string httpDomain = System.Configuration.ConfigurationManager.AppSettings["signingAppDomain"];

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
            batch.WorkDirectory = Guid.NewGuid().ToString(); // has hyphyns but no {}

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
            try
            {
                doc.BatchId = int.Parse(batchid);
                doc.Batch = batch;
                doc.EmployeeId = emp.EmployeeId;
                doc.CompanyId = batch.CompanyId;
                doc.SignStatus = SignStatus.Unsigned;
                doc.PathToSignatureFile = Guid.NewGuid().ToString(); // has hyphyns but no {}
                doc.PathToFile = Guid.NewGuid().ToString(); // has hyphyns but no {}
                doc.PayperiodDate = DateTime.Now;
                doc.UploadTime = DateTime.Now;
                doc.SignatureFileHash = "not sure";
                db.Documents.Add(doc);
                db.SaveChanges();
            }
            catch (Exception dbex)
            {
                return BadRequest(dbex.Message);
            }

            // write file to Disk
            // TODO: remove JPG and only support PDF's
            // write file to working directory for company, and only send msg's if file write succeeds
            if (NomiFileAccess.WriteFile(doc, batchInfo.Content))
            {
                // send notifications
                string smsBody = String.Format("Please visit nomisign site for review of new docs, http://{0}/nomisign", httpDomain);
                SendEmail.SendEmailMessage(emp.EmailAddress, "review your new docs", smsBody);
                if (null != emp.CellPhoneNumber)
                {
                    SendSMS.SendSMSMsg(emp.CellPhoneNumber, smsBody);
                }
            }
            else
            {
                // should i delete doc table row, add error writing to file column, what??
                return BadRequest("Error Writing file to disk!");
            }

            return Ok();
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
    }

}
