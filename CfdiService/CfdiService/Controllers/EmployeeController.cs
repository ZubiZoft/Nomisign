using CfdiService.Model;
using CfdiService.Services;
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace CfdiService.Controllers
{
    [RoutePrefix("api")]
    public class EmployeeController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();
        private readonly string httpDomain = System.Configuration.ConfigurationManager.AppSettings["signingAppDomain"];

        // GET: api/employees
        [HttpGet]
        [Route("employees/{cid}")]
        public IHttpActionResult GetCompanyEmployees(int cid)
        {
            var result = new List<EmployeeListShape>();
            foreach (var c in db.Employees)
            {
                if (c.CompanyId == cid)
                {
                    result.Add(EmployeeListShape.FromDataModel(c, Request));
                }
            }
            return Ok(result);
        }

        // GET: api/companyusers/5
        [HttpGet]
        [Route("employees/{cid}/{id}")]
        public IHttpActionResult GetCompanyEmployee(int id)
        {
            // not validating company ID here
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(EmployeeShape.FromDataModel(employee, Request));
        }

        // GET: api/employee/5
        [HttpGet]
        [Route("employee/{id}")]
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


        [HttpPut]
        [Route("employees/{id}")]
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

            EmployeeShape.ToDataModel(employeeShape, employee);
            db.SaveChanges();
            return Ok(employeeShape);
        }

        // POST: api/companyusers
        [HttpPost]
        [Route("employees")]
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

            // try send email
            try
            {
                // TODO: move these settings to a cache class so it does not get pulled from web.config every time
                string msgBody = String.Format("Dear {0} {1},\r\n\r\nWecome to the nomisign application.  Please visit the site at http://{2}/nomisign/account/{3} to complete your user setup.\r\n\r\nPlease use the email address of {4} to set up your password!",
                    employee.FirstName,
                    employee.LastName1,
                    httpDomain,
                    employee.EmployeeId, 
                    employee.EmailAddress);

                if (null != employee.CellPhoneNumber)
                {
                    string smsBody = String.Format("Your user has been created for the Nomisign application.  Please visit the site at http://{0}/nomisign/account/{1} or check email for login deatils",
                        httpDomain, employee.EmployeeId );
                    SendSMS.SendSMSMsg(employee.CellPhoneNumber, smsBody);
                }

                SendEmail.SendEmailMessage(employee.EmailAddress, "Welcome new user to Nomisign web site", msgBody);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(EmployeeShape.FromDataModel(employee, Request));
        }


        // DELETE: api/companyusers/5
        [HttpDelete]
        [Route("employees/{id}") ]
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