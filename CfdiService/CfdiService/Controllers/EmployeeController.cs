using CfdiService.Model;
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
            return Ok(EmployeeShape.FromDataModel(employee, Request));
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
            db.Employees.Add(employee);
            db.SaveChanges();

            // try send email
            try
            {
                string msgBody = String.Format("Dear {0} {1},\r\n\r\nWecome to the nomisign application.  Please visit the site at www.ogrean.com/nomisign to login.\r\n\r\nPlease use the email address of {2} and password {3} to log in the first time!",
                    employee.FirstName, employee.LastName1, employee.EmailAddress, employee.PasswordHash);
                SendUserEmail(employee.EmailAddress, "Welcome new user to Nomisign web site", msgBody);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(EmployeeShape.FromDataModel(employee, Request));
        }

        private void SendUserEmail(string toAddress, string subject, string body)
        {
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.To.Add(toAddress);
            message.To.Add("ted@ogrean.com");
            message.Subject = subject;
            message.From = new System.Net.Mail.MailAddress("postmaster@ogrean.com");
            message.Body = body;
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.ogrean.com");
            //smtp.Credentials = new NetworkCredential("postmaster@ogrean.com", "Maryjo11#");
            smtp.Send(message);
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