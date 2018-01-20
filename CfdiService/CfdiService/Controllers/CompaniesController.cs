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

namespace CfdiService.Controllers
{
    [RoutePrefix("api")]
    public class CompaniesController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: api/companies
        [HttpGet]
        [Route("companies")]
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
        public IHttpActionResult AddCompany(CompanyShape companyShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Company company = CompanyShape.ToDataModel(companyShape);
            company.PayPeriod = PayPeriodType.Monthly;
            company.BillingEmailAddress = "billing@somewhere.com";
            company.ApiKey = Guid.NewGuid().ToString();
            company.AccountStatus = AccountStatusType.Active;
            db.Companies.Add(company);
            db.SaveChanges();
            return Ok(CompanyShape.FromDataModel(company, Request));
        }

        // DELETE: api/Companies/5
        [HttpDelete]
        [Route("companies/{id}")]
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