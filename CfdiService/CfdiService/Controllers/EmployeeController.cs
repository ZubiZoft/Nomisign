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
    public class EmployeeController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: api/employees
        [HttpGet]
        [Route("employees")]
        public IHttpActionResult GetCompanyEmployees()
        {
            var result = new List<EmployeeListShape>();
            foreach (var c in db.Employees)
            {
                result.Add(EmployeeListShape.FromDataModel(c, Request));
            }
            return Ok(result);
        }

        // GET: api/companyusers/5
        [HttpGet]
        [Route("employees/{id}")]
        public IHttpActionResult GetCompanyEmployee(int id)
        {
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