﻿using CfdiService.Model;
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
        [Route("companyusers/{cid}")]
        public IHttpActionResult GetCompanyUsers(int cid)
        {
            var result = new List<UserListShape>();
            foreach (var c in db.Users)
            {
                if (c.CompanyId == cid) // && c.UserType != UserAdminType.GlobalAdmin)
                {
                    result.Add(UserListShape.FromDataModel(c, Request));
                }
            }
            return Ok(result);
        }

        // GET: api/companyusers/5
        [HttpGet]
        [Route("companyusers/{cid}/{id}")]
        public IHttpActionResult GetCompanyUser(int cid, int id)
        {
            // not validating cid here
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            var userShape = UserShape.FromDataModel(user, Request);

            // hack till i figure out EF
            var createdByUser = db.Users.Find(user.CreatedByUserId);
            if (createdByUser != null) {
                userShape.CreatedByUserName = createdByUser.DisplayName;
            }
            return Ok(userShape);
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
        public IHttpActionResult AddCompanyUser(UserShape userShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            User user = UserShape.ToDataModel(userShape);
            // default these
            user.LastLogin = DateTime.Now;
            user.LastPasswordChange = DateTime.Now;
            user.DateUserCreated = DateTime.Now;
            user.UserStatus = UserStatusType.Active; // may need to change this to unverified once this is an option 

            // TODO: hard coded for now
            // user.CreatedByUserId = 1;
            db.Users.Add(user);
            db.SaveChanges();
            return Ok(UserShape.FromDataModel(user, Request));
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