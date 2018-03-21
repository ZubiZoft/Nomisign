using CfdiService.Filters;
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
        [Route("companyusers/{cid}/{utid}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetCompanyUsers(int cid, int utid)
        {
            var result = new List<UserListShape>();
            UserAdminType type = UserAdminType.Invalid;
            Enum.TryParse<UserAdminType>(utid.ToString(), out type);
            var company = db.Companies.Find(cid);
            if (company == null)
                return Ok(result);
            foreach (var c in db.Users)
            {
                if (c.CompanyId == cid)
                {
                    // filter global admins so only global admins can view / edit
                    if (c.UserType == UserAdminType.GlobalAdmin && type == UserAdminType.GlobalAdmin)
                    {
                        result.Add(UserListShape.FromDataModel(c, Request, company.CompanyName));
                    }
                    else if (c.UserType != UserAdminType.GlobalAdmin)
                    {
                        result.Add(UserListShape.FromDataModel(c, Request, company.CompanyName));
                    }
                }
            }
            return Ok(result);
        }

        // GET: api/companyusers/5
        [HttpGet]
        [Route("companyuser/{cid}/{id}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
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
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
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

            db.CreateLog(OperationTypes.UserCreated, string.Format("Actualización de usuario {0}", user.UserId),
                    User, user.UserId, ObjectTypes.User);

            return Ok(userShape);
        }

        // POST: api/companyusers
        [HttpPost]
        [Route("companyusers")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
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

            db.CreateLog(OperationTypes.UserCreated, string.Format("Nuevo usuario creado {0}", user.UserId), 
                    User, user.UserId, ObjectTypes.User);

            return Ok(UserShape.FromDataModel(user, Request));
        }

        // DELETE: api/companyusers/5
        [HttpDelete]
        [Route("companyusers/{id}")]
        [Authorize(Roles = "ADMIN")]
        [IdentityBasicAuthentication]
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