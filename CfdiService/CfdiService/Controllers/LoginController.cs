using CfdiService.Model;
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CfdiService.Controllers
{
    [RoutePrefix("api")]
    public class LoginController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();


        // POST: api/companyusers
        [HttpGet]
        [Route("login")]
        public IHttpActionResult Ping()
        {
            return Ok();
        }

        // POST: api/companyusers
        [HttpPost]
        [Route("login")]
        public IHttpActionResult DoEmployeeLogin(EmployeeShape employeeShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employeeByEmail = db.Employees.Where(e => e.EmailAddress.Equals(employeeShape.EmailAddress, StringComparison.InvariantCultureIgnoreCase)).ToList();
            foreach (Employee emp in employeeByEmail)
            {
                if (emp.PasswordHash == employeeShape.PasswordHash)
                {
                    emp.LastLogin = DateTime.Now;
                    db.SaveChanges();
                    return Ok(EmployeeShape.FromDataModel(emp, Request));
                }
            }

            return BadRequest("Invalid Login");
        }

        // POST: api/companyusers
        [HttpPost]
        [Route("adminlogin")]
        public IHttpActionResult DoAdminLogin(UserShape userShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userByEmail = db.Users.Where(u => u.EmailAddress.Equals(userShape.EmailAddress, StringComparison.InvariantCultureIgnoreCase)).ToList();
            foreach (User user in userByEmail)
            {
                if (user.PasswordHash == userShape.PasswordHash)
                {
                    user.LastLogin = DateTime.Now;
                    db.SaveChanges();
                    // return DB user.  NOt shape because need user stats as a int
                    return Ok(user);
                }
            }

            return BadRequest("Invalid Login");
        }
    }
}