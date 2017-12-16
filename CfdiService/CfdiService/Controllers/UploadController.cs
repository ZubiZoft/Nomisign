using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CfdiService.Shapes;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using CfdiService.Services;

namespace CfdiService.Controllers
{
    [RoutePrefix("api/upload")]
    public class UploadController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();

        [HttpPost]
        [Route("openbatch/{id}")]
        public IHttpActionResult OpenBatch(int id, [FromBody] OpenBatch batchInfo)
        {
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return BadRequest();
            }
            if (company.ApiKey != batchInfo.ApiKey)
            {
                return BadRequest();
            }
            if (company.AccountStatus != AccountStatusType.Active)
            {
                return Ok(new BatchResult(0, BatchResultCode.AccountStatus));
            }
            if (company.SignatureBalance < batchInfo.FileCount)
            {
                return Ok(new BatchResult(0, BatchResultCode.LicenseBalance));
            }

            // look to see if there is an open batch by this company and abort it if so

            string filePath1;
            string filePath2;
            string currVol1;
            string currVol2;
            GetVolumePaths(out currVol1, out filePath1, out currVol2, out filePath2, company.CompanyId.ToString());
            CanWriteTo(filePath1);
            CanWriteTo(filePath2);

            Batch batch = new Batch
            {
                Company = company,
                BatchOpenTime = DateTime.UtcNow,
                BatchCloseTime = SqlDateTime.MinValue.Value,
                ItemCount = batchInfo.FileCount,
                ActualItemCount = 0,
                BatchStatus = BatchStatus.Open,
            };

            company.SignatureBalance -= batchInfo.FileCount;
            db.Batches.Add(batch);
            db.SaveChanges();
            return Ok(new BatchResult(batch.BatchId, BatchResultCode.Ok));
        }

        [HttpPost]
        [Route("addfile/{batchid}")]
        public IHttpActionResult AddFile(int batchid, [FromBody] CfdiService.Shapes.FileUpload upload)
        {
            Batch batch = db.Batches.Find(batchid);
            if (batch == null)
            {
                return BadRequest();
            }
            if (batch.ItemCount == batch.ActualItemCount)
            {
                CancelBatch(batch);
                return Ok(new BatchResult(batch.BatchId, BatchResultCode.Cancelled));
            }
            Document newDoc = new Model.Document
            {
                Batch = batch,
                UploadTime = DateTime.UtcNow,
                SignStatus = SignStatus.SinFirma,
                PathToFile = Guid.NewGuid().ToString()
            };

            try
            {
                EvaluateUpload(upload, batch, newDoc);
            }
            catch (Exception ex)
            {
                // log exception
                return BadRequest();
            }

            SaveContent(upload, batch, newDoc);
            db.Documents.Add(newDoc);
            batch.ActualItemCount++;
            db.SaveChanges();
            // update order to allow for teh document id to be the base filename

            return Ok(new BatchResult(batch.BatchId, BatchResultCode.Ok, batch.ItemCount));
        }

        [HttpGet]
        [Route("closebatch/{batchid}")]
        public IHttpActionResult CloseBatch(int batchid)
        {
            Batch batch = db.Batches.Find(batchid);
            if (batch == null)
            {
                return BadRequest();
            }
            batch.BatchStatus = BatchStatus.Completed;
            batch.BatchCloseTime = DateTime.UtcNow;

            int licenseCorrection = batch.ItemCount - batch.ActualItemCount;
            if (licenseCorrection > 0)
            {
                batch.Company.SignatureBalance += licenseCorrection;
            }
            db.SaveChanges();
            return Ok();
        }


        private void CancelBatch(Batch batch)
        {

        }

        private void EvaluateUpload(FileUpload upload, Batch batch, Document doc)
        {
            byte[] content = Encoding.UTF8.GetBytes(upload.XMLContent);
            ValidateContentHash(content, upload.FileHash);
            XElement root;
            using (MemoryStream ms = new MemoryStream(content))
                root = XElement.Load(ms);

            XNamespace cfdi = "http://www.sat.gob.mx/cfd/3";
            XNamespace nomina12 = "http://www.sat.gob.mx/nomina12";
            XElement elem = root.Element(cfdi + "Emisor");
            XAttribute emisorRfc = elem.Attribute("rfc");
            elem = root.Element(cfdi + "Conceptos");
            elem = elem.Descendants(cfdi + "Concepto").First();
            XAttribute payAmount = elem.Attribute("importe");
            elem = root.Element(cfdi + "Receptor");
            XAttribute receptorRfc = elem.Attribute("rfc");
            XAttribute fullName = elem.Attribute("nombre");
            elem = root.Descendants(nomina12 + "Nomina").First();
            XAttribute payPeriod = elem.Attribute("FechaFinalPago");
            elem = elem.Descendants(nomina12 + "Receptor").First();
            XAttribute receptorCurp = elem.Attribute("Curp");
            XAttribute clientRfc = null;
            try
            {
                elem = root.Element(cfdi + "Complemento");
                elem = elem.Descendants(nomina12 + "Nomina").First();
                elem = elem.Descendants(nomina12 + "Receptor").First();
                elem = elem.Descendants(nomina12 + "SubContratacion").First();
                clientRfc = elem.Attribute("rfcLabora");
            }
            catch (Exception)
            { }

            if (emisorRfc == null || receptorRfc == null || receptorCurp == null || payPeriod == null)
                throw new ApplicationException("Invalid XML format");

            Employee emp = db.FindEmployeeByRfc((string)receptorRfc);
            if (emp == null)
            {
                // create employee setting full name
                // employee status is provisional (to be completed at a later time)
                // throw new ApplicationException("Cannot find employee RFC with ID: " + receptorRfc);
                emp = new Employee
                {
                    RFC = (string)receptorRfc,
                    FullName = (string)fullName,
                    CURP = (string)receptorCurp
                };
                emp.Company = batch.Company;
                db.Employees.Add(emp);
            }
                

            if (emp.Company.CompanyRFC != emisorRfc.Value)
                throw new ApplicationException("Employee RFC with ID: " + receptorRfc + " does not match Company RFC: " + emisorRfc);

            if (clientRfc != null)
            {
                Client client = db.FindClientByRfc(clientRfc.Value);
                if (client != null)
                    doc.ClientCompanyId = client.ClientCompanyID;
            }
            
            // VALIDATE CURP?
            doc.Employee = emp;
            doc.PayAmount = Decimal.Parse(payAmount.Value);
            doc.FileHash = upload.FileHash;
            doc.PayperiodDate = DateTime.ParseExact((string)payPeriod, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private void ValidateContentHash(byte[] content, string hash)
        {
            SHA256 mySHA256 = SHA256Managed.Create();
            MemoryStream ms = new MemoryStream(content);
            byte[] hashValue = mySHA256.ComputeHash(ms);
            var stringBuilder = new StringBuilder();
            foreach (byte b in hashValue)
                stringBuilder.AppendFormat("{0:x2}", b);

            string testHash = stringBuilder.ToString();
            if (hash != testHash)
                throw new ApplicationException("Content hash does not match upload");
        }

        private bool ValidateDirectory(string dir)
        {
            return Directory.GetFileSystemEntries(dir).Length > 1;
        }

        private void SaveContent(FileUpload upload, Batch batch, Document doc)
        {
            NomiFileAccess.WriteFile(doc, upload.XMLContent, ".xml");
            NomiFileAccess.WriteEncodedFile(doc, upload.PDFContent, ".pdf");
        }

        private void GetVolumePaths(out string vol1, out string path1, out string vol2, out string path2, string companyDir)
        {
            vol1 = ConfigurationManager.AppSettings["CurrentVolume-1"];
            string curr1 = ConfigurationManager.AppSettings[vol1];
            vol2 = ConfigurationManager.AppSettings["CurrentVolume-2"];
            string curr2 = ConfigurationManager.AppSettings[vol2];
            path1 = Path.Combine(curr1, companyDir);
            path2 = Path.Combine(curr2, companyDir);
        }

        private void GetVolumePaths(out string vol1, out string vol2)
        {
            vol1 = ConfigurationManager.AppSettings["CurrentVolume-1"];
            vol2 = ConfigurationManager.AppSettings["CurrentVolume-2"];
        }


        private bool CanWriteTo(string dir)
        {
            return true;
        }
    }

}
