﻿using CfdiService.Filters;
using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CfdiService.Controllers
{

    [RoutePrefix("api")]
    public class EmployeeSecurityQuestionController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: api/securityquestions/5
        [HttpGet]
        [Route("securityquestions/{id}")]
        public IHttpActionResult GetEmployeeSecurityQuestions(int id)
        {
            EmployeeSecurityQuestions securityQuestions = db.SecurityQuestions.Find(id);
            if (securityQuestions == null)
            {
                return Ok(new EmployeeSecurityQuestions() { userID = id });
            }
            return Ok(securityQuestions);
        }

        [HttpGet]
        [Route("securityQuestionsForgot/{account}")]
        public IHttpActionResult GetEmployeeSecurityQuestionsAcc(string account)
        {
            var emps = db.Employees.Where(e => e.EmailAddress == account).ToList();
            if (emps.Count > 0)
            {
                foreach(var e in emps)
                {
                    var q = db.SecurityQuestions.Find(e.EmployeeId);
                    if (q != null)
                    {
                        q.SecurityAnswer1 = null;
                        q.SecurityAnswer2 = null;
                        q.SecurityAnswer3 = null;
                        return Ok(q);
                    }
                }
            }
            emps = db.Employees.Where(e => e.CellPhoneNumber == account).ToList();
            if (emps.Count > 0)
            {
                foreach (var e in emps)
                {
                    var q = db.SecurityQuestions.Find(e.EmployeeId);
                    if (q != null)
                    {
                        q.SecurityAnswer1 = null;
                        q.SecurityAnswer2 = null;
                        q.SecurityAnswer3 = null;
                        return Ok(q);
                    }
                }
            }
            return BadRequest();
        }

        [HttpPut]
        [Route("securityquestions/{id}")]
        [Authorize(Roles = "EMPLOYEE,CLIENT")]
        [IdentityBasicAuthentication]
        public IHttpActionResult UpdateEmployeeSecurityQuestions(int id, EmployeeSecurityQuestions newSecurityQuestions)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != newSecurityQuestions.userID)
            {
                return BadRequest();
            }

            EmployeeSecurityQuestions securityQuestions = db.SecurityQuestions.Find(id);
            if (securityQuestions == null)
            {
                securityQuestions = new EmployeeSecurityQuestions() { userID = id };
                // since allready queried and did not find, tell entity this is an add
                db.Entry<EmployeeSecurityQuestions>(securityQuestions).State = System.Data.Entity.EntityState.Added;
                db.SecurityQuestions.Add(securityQuestions);
            }

            securityQuestions.SecurityQuestion1 = newSecurityQuestions.SecurityQuestion1;
            securityQuestions.SecurityQuestion2 = newSecurityQuestions.SecurityQuestion2;
            securityQuestions.SecurityQuestion3 = newSecurityQuestions.SecurityQuestion3;
            securityQuestions.SecurityAnswer1 = newSecurityQuestions.SecurityAnswer1;
            securityQuestions.SecurityAnswer2 = newSecurityQuestions.SecurityAnswer2;
            securityQuestions.SecurityAnswer3 = newSecurityQuestions.SecurityAnswer3;
            db.SaveChanges();
            return Ok(securityQuestions);
        }

        // POST: api/companyusers
        [HttpPost]
        [Route("securityquestions")]
        [Authorize(Roles = "EMPLOYEE,CLIENT")]
        [IdentityBasicAuthentication]
        public IHttpActionResult AddSecurityQuestons(EmployeeSecurityQuestions securityQuestions)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // SignaturePurchase purchase = SignaturePurchaseShape.ToDataModel(purchaseShape);
            db.SecurityQuestions.Add(securityQuestions);
            db.SaveChanges();
            return Ok(securityQuestions);
        }

        ////// DELETE: api/companyusers/5
        ////[HttpDelete]
        //[Route("securityquestions/{id}")]
        //public IHttpActionResult DeleteEmployee(int id)
        //{
        //    SignaturePurchase purchase = db.SignaturePurchases.Find(id);
        //    if (purchase == null)
        //    {
        //        return NotFound();
        //    }
        //    db.SignaturePurchases.Remove(purchase);
        //    db.SaveChanges();
        //    return Ok();
        //}

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