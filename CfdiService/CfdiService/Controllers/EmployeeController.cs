using CfdiService.Model;
using CfdiService.Services;
using CfdiService.Shapes;
using CfdiService.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CfdiService.Controllers
{
    [RoutePrefix("api")]
    public class EmployeeController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string httpDomain = System.Configuration.ConfigurationManager.AppSettings["signingAppDomain"];

        // GET: api/employees
        [HttpGet]
        [Route("employees/{cid}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetCompanyEmployees(int cid)
        {
            var result = new List<EmployeeListShape>();
            var company = db.Companies.Find(cid);
            if (company == null)
                return Ok(result);
            foreach (var c in db.Employees)
            {
                if (c.CompanyId == cid)
                {
                    result.Add(EmployeeListShape.FromDataModel(c, Request, company.CompanyName));
                }
            }

            log.Info("Looking for employees for cid: " + cid);
            return Ok(result);
        }

        // GET: api/employees
        [HttpGet]
        [Route("employees/{cid}/new")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetCompanyNewEmployees(int cid)
        {
            var result = new List<EmployeeListShape>();
            var employeeResult = db.Employees.Where(x => x.CompanyId == cid
                    && (x.EmailAddress == null || x.EmailAddress.Equals(""))
                    && (x.CellPhoneNumber == null || x.CellPhoneNumber.Equals(""))).ToList();
            var company = db.Companies.Find(cid);
            if (company == null)
                return Ok(result);
            foreach (var c in employeeResult)
            {
                result.Add(EmployeeListShape.FromDataModel(c, Request, company.CompanyName));
            }

            log.Info("Looking for employees that doesn't have email and phone for cid: " + cid);
            return Ok(result);
        }

        // GET: api/employees
        [HttpGet]
        [Route("employees/{cid}/inactive")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetCompanyDisableEmployees(int cid)
        {
            var result = new List<EmployeeListShape>();
            try
            {
                var company = db.Companies.Find(cid);
                if (company == null)
                    return Ok(result);

                var employeesIds = db.Documents.Where(d => d.CompanyId == cid).GroupBy(d => d.EmployeeId)
                    .Select(doc => new
                    {
                        EmployeeId = doc.Key,
                        LastDate = doc.Max(d => d.PayperiodDate)
                    }).ToList();

                var auxDate = DateTime.Now.AddMonths(-3);
                employeesIds = employeesIds.Where(d => d.LastDate.CompareTo(auxDate) <= 0).ToList();

                foreach (var empId in employeesIds)
                {
                    result.Add(EmployeeListShape.FromDataModel(db.Employees.Find(empId.EmployeeId), Request, company.CompanyName));
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                log.Error(ex.Message);
                log.Error(ex.Source);
                log.Error(ex.StackTrace);
            }
            return Ok(result);
        }

        // GET: api/companyusers/5
        [HttpGet]
        [Route("employees/{cid}/{id}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetCompanyEmployee(int id)
        {
            // not validating company ID here
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }

            log.Info("Getting employee data for id: " + id);
            return Ok(EmployeeShape.FromDataModel(employee, Request));
        }

        // GET: api/employee/5
        [HttpGet]
        [Route("employee/{id}")]
        [Authorize(Roles = "EMPLOYEE,ADMIN,CLIENT")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetEmployee(int id)
        {
            // not validating company ID here
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }

            var employeeShape = EmployeeShape.FromDataModel(employee, Request);

            // hack till i figure out EF
            var createdByUser = db.Users.Find(employee.CreatedByUserId);
            if (createdByUser != null)
            {
                employeeShape.CreatedByUserName = createdByUser.DisplayName;
            }
            return Ok(employeeShape);
        }

        [HttpGet]
        [Route("employeeexist/{id}")]
        public IHttpActionResult GetEmployeeExist(int id)
        {
            // not validating company ID here
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }

            var employeeShape = EmployeeShape.FromDataModel(employee, Request);

            // hack till i figure out EF
            var createdByUser = db.Users.Find(employee.CreatedByUserId);
            if (createdByUser != null)
            {
                employeeShape.CreatedByUserName = createdByUser.DisplayName;
            }
            return Ok(employeeShape);
        }

        [HttpPut]
        [Route("employees/passwordsession/{id}")]
        [Authorize(Roles = "EMPLOYEE")]
        [IdentityBasicAuthentication]
        public IHttpActionResult UpdateEmployeePasswordSession(int id, EmployeeShape employeeShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != employeeShape.EmployeeId)
            {
                return BadRequest();
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            // transform to data model
            EmployeeShape.ToDataModel(employeeShape, employee);

            // Get security codes
            EmployeesCode codes = db.EmployeeSecurityCodes.Find(employee.EmployeeId);

            // IF this is a password reset update, verify code.  dont make changes if not, unless unverified.
            // TODO: add time expiration to vcode
            if (employee.EmployeeStatus != EmployeeStatusType.Active && employeeShape.SecurityCode != codes.Vcode)
            {
                return BadRequest();
            }
            else
            {
                // password is not set on initial employee creation
                if (!String.IsNullOrEmpty(employeeShape.PasswordHash))
                {
                    employee.PasswordHash = EncryptionService.Sha256_hash(employeeShape.PasswordHash, codes.Prefix);
                    var emps = db.Employees.Where(e => e.CURP == employee.CURP).ToList();
                    foreach (var e in emps)
                    {
                        e.PasswordHash = EncryptionService.Sha256_hash(employeeShape.PasswordHash, codes.Prefix);
                    }
                    db.SaveChanges();
                }
                codes.Vcode = string.Empty;
            }
            db.SaveChanges();
            return Ok(employeeShape);
        }

        [HttpPut]
        [Route("employees/password/{id}")]
        public IHttpActionResult UpdateEmployeePassword(int id, EmployeeShape employeeShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != employeeShape.EmployeeId)
            {
                return BadRequest();
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            // transform to data model
            EmployeeShape.ToDataModel(employeeShape, employee);

            // Get security codes
            EmployeesCode codes = db.EmployeeSecurityCodes.Find(employee.EmployeeId);

            // IF this is a password reset update, verify code.  dont make changes if not, unless unverified.
            // TODO: add time expiration to vcode
            if (employee.EmployeeStatus != EmployeeStatusType.Active && employeeShape.SecurityCode != codes.Vcode)
            {
                return BadRequest();
            }
            else
            {
                // password is not set on initial employee creation
                if (!String.IsNullOrEmpty(employeeShape.PasswordHash))
                {
                    employee.PasswordHash = EncryptionService.Sha256_hash(employeeShape.PasswordHash, codes.Prefix);
                    var emps = db.Employees.Where(e => e.CURP == employee.CURP).ToList();
                    foreach (var e in emps)
                    {
                        e.PasswordHash = EncryptionService.Sha256_hash(employeeShape.PasswordHash, codes.Prefix);
                        e.EmployeeStatus = EmployeeStatusType.Active;
                    }
                    employee.FirstName = employeeShape.FirstName;
                    employee.LastName1 = employeeShape.LastName1;
                    employee.LastName2 = employeeShape.LastName2;
                    db.SaveChanges();
                }
                codes.Vcode = string.Empty;
                //db.SaveChanges(); redundant
            }
            db.SaveChanges();

            db.CreateLog(OperationTypes.EmployeeAccountActivated, string.Format("Cuenta de empleado activada {0}", employee.EmployeeId), User, 
                    employee.EmployeeId, ObjectTypes.Employee);

            return Ok(employeeShape);
        }

        [HttpPut]
        [Route("employees/phone/{id}")]
        [Authorize(Roles = "EMPLOYEE")]
        [IdentityBasicAuthentication]
        public IHttpActionResult UpdateEmployeePhone(int id, EmployeeShape employeeShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != employeeShape.EmployeeId)
            {
                return BadRequest();
            }
            log.Info("New Employee Phone : " + employeeShape.CellPhoneNumber);
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            var emps = db.Employees.Where(e => e.CURP == employee.CURP).ToList();
            foreach (var e in emps)
            {
                // transform to data model
                e.CellPhoneNumber = employee.CellPhoneNumber;
                db.Entry(e).State = System.Data.Entity.EntityState.Modified;
            }
            db.SaveChanges();
            return Ok(employeeShape);
        }

        [HttpPut]
        [Route("employees/{id}")]
        [Authorize(Roles = "ADMIN,CLIENT")]
        [IdentityBasicAuthentication]
        public IHttpActionResult UpdateEmployee(int id, EmployeeShape employeeShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != employeeShape.EmployeeId)
            {
                return BadRequest();
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            var emps = db.Employees.Where(e => e.CURP == employee.CURP).ToList();
            foreach (var e in emps)
            {
                // transform to data model
                EmployeeShape.ToDataModel(employeeShape, e);
            }
            //seperate service for password change
            // but for one time updates for auto generated users need to set code
            try
            {
                if (employeeShape.EmployeeStatus == EmployeeStatusType.PasswordAwaitingLocked)
                {
                    EmployeesCode codes = db.EmployeeSecurityCodes.Find(employee.EmployeeId);
                    if (codes != null)
                    {
                        codes.Vcode = EncryptionService.GenerateSecurityCode();
                        codes.GeneratedDate = DateTime.Now;
                    }
                    else
                    {
                        // verify a codes row does not already exist, if not create one, otherwise add new vcode
                        codes = new EmployeesCode() { EmployeeId = employee.EmployeeId, GeneratedDate = DateTime.Now, Prefix = Guid.NewGuid().ToString(), Vcode = EncryptionService.GenerateSecurityCode() };
                        db.EmployeeSecurityCodes.Add(codes);
                    }
                    db.SaveChanges();

                    string customsizedmail = string.Format(@"<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <title>TemplatesNomisign</title>
  <base href=""/"">

  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
</head>
<body bgcolor=""#efefef"" text=""#7e7e7e"" link=""#7e7e7e"" vlink=""#7e7e7e"">
<font face=""verdana"">
<table width=""100%"">
  <tr>
    <th width=""15%""></th>
    <th width=""70%"" bgcolor=""#ffffff"">
      <br>
      <img width=""50%"" src=""http://{0}/nomiadmin/assets/images/Nomi_Sign-12-1-1.png"">
      <br>
      <br>
      <br>
      <h1>Bienvenido a Nomisign&copy;</h1>
      <br>
      <p>
        La empresa #-COMPANY-# utiliza los servicios de la plataforma NomiSign® para que tengas la facilidad de firmar electrónicamente tus recibos de nómina. Da click en este enlace para crear tu contraseña. 
        
      </p>
      <br>
      <div>
        <table width=""100%"" cellpadding=""15px"">
          <tr>
            <th width=""30%""></th>
            <th width=""40%"" bgcolor=""#2cbbc3"">
              <a href=""http://{0}/nomisign/account/#-ID-#"" target=""_blank"">
                Registro
              </a>
            </th>
            <th width=""30%""></th>
          </tr>
        </table>
      </div>
      <br>
        <p>
        Tu código de seguridad es <strong>#-SECCODE-#</strong>
        </p>
      <br>
      <p>O copia y pega la siguiente liga en cualquier navegador:</p>
      <p>http://{0}/nomisign/account/#-ID-#</p>
      <br>
      <br>
    </th>
    <th width=""15%""></th>
  </tr>
  <tr>
    <th></th>
    <th>
      <font size=""1"">
        <code>
        
        </code>
      </font>
    </th>
    <th></th>
  </tr>
</table>
</font>
</body>
</html>
", httpDomain);
                    customsizedmail = customsizedmail.Replace("#-SECCODE-#", codes.Vcode);
                    customsizedmail = customsizedmail.Replace("#-ID-#", employee.EmployeeId.ToString());
                    customsizedmail = customsizedmail.Replace("#-COMPANY-#", employee.Company.CompanyName);
                    string msgBodySpanish = String.Format(Strings.newEmployeeWelcomeMessge, httpDomain, employee.EmployeeId, codes.Vcode);
                    string msgBodyMobile = String.Format(Strings.newEmployeeWelcomeMessgeMobile, employee.Company.CompanyName, httpDomain, employee.EmployeeId, codes.Vcode);
                    if (null != employee.CellPhoneNumber)
                    {
                        //SendSMS.SendSMSMsg(employee.CellPhoneNumber, msgBodyMobile);
                        //SendSMS.SendSMSMsg(employee.CellPhoneNumber, String.Format(Strings.newEmployeeWelcomeMessgeMobileLink, httpDomain, employee.EmployeeId));
                        if (employee.Company.SMSBalance > 0 && employee.Company.TotalSMSPurchased > 0)
                        {
                            string res = "";
                            SendSMS.SendSMSQuiubo(msgBodyMobile, string.Format("+52{0}", employee.CellPhoneNumber), out res);
                            employee.Company.SMSBalance -= 1;
                            db.SaveChanges();
                        }
                        if (employee.Company.SMSBalance <= 10 && employee.Company.TotalSMSPurchased > 0)
                        {
                            try { SendEmail.SendEmailMessage(employee.Company.BillingEmailAddress, string.Format(Strings.smsQuantityWarningSubject), string.Format(Strings.smsQuantityWarning, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch(Exception ex) { log.Error("Error sending Email - " + employee.Company.BillingEmailAddress, ex); }
                            try { SendEmail.SendEmailMessage("mariana.basto@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, employee.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch (Exception ex) { log.Error("Error sending Email - mariana.basto@nomisign.com ", ex); }
                            try { SendEmail.SendEmailMessage("estela.gonzalez@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, employee.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch (Exception ex) { log.Error("Error sending Email - estela.gonzalez@nomisign.com ", ex); }
                            try { SendEmail.SendEmailMessage("info@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, employee.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch (Exception ex) { log.Error("Error sending Email - info@nomisign.com ", ex); }
                            try { SendEmail.SendEmailMessage("artturobldrq@gmail.com", string.Format(Strings.smsWarningSalesMessageSubject, employee.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch (Exception ex) { log.Error("Error sending Email - artturobldrq@gmail.com ", ex); }

                        }
                        //SendSMS.SendSMSQuiubo(String.Format(Strings.newEmployeeWelcomeMessgeMobileLink, httpDomain, employee.EmployeeId), string.Format("+52{0}", employee.CellPhoneNumber), out res2);
                    }

                    SendEmail.SendEmailMessage(employee.EmailAddress, Strings.newEmployeeWelcomeMessgeEmailSubject, customsizedmail);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error sending Msg - Adding employee: " + employeeShape.EmployeeId, ex);
                return BadRequest(ex.Message);
            }

            db.SaveChanges();

            db.CreateLog(OperationTypes.EmployeeUpdated, string.Format("Empleado actualizado {0}", id), 
                    User, id, ObjectTypes.Employee);

            return Ok(employeeShape);
        }

        // POST: api/companyusers
        [HttpPost]
        [Route("employees")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult AddEmployee(EmployeeShape employeeShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Employee employee = EmployeeShape.ToDataModel(employeeShape);
            employee.CreatedDate = DateTime.Now;
            // set last login to non null
            employee.LastLoginDate = DateTime.Now;
            employee.EmployeeStatus = EmployeeStatusType.PasswordAwaitingLocked;
            db.Employees.Add(employee);
            db.SaveChanges();

            // add company agreement doc
            if (null == employee.Company)
            {
                Company company = db.Companies.Find(employee.CompanyId);
                if (null != company)
                {
                    employee.Company = company;
                }
            }

            if (employee.Company.NewEmployeeGetDoc == NewEmployeeGetDocType.AddDocument)
            {
                // need a batch first
                Batch batch = new Batch()
                {
                    CompanyId = employee.CompanyId,
                    BatchOpenTime = DateTime.Now,
                    BatchCloseTime = DateTime.Now,
                    ActualItemCount = 1,
                    ItemCount = 1,
                    WorkDirectory = Guid.NewGuid().ToString()
                };

                var batchPath = db.Batches.Add(batch);
                db.SaveChanges();

                string fileName = NomiFileAccess.CopyCompanyAgreementFileForEmployee(employee.CompanyId,
                   batch.WorkDirectory,  //file name to write
                   employee.Company.NewEmployeeDocument.Trim()); // trim only needed due to DB scheme issue

                if (!String.IsNullOrEmpty(fileName))
                {
                    // TODO: Write file to DB after copied to disk
                    Document document = new Document()
                    {
                        AlwaysShow = 1,
                        BatchId = batch.BatchId,
                        CompanyId = employee.CompanyId,
                        EmployeeId = employee.EmployeeId,
                        PathToFile = fileName,
                        PayperiodDate = DateTime.Now,
                        SignStatus = SignStatus.SinFirma,
                        UploadTime = DateTime.Now
                    };
                    db.Documents.Add(document);
                    db.SaveChanges();
                }
            }
            // try send email
            try
            {
                EmployeesCode codes = new EmployeesCode() { EmployeeId = employee.EmployeeId, GeneratedDate = DateTime.Now, Prefix = Guid.NewGuid().ToString(), Vcode = EncryptionService.GenerateSecurityCode() };
                db.EmployeeSecurityCodes.Add(codes);
                db.SaveChanges();

                string msgBodySpanish = String.Format(Strings.newEmployeeWelcomeMessge,
                    httpDomain, employee.EmployeeId, codes.Vcode);

                SendEmail.SendEmailMessage(employee.EmailAddress, Strings.newEmployeeWelcomeMessgeEmailSubject, msgBodySpanish);

                if (null != employee.CellPhoneNumber)
                {
                    //SendSMS.SendSMSMsg(employee.CellPhoneNumber, msgBodySpanish);
                    if (employee.Company.SMSBalance > 0 && employee.Company.TotalSMSPurchased > 0)
                    {
                        string res = "";
                        SendSMS.SendSMSQuiubo(msgBodySpanish, string.Format("+52{0}", employee.CellPhoneNumber), out res);
                        employee.Company.SMSBalance -= 1;
                        db.SaveChanges();
                        log.Info("res : " + res);
                    }
                    if (employee.Company.SMSBalance <= 10 && employee.Company.TotalSMSPurchased > 0)
                    {
                        try { SendEmail.SendEmailMessage(employee.Company.BillingEmailAddress, string.Format(Strings.smsQuantityWarningSubject), string.Format(Strings.smsQuantityWarning, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch { }
                        try { SendEmail.SendEmailMessage("mariana.basto@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, employee.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch { }
                        try { SendEmail.SendEmailMessage("estela.gonzalez@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, employee.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch { }
                        try { SendEmail.SendEmailMessage("info@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, employee.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch { }
                        try { SendEmail.SendEmailMessage("artturobldrq@gmail.com", string.Format(Strings.smsWarningSalesMessageSubject, employee.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch { }

                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error sending Msg - Adding employee: " + employeeShape.EmployeeId, ex);
                return BadRequest(ex.Message);
            }
            return Ok(EmployeeShape.FromDataModel(employee, Request));
        }

        // POST: api/companyusers
        [HttpPost]
        [Route("employee/verifycell")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult VerifyEmployeePhoneNumber(EmployeeShape employeeShape)
        {
            if (null != employeeShape.CellPhoneNumber)
            {
                try
                {
                    Employee employee = EmployeeShape.ToDataModel(employeeShape);
                    //SendSMS.SendSMSMsg(employeeShape.CellPhoneNumber, Strings.verifyPhoneNumberSMSMessage);
                    log.Info("EmployeeShape celphone : " + employeeShape.CellPhoneNumber);
                    log.Info("verifyPhoneNumberSMSMessage : " + Strings.verifyPhoneNumberSMSMessage);
                    if (employee.Company.SMSBalance > 0 && employee.Company.TotalSMSPurchased > 0)
                    {
                        string res = "";
                        SendSMS.SendSMSQuiubo(Strings.verifyPhoneNumberSMSMessage, string.Format("+52{0}", employeeShape.CellPhoneNumber), out res);
                        employee.Company.SMSBalance -= 1;
                        db.SaveChanges();
                        log.Info("res : " + res);
                    }
                    if (employee.Company.SMSBalance <= 10 && employee.Company.TotalSMSPurchased > 0)
                    {
                        try { SendEmail.SendEmailMessage(employee.Company.BillingEmailAddress, string.Format(Strings.smsQuantityWarningSubject), string.Format(Strings.smsQuantityWarning, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch { }
                        try { SendEmail.SendEmailMessage("mariana.basto@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, employee.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch { }
                        try { SendEmail.SendEmailMessage("estela.gonzalez@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, employee.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch { }
                        try { SendEmail.SendEmailMessage("info@nomisign.com", string.Format(Strings.smsWarningSalesMessageSubject, employee.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch { }
                        try { SendEmail.SendEmailMessage("artturobldrq@gmail.com", string.Format(Strings.smsWarningSalesMessageSubject, employee.Company.CompanyName), string.Format(Strings.smsWarningSalesMessage, httpDomain, employee.Company.CompanyName, employee.Company.SMSBalance)); } catch { }

                    }
                    return Ok("Success");
                }
                catch (Exception ex)
                {
                    log.Error("Error verifying cell number: " + employeeShape.CellPhoneNumber, ex);
                    return Ok("Cell Phone not verified");
                }
            }
            else
            {
                log.Error("Error verifying cell number: " + employeeShape.CellPhoneNumber);
                return BadRequest("Cell Phone Number empty");
            }
        }
        // DELETE: api/companyusers/5
        [HttpDelete]
        [Route("employees/{id}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult DeleteEmployee(int id)
        {
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            db.Employees.Remove(employee);
            db.SaveChanges();
            return Ok();
        }

        [HttpGet]
        [Route("employeesInactive/{id}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult InactiveEmployee(int id)
        {
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            db.Entry(employee).State = System.Data.Entity.EntityState.Modified;
            employee.EmployeeStatus = EmployeeStatusType.NotLongerEmployed;
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