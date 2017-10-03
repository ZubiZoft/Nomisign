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
        [EnableCors(origins: "http://www.ogrean.com", headers: "*", methods: "*")]
        public IHttpActionResult DoEmployeeLogin(EmployeeShape employeeShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            foreach (var employee in db.Employees)
            {
                if (employee.EmailAddress == employeeShape.EmailAddress && employee.PasswordHash == employeeShape.PasswordHash)
                {
                    return Ok(EmployeeShape.FromDataModel(employee, Request));
                }
            }

            return BadRequest("Invalid Login");
        }
    }
}