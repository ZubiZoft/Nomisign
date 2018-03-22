using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using CfdiService;
using CfdiService.Model;
using CfdiService.Shapes;
using System.IO;
using CfdiService.Filters;

namespace CfdiService.Controllers
{
    [RoutePrefix("api")]
    public class CompaniesController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: api/companies
        [HttpGet]
        [Route("companies")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetCompanies()
        {
            var result = new List<CompanyListShape>();
            foreach (var c in db.Companies)
            {
                result.Add(CompanyListShape.FromDataModel(c, Request));
            }
            return Ok(result);
        }

        // GET: api/companies/5
        [HttpGet]
        [Route("companies/{id}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetCompany(int id)
        {
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return NotFound();
            }
            return Ok(CompanyShape.FromDataModel(company, Request));
        }

        [HttpPut]
        [Route("companies/{id}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult UpdateCompany(int id, CompanyShape companyShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != companyShape.CompanyId)
            {
                return BadRequest();
            }
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return NotFound();
            }

            CompanyShape.ToDataModel(companyShape, company);
            db.SaveChanges();
            return Ok(companyShape);
        }

        // POST: api/Companies
        [HttpPost]
        [Route("companies")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult AddCompany(CompanyShape companyShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Company company = CompanyShape.ToDataModel(companyShape);
            company.PayPeriod = PayPeriodType.Monthly;
            company.BillingEmailAddress = companyShape.BillingEmailAddress;
            company.ApiKey = Guid.NewGuid().ToString();
            company.AccountStatus = AccountStatusType.Active;
            db.Companies.Add(company);
            db.SaveChanges();
            return Ok(CompanyShape.FromDataModel(company, Request));
        }

        // DELETE: api/Companies/5
        [HttpDelete]
        [Route("companies/{id}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult DeleteCompany(int id)
        {
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return NotFound();
            }
            db.Companies.Remove(company);
            db.SaveChanges();
            return Ok();
        }

        // GET: api/companies/5
        [HttpGet]
        [Route("cleardemo")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult ClearDemo()
        {
            var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext;
            /*truncate table Documents;
            truncate table EmployeesCode;
            truncate table EmployeeSecurityQuestions;
            delete from dbo.Batches;
            delete from Employees*/
            objCtx.ExecuteStoreCommand("truncate table Documents");
            objCtx.ExecuteStoreCommand("truncate table EmployeesCode");
            objCtx.ExecuteStoreCommand("truncate table EmployeeSecurityQuestions");
            objCtx.ExecuteStoreCommand("truncate table Logs");
            objCtx.ExecuteStoreCommand("delete from dbo.Batches");
            objCtx.ExecuteStoreCommand("delete from Employees");
            return Ok("Success");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CompanyExists(int id)
        {
            return db.Companies.Count(e => e.CompanyId == id) > 0;
        }
    }
}