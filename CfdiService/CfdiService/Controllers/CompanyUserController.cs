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
    public class CompanyUserController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: api/companyusers
        [HttpGet]
        [Route("companyusers")]
        public IHttpActionResult GetCompanyUsers()
        {
            var result = new List<UserListShape>();
            foreach (var c in db.Users)
            {
                result.Add(UserListShape.FromDataModel(c, Request));
            }
            return Ok(result);
        }

        // GET: api/companyusers/5
        [HttpGet]
        [Route("companyusers/{id}")]
        public IHttpActionResult GetCompanyUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(UserShape.FromDataModel(user, Request));
        }

        [HttpPut]
        [Route("companyusers/{id}")]
        public IHttpActionResult UpdateCompanyUser(int id, UserShape userShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != userShape.UserId)
            {
                return BadRequest();
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            UserShape.ToDataModel(userShape, user);
            db.SaveChanges();
            return Ok(userShape);
        }

        // POST: api/companyusers
        [HttpPost]
        [Route("companyusers")]
        public IHttpActionResult AddCompany(CompanyShape companyShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Company company = CompanyShape.ToDataModel(companyShape);
            db.Companies.Add(company);
            db.SaveChanges();
            return Ok(CompanyShape.FromDataModel(company, Request));
        }

        // DELETE: api/companyusers/5
        [HttpDelete]
        [Route("companyusers/{id}")]
        public IHttpActionResult DeleteCompanyUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            db.Users.Remove(user);
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