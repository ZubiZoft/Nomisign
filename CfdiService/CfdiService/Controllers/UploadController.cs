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
using CfdiService.Filters;

namespace CfdiService.Controllers
{
    [RoutePrefix("api/upload")]
    public class UploadController : ApiController
    {

        private ModelDbContext db = new ModelDbContext();
        private readonly string httpDomain = System.Configuration.ConfigurationManager.AppSettings["signingAppDomain"];
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [HttpPost]
        [Route("openbatch/{id}")]
        [Authorize(Roles = "ADMIN,UPLOADER")]
        [IdentityBasicAuthentication]
        public IHttpActionResult OpenBatch(int id, [FromBody] OpenBatch batchInfo)
        {
            Company company = db.Companies.Where(c => c.ApiKey.Equals(batchInfo.ApiKey)).FirstOrDefault();
            log.Error("Batcinfo APIkey: " + batchInfo.ApiKey);
            log.Error("Company APIkey: " + id);
            if (company == null)
            {
                log.Error("Error adding document: company not found: " + id);
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

            //string filePath1;
            //string filePath2;
            //string currVol1;
            //string currVol2;
            //GetVolumePaths(out currVol1, out filePath1, out currVol2, out filePath2, company.CompanyId.ToString());
            //CanWriteTo(filePath1);
            //CanWriteTo(filePath2);

            Batch batch = new Batch
            {
                Company = company,
                CompanyId = company.CompanyId,
                BatchOpenTime = DateTime.Now,
                BatchCloseTime = SqlDateTime.MinValue.Value,
                ItemCount = batchInfo.FileCount,
                WorkDirectory = Guid.NewGuid().ToString(),
                ActualItemCount = 0,
                BatchStatus = BatchStatus.Open,
                ApiKey = company.ApiKey,
            };

            company.SignatureBalance -= batchInfo.FileCount;
            db.Batches.Add(batch);
            db.SaveChanges();
            return Ok(new BatchResult(batch.BatchId, BatchResultCode.Ok));
        }

        [HttpPost]
        [Route("openbatch2/{id}")]
        
        public IHttpActionResult OpenBatch2(int id, [FromBody] OpenBatch batchInfo)
        {
            Company company = db.Companies.Where(c => c.ApiKey.Equals(batchInfo.ApiKey)).FirstOrDefault();
            log.Error("Batcinfo APIkey: " + batchInfo.ApiKey);
            log.Error("Company APIkey: " + id);
            if (company == null)
            {
                log.Error("Error adding document: company not found: " + id);
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

            //string filePath1;
            //string filePath2;
            //string currVol1;
            //string currVol2;
            //GetVolumePaths(out currVol1, out filePath1, out currVol2, out filePath2, company.CompanyId.ToString());
            //CanWriteTo(filePath1);
            //CanWriteTo(filePath2);

            Batch batch = new Batch
            {
                Company = company,
                CompanyId = company.CompanyId,
                BatchOpenTime = DateTime.Now,
                BatchCloseTime = SqlDateTime.MinValue.Value,
                ItemCount = batchInfo.FileCount,
                WorkDirectory = Guid.NewGuid().ToString(),
                ActualItemCount = 0,
                BatchStatus = BatchStatus.Open,
                ApiKey = company.ApiKey,
            };

            company.SignatureBalance -= batchInfo.FileCount;
            db.Batches.Add(batch);
            db.SaveChanges();
            return Ok(new BatchResult(batch.BatchId, BatchResultCode.Ok));
        }

        [HttpPost]
        [Route("addfile/{batchid}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult AddFile(int batchid, [FromBody] CfdiService.Shapes.FileUpload upload)
        {
            Batch batch = db.Batches.Find(batchid);
            Company company = db.Companies.Find(batch.CompanyId);
            if (batch == null)
            {
                log.Error("Error adding document: batch not found: " + batchid);
                return BadRequest();
            }
            if (batch.ItemCount == batch.ActualItemCount)
            {
                log.Error("Error adding document: canceling batch due to item count: " + batchid);
                CancelBatch(batch);
                return Ok(new BatchResult(batch.BatchId, BatchResultCode.Cancelled));
            }
            Document newDoc = new Model.Document
            {
                Batch = batch,
                UploadTime = DateTime.Now,
                SignStatus = SignStatus.SinFirma,
                PathToFile = Guid.NewGuid().ToString()
            };

            try
            {
                // this only applies to XML vis bulk uploader
                if (!string.IsNullOrEmpty(upload.XMLContent))
                {
                    EvaluateBulkUpload(upload, batch, newDoc, company);
                }
                else // this only applies to admin app uploads where no xml is supplied
                {
                    EvaluateAdminUpload(upload, batch, newDoc);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error adding document: verification failed", ex);
                // log exception
                return BadRequest();
            }

            SaveContent(upload, newDoc);

            // send notifications - if fail, log but dont return error code.
            try
            {
                // Send SMS alerting employee of new docs
                // send notifications
                string smsBody = String.Format(Strings.visitSiteTosignDocumentMessage, company.CompanyName, newDoc.PayperiodDate.ToString("dd/MM/yyyy"), httpDomain);
                SendEmail.SendEmailMessage(newDoc.Employee.EmailAddress, Strings.visitSiteTosignDocumentMessageEmailSubject, smsBody);
                if (null != newDoc.Employee.CellPhoneNumber || newDoc.Employee.CellPhoneNumber.Length > 5) // check for > 5 as i needed to default to 52. for bulk uploader created new employee
                {
                    //SendSMS.SendSMSMsg(newDoc.Employee.CellPhoneNumber, smsBody);
                    string res = "";
                    SendSMS.SendSMSQuiubo(smsBody, string.Format("+52{0}", newDoc.Employee.CellPhoneNumber), out res);
                }
            }
            catch (Exception ex)
            {
                log.Error("warning adding document: one or both notifications failed to send", ex);
            }
            finally
            { // commit to DB
                db.Documents.Add(newDoc);
                batch.ActualItemCount++;
                db.SaveChanges();
            }

            return Ok(new BatchResult(batch.BatchId, BatchResultCode.Ok, batch.ItemCount));
        }

        [HttpPost]
        [Route("addfileexe/{batchid}")]
        public IHttpActionResult AddFileExe(int batchid, [FromBody] CfdiService.Shapes.FileUpload upload)
        {
            Batch batch = db.Batches.Find(batchid);
            Company company = db.Companies.Find(batch.CompanyId);
            if (batch == null)
            {
                log.Error("Error adding document: batch not found: " + batchid);
                return BadRequest();
            }
            if (batch.ItemCount == batch.ActualItemCount)
            {
                log.Error("Error adding document: canceling batch due to item count: " + batchid);
                CancelBatch(batch);
                return Ok(new BatchResult(batch.BatchId, BatchResultCode.Cancelled));
            }
            Document newDoc = new Model.Document
            {
                Batch = batch,
                UploadTime = DateTime.Now,
                SignStatus = SignStatus.SinFirma,
                PathToFile = Guid.NewGuid().ToString()
            };

            try
            {
                // this only applies to XML vis bulk uploader
                if (!string.IsNullOrEmpty(upload.XMLContent))
                {
                    EvaluateBulkUpload(upload, batch, newDoc, company);
                }
                else // this only applies to admin app uploads where no xml is supplied
                {
                    EvaluateAdminUpload(upload, batch, newDoc);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error adding document: verification failed", ex);
                // log exception
                return BadRequest();
            }

            SaveContent(upload, newDoc);

            // send notifications - if fail, log but dont return error code.
            try
            {
                // Send SMS alerting employee of new docs
                // send notifications
                string smsBody = String.Format(Strings.visitSiteTosignDocumentMessage, company.CompanyName, newDoc.PayperiodDate.ToString("dd/MM/yyyy"), httpDomain);
                SendEmail.SendEmailMessage(newDoc.Employee.EmailAddress, Strings.visitSiteTosignDocumentMessageEmailSubject, smsBody);
                if (null != newDoc.Employee.CellPhoneNumber || newDoc.Employee.CellPhoneNumber.Length > 5) // check for > 5 as i needed to default to 52. for bulk uploader created new employee
                {
                    //SendSMS.SendSMSMsg(newDoc.Employee.CellPhoneNumber, smsBody);
                    string res = "";
                    SendSMS.SendSMSQuiubo(smsBody, string.Format("+52{0}", newDoc.Employee.CellPhoneNumber), out res);
                }
            }
            catch (Exception ex)
            {
                log.Error("warning adding document: one or both notifications failed to send", ex);
            }
            finally
            { // commit to DB
                db.Documents.Add(newDoc);
                batch.ActualItemCount++;
                db.SaveChanges();
            }

            return Ok(new BatchResult(batch.BatchId, BatchResultCode.Ok, batch.ItemCount));
        }

        [HttpGet]
        [Route("closebatch/{batchid}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult CloseBatch(int batchid)
        {
            Batch batch = db.Batches.Find(batchid);
            if (batch == null)
            {
                return BadRequest();
            }
            batch.BatchStatus = BatchStatus.Completed;
            batch.BatchCloseTime = DateTime.Now;

            // There can be a race condition when uploading via admin screen.  so removing this for now
            // not sure its needed.  USe case i guess is batch has 10 docs, but for some reason one fails to upload...
            //int licenseCorrection = batch.ItemCount - batch.ActualItemCount;
            //if (licenseCorrection > 0)
            //{
            //    batch.Company.SignatureBalance += licenseCorrection;
            //}
            db.SaveChanges();
            return Ok();
        }

        [HttpPost]
        [Route("addcompanyfile/{companyId}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult AddCompanyAgreementFile(string companyId, [FromBody] CfdiService.Shapes.FileUpload batchInfo)
        {
            Company company = db.Companies.FirstOrDefault(e => e.CompanyId.ToString() == companyId);
            if (company == null)
            {
                log.Error("Error adding company document: company not found");
                return NotFound();
            }

            try
            {
                // write file to working directory for company, and only send msg's if file write succeeds
                NomiFileAccess.WriteCompanyAgreementFile(company, batchInfo);
                // write filename to DB
                company.NewEmployeeDocument = batchInfo.FileName;
                company.NewEmployeeGetDoc = NewEmployeeGetDocType.AddDocument;
                db.SaveChanges();
            }
            catch (Exception dbex)
            {
                log.Error("Error adding company document: ", dbex);
                if (dbex.InnerException != null)
                {
                    if (dbex.InnerException.InnerException != null)
                    {
                        return BadRequest(dbex.InnerException.InnerException.Message);
                    }
                    else
                    {
                        return BadRequest(dbex.InnerException.Message);
                    }
                }
                else
                {
                    return BadRequest(dbex.Message);
                }
            }


            return Ok();
        }
        [HttpPost]
        [Route("uploadfilesfront/{companyId}")]
        [Authorize(Roles = "ADMIN,UPLOADER")]
        [IdentityBasicAuthentication]
        public IHttpActionResult UploadFilesFront(int companyId, [FromBody] List<FileUpload> flist)
        {
            Company company = db.Companies.Find(companyId);
            Batch batchn = new Batch
            {
                Company = company,
                CompanyId = company.CompanyId,
                BatchOpenTime = DateTime.Now,
                BatchCloseTime = SqlDateTime.MinValue.Value,
                ItemCount = flist.Count,
                WorkDirectory = Guid.NewGuid().ToString(),
                ActualItemCount = 0,
                BatchStatus = BatchStatus.Open,
                ApiKey = company.ApiKey,
            };

            company.SignatureBalance -= flist.Count;
            db.Batches.Add(batchn);
            db.SaveChanges();
            int BatchId = batchn.BatchId;
            foreach (FileUpload filetemp in flist)
            {
                log.Info("XMLContent: " + filetemp.XMLContent + "\n");
                log.Info("Filehash: " + filetemp.FileHash + "\n");

                if (string.IsNullOrEmpty(filetemp.XMLContent))
                {
                    continue;
                }
                byte[] content = Encoding.UTF8.GetBytes(filetemp.XMLContent);
                XElement root;
                using (MemoryStream ms = new MemoryStream(content))
                    root = XElement.Load(ms);

                XNamespace cfdi = "http://www.sat.gob.mx/cfd/3";
                XNamespace nomina12 = "http://www.sat.gob.mx/nomina12";

                XElement complementoelem = null;
                ElementCheckXMLTagValue(cfdi, "Complemento", root, ref complementoelem);
                XElement nominaSubcontracion = null;
                DescendantsCheckXMLTagValue(nomina12, "SubContratacion", complementoelem, ref nominaSubcontracion);
                XAttribute clientRfc = null;
                AttributeCheckXMLTagValue("RfcLabora", nominaSubcontracion, ref clientRfc);

                if (clientRfc != null)
                {
                    Client client = db.FindClientByRfc(clientRfc.Value);
                    if (client == null)
                        continue;
                }
                else
                {
                    continue;
                }

                //Checking for duplicate Receipt XML Hash
                //if (CheckifReceiptAlreadyExists(filetemp))
                //continue;

                if (!filetemp.XMLContent.Contains("<cfdi:Comprobante") || !filetemp.XMLContent.Contains("<nomina12:Nomina") || string.IsNullOrEmpty(filetemp.PDFContent)) { continue; }
                Batch batch = db.Batches.Find(BatchId);
                if (batch == null)
                {
                    log.Error("Error adding document: batch not found: " + BatchId);
                    return BadRequest();
                }
                /*if (batch.ItemCount == batch.ActualItemCount)
                {
                    log.Error("Error adding document: canceling batch due to item count: " + BatchId);
                    CancelBatch(batch);
                    return Ok(new BatchResult(batch.BatchId, BatchResultCode.Cancelled));
                }*/
                Document newDoc = new Model.Document
                {
                    Batch = batch,
                    UploadTime = DateTime.Now,
                    SignStatus = SignStatus.SinFirma,
                    PathToFile = Guid.NewGuid().ToString(),
                    CompanyId = company.CompanyId
                };

                try
                {
                    // this only applies to XML vis bulk uploader
                    if (!string.IsNullOrEmpty(filetemp.XMLContent))
                    {
                        EvaluateBulkUpload(filetemp, batch, newDoc, company);
                    }
                    else // this only applies to admin app uploads where no xml is supplied
                    {
                        EvaluateAdminUpload(filetemp, batch, newDoc);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error adding document: verification failed", ex);
                    // log exception
                    return BadRequest();
                }

                SaveContent(filetemp, newDoc);

                db.Documents.Add(newDoc);
                batch.ActualItemCount++;
                db.SaveChanges();

                var empDoc = db.Employees.Find(newDoc.EmployeeId);
                log.Info("ID Emp: " + newDoc.EmployeeId.ToString());

                // send notifications - if fail, log but dont return error code.
                try
                {
                    // Send SMS alerting employee of new docs
                    // send notifications
                    string emailBody = String.Format(Strings.visitSiteTosignDocumentMessage, httpDomain, newDoc.Employee.Company.CompanyName, newDoc.PayperiodDate.ToString("dd/MM/yyyy"));
                    SendEmail.SendEmailMessage(empDoc.EmailAddress, Strings.visitSiteTosignDocumentMessageEmailSubject, emailBody);
                    if (null != newDoc.Employee.CellPhoneNumber || newDoc.Employee.CellPhoneNumber.Length > 5) // check for > 5 as i needed to default to 52. for bulk uploader created new employee
                    {
                        //SendSMS.SendSMSMsg(newDoc.Employee.CellPhoneNumber, smsBody);
                        string res = "";
                        var smsBody = String.Format(Strings.visitSiteTosignDocumentSMS, newDoc.Employee.Company.CompanyName, newDoc.PayperiodDate.ToString("dd/MM/yyyy"), httpDomain);
                        SendSMS.SendSMSQuiubo(smsBody, string.Format("+52{0}", newDoc.Employee.CellPhoneNumber), out res);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("warning adding document: one or both notifications failed to send", ex);
                    log.Error(ex.Message);
                    log.Error(ex.Source);
                    log.Error(ex.StackTrace);
                }
            }
            return Ok();
        }
        private bool CheckifReceiptAlreadyExists(FileUpload filetemp)
        {
            var docListResult = db.Documents.Where(x => x.FileHash == filetemp.FileHash).ToList();
            if (docListResult.Count > 0)
                return true;
            else
                return false;
        }
        private void CancelBatch(Batch batch)
        {

        }
        private void EvaluateAdminUpload(FileUpload upload, Batch batch, Document doc)
        {
            Employee emp = db.Employees.FirstOrDefault(e => e.CURP == upload.EmployeeCURP && e.CompanyId == batch.CompanyId);
            if (emp == null)
            {
                log.Error("Error adding document: employee not found: " + emp.EmployeeId);
                throw new Exception("Employee not found");
            }
            doc.EmployeeId = emp.EmployeeId;
            doc.Employee = emp;

            Client client = null;
            // TODO: will need to deal with RFC and non RFC doc storage - i think!!
            if (!String.IsNullOrEmpty(emp.RFC))
            {
                client = db.Clients.FirstOrDefault(e => e.ClientCompanyRFC.ToString() == emp.RFC);
                if (client == null)
                {
                    log.Error("Failed adding client document: client not found: " + emp.RFC);
                    // no error, may not belog to a client
                    // throw new Exception("Client not found");
                }
                else
                {
                    doc.ClientCompanyId = client.ClientCompanyID;
                }
            }
            // on admin upload, we get this from batch
            doc.CompanyId = batch.CompanyId;

            // dont have this data on admin upload
            doc.PayperiodDate = DateTime.Now;
        }
        private void EvaluateBulkUpload(FileUpload upload, Batch batch, Document doc, Company company)
        {
            // No need for this if only a PDF doc is uploadded by admin screen
            if (string.IsNullOrEmpty(upload.XMLContent))
            {
                return;
            }
            byte[] content = Encoding.UTF8.GetBytes(upload.XMLContent);
            ValidateContentHash(content, upload.FileHash);
            XElement root;
            using (MemoryStream ms = new MemoryStream(content))
                root = XElement.Load(ms);

            XNamespace cfdi = "http://www.sat.gob.mx/cfd/3";
            XNamespace nomina12 = "http://www.sat.gob.mx/nomina12";

            //XElement elem = root.Element(cfdi + "Emisor");
            XElement elem = null;
            ElementCheckXMLTagValue(cfdi, "Emisor", root, ref elem);
            XAttribute emisorRfc = null;
            AttributeCheckXMLTagValue("rfc", elem, ref emisorRfc);
            XAttribute emisorName = null;
            AttributeCheckXMLTagValue("Nombre", elem, ref emisorName);
            XAttribute emisorRegimenFiscal = null;
            AttributeCheckXMLTagValue("RegimenFiscal", elem, ref emisorRegimenFiscal);
            XElement conceptoselem = null;
            ElementCheckXMLTagValue(cfdi, "Conceptos", root, ref conceptoselem);
            XElement conceptoelem = null;
            DescendantsCheckXMLTagValue(cfdi, "Concepto", conceptoselem, ref conceptoelem);
            XAttribute payAmount = null;
            AttributeCheckXMLTagValue("importe", conceptoelem, ref payAmount);
            XElement Receptorelem = null;
            ElementCheckXMLTagValue(cfdi, "Receptor", root, ref Receptorelem);
            XAttribute receptorRfc = null;
            AttributeCheckXMLTagValue("rfc", Receptorelem, ref receptorRfc);
            XAttribute receptorfullName = null;
            AttributeCheckXMLTagValue("nombre", Receptorelem, ref receptorfullName);
            XElement complementoelem = null;
            ElementCheckXMLTagValue(cfdi, "Complemento", root, ref complementoelem);
            XElement nomina = null;
            DescendantsCheckXMLTagValue(nomina12, "Nomina", complementoelem, ref nomina);
            XAttribute payPeriod = null;
            AttributeCheckXMLTagValue("FechaFinalPago", nomina, ref payPeriod);
            XElement nominaReceptor = null;
            DescendantsCheckXMLTagValue(nomina12, "Receptor", complementoelem, ref nominaReceptor);
            XAttribute receptorCurp = null;
            AttributeCheckXMLTagValue("Curp", nominaReceptor, ref receptorCurp);
            // try to get client RFC - may not be one
            XElement nominaSubcontracion = null;
            DescendantsCheckXMLTagValue(nomina12, "SubContratacion", complementoelem, ref nominaSubcontracion);
            XAttribute clientRfc = null;
            AttributeCheckXMLTagValue("RfcLabora", nominaSubcontracion, ref clientRfc);

            /* //XElement elem = root.Element(cfdi + "Emisor");
             XElement elem = ElementCheckXMLTagValue(cfdi.NamespaceName, "Emisor", root);
            //XAttribute emisorRfc = elem.Attribute("rfc");
            XAttribute emisorRfc = AttributeCheckXMLTagValue("rfc", elem);
            //elem = root.Element(cfdi + "Conceptos");
            elem = ElementCheckXMLTagValue(cfdi.NamespaceName, "Conceptos", root);
            //elem = elem.Descendants(cfdi + "Concepto").First();
            elem = DescendantsCheckXMLTagValue(cfdi.NamespaceName, "Concepto", root);
            //XAttribute payAmount = elem.Attribute("importe");
            XAttribute payAmount = AttributeCheckXMLTagValue("importe", elem);
            //elem = root.Element(cfdi + "Receptor");
            elem = ElementCheckXMLTagValue(cfdi.NamespaceName, "Receptor", root);
            //XAttribute receptorRfc = elem.Attribute("rfc");
            XAttribute receptorRfc = AttributeCheckXMLTagValue("rfc", elem);
            //XAttribute fullName = elem.Attribute("nombre");
            XAttribute fullName = AttributeCheckXMLTagValue("nombre", elem);
            //elem = root.Descendants(nomina12 + "Nomina").First();
            elem = DescendantsCheckXMLTagValue(nomina12.NamespaceName, "Nomina", root);
            //XAttribute payPeriod = elem.Attribute("FechaFinalPago");
            XAttribute payPeriod = AttributeCheckXMLTagValue("FechaFinalPago", elem);
            //elem = elem.Descendants(nomina12 + "Receptor").First();
            elem = DescendantsCheckXMLTagValue(nomina12.NamespaceName, "Receptor", root);
            //XAttribute receptorCurp = elem.Attribute("Curp");
            XAttribute receptorCurp = AttributeCheckXMLTagValue("Curp", elem);
            XAttribute clientRfc = null;

            // try to get client RFC - may not be one
            try
            {
                //elem = root.Element(cfdi + "Complemento");
                elem = ElementCheckXMLTagValue(cfdi.NamespaceName, "Complemento", root);
                //elem = elem.Descendants(nomina12 + "Nomina").First();
                elem = DescendantsCheckXMLTagValue(nomina12.NamespaceName, "Nomina", elem);
                //elem = elem.Descendants(nomina12 + "Receptor").First();
                elem = DescendantsCheckXMLTagValue(nomina12.NamespaceName, "Receptor", elem);
                //elem = elem.Descendants(nomina12 + "SubContratacion").First();
                elem = DescendantsCheckXMLTagValue(nomina12.NamespaceName, "SubContratacion", elem);
                //clientRfc = elem.Attribute("RfcLabora");
                clientRfc = AttributeCheckXMLTagValue("RfcLabora", elem);
            }
            catch (Exception ex)
            { log.Error("Error adding document: bulk verification failed", ex); }*/
            if (emisorRfc == null)
                log.Info("emisorRfc is NULL");
            if (receptorRfc == null)
                log.Info("receptorRfc is NULL");
            if (receptorCurp == null)
                log.Info("receptorCurp is NULL");
            if (payPeriod == null)
                log.Info("payPeriod is NULL");

            if (emisorRfc == null || receptorRfc == null || receptorCurp == null || payPeriod == null)
                throw new ApplicationException("Invalid XML format");

            if (clientRfc != null)
            {
                Client client = db.FindClientByRfc(clientRfc.Value);
                if (client != null)
                    doc.ClientCompanyId = client.ClientCompanyID;
            }

            Employee emp = db.FindEmployeeByCURPCompany(company.CompanyId, (string)receptorCurp);
            if (emp == null)
            {
                // create employee setting full name
                // employee status is provisional (to be completed at a later time)
                // throw new ApplicationException("Cannot find employee RFC with ID: " + receptorRfc);
                log.Info("Created By: " + User.Identity.GetName());
                emp = new Employee
                {
                    RFC = (string)receptorRfc,
                    FullName = (string)receptorfullName,
                    CURP = (string)receptorCurp,
                    CreatedDate = DateTime.Now,
                    LastLoginDate = SqlDateTime.MinValue.Value,
                    EmployeeStatus = EmployeeStatusType.Unverified,
                    CreatedByUserId = int.Parse(User.Identity.GetName()),
                    CellPhoneNumber = ""
                };
                emp.Company = batch.Company;

                try
                {
                    var splittedName = FullNameSplitterFromRFCService.SplitName((string)receptorRfc, (string)receptorfullName);
                    if (splittedName != null)
                    {
                        emp.FirstName = splittedName[0];
                        emp.LastName1 = splittedName[1];
                        emp.LastName2 = splittedName[2];
                    }
                }
                catch (Exception ex)
                {
                    log.Info("Error splitting the Full Name", ex);
                }

                db.Employees.Add(emp);

                // add company default doc
                if (emp.Company.NewEmployeeGetDoc == NewEmployeeGetDocType.AddDocument)
                {
                    batch.ActualItemCount++;
                    string fileName = NomiFileAccess.CopyCompanyAgreementFileForEmployee(emp.CompanyId,
                       batch.WorkDirectory,  //file name to write
                       emp.Company.NewEmployeeDocument.Trim()); // trim only needed due to DB scheme issue

                    if (!String.IsNullOrEmpty(fileName))
                    {
                        // TODO: Write file to DB after copied to disk
                        Document document = new Document()
                        {
                            AlwaysShow = 1,
                            BatchId = batch.BatchId,
                            CompanyId = emp.CompanyId,
                            EmployeeId = emp.EmployeeId,
                            PathToFile = fileName,
                            PayperiodDate = DateTime.Now,
                            SignStatus = SignStatus.SinFirma,
                            UploadTime = DateTime.Now,
                            ClientCompanyId = doc.ClientCompanyId
                        };
                        db.Documents.Add(document);
                    }
                }
                log.Error("warning adding document: employee not found for CURP " + (string)receptorCurp);
            }

            log.Info("Emisor: " + emisorRfc);
            log.Info("Receptor: " + receptorRfc);
            log.Info("Company: " + emp.Company.CompanyRFC);
            //Changed emp.Company.CompanyRFC to be current company since one employee can be in several companies.
            if (company.CompanyRFC != emisorRfc.Value)
                throw new ApplicationException("Company RFC with ID: " + company.CompanyRFC + " does not match Company RFC in xml: " + emisorRfc);

            if (clientRfc != null)
            {
                Client client = db.FindClientByRfc(clientRfc.Value);
                if (client != null)
                    doc.ClientCompanyId = client.ClientCompanyID;
            }

            // VALIDATE CURP?
            doc.Employee = emp;
            doc.EmployeeId = emp.EmployeeId;
            doc.CompanyId = emp.Company.CompanyId;
            doc.PayAmount = Decimal.Parse(payAmount.Value);
            doc.FileHash = upload.FileHash;
            doc.PayperiodDate = DateTime.ParseExact((string)payPeriod, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        }

        private void AttributeCheckXMLTagValue(string tag, XElement elem, ref XAttribute element)
        {
            try { element = elem.Attribute(tag); } catch { }
            if (element == null)
            { try { element = elem.Attribute(FirstCharToUpper(tag)); } catch { } }
            if (element == null)
            { try { element = elem.Attribute(FirstCharToLower(tag)); } catch { } }
            if (element == null)
            { try { element = elem.Attribute(tag.ToLower()); } catch { } }
            if (element == null)
            { try { element = elem.Attribute(tag.ToUpper()); } catch { } }
        }

        private void DescendantsCheckXMLTagValue(XNamespace cfdi, string tag, XElement root, ref XElement elem)
        {
            try { elem = root.Descendants(cfdi + tag).First(); } catch { }
            if (elem == null)
            { try { elem = root.Descendants(cfdi + FirstCharToUpper(tag)).First(); } catch { } }
            if (elem == null)
            { try { elem = root.Descendants(cfdi + FirstCharToLower(tag)).First(); } catch { } }
            if (elem == null)
            { try { elem = root.Descendants(cfdi + tag.ToLower()).First(); } catch { } }
            if (elem == null)
            { try { elem = root.Descendants(cfdi + tag.ToUpper()).First(); } catch { } }
        }

        private void ElementCheckXMLTagValue(XNamespace cfdi, string tag, XElement root, ref XElement elem2)
        {
            try { elem2 = root.Element(cfdi + tag); } catch { }
            if (elem2 == null)
            { try { elem2 = root.Element(cfdi + FirstCharToUpper(tag)); } catch { } }
            if (elem2 == null)
            { try { elem2 = root.Element(cfdi + FirstCharToLower(tag)); } catch { } }
            if (elem2 == null)
            { try { elem2 = root.Element(cfdi + tag.ToLower()); } catch { } }
            if (elem2 == null)
            { try { elem2 = root.Element(cfdi + tag.ToUpper()); } catch { } }
        }

        public static string FirstCharToLower(string input)
        {
            switch (input)
            {
                case null: return "";
                case "": return "";
                default: return input.First().ToString().ToLower() + input.Substring(1);
            }
        }

        public static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: return "";
                case "": return "";
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
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

        private void SaveContent(FileUpload upload, Document doc)
        {
            // Admin screen upload does not include Xml
            if (!string.IsNullOrEmpty(upload.XMLContent)) { NomiFileAccess.WriteFile(doc, upload.XMLContent, Strings.XML_EXT); }
            //NomiFileAccess.WriteEncodedFile(doc, upload.PDFContent, Strings.PDF_EXT);
            var company = db.Companies.Find(doc.CompanyId);
            NomiFileAccess.WriteEncodedFileAttachedXML(doc, upload.PDFContent, Strings.PDF_EXT, upload.XMLContent, company);
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
