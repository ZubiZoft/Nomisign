using CfdiService.Model;
using CfdiService.Services;
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CfdiService.Controllers
{
    [RoutePrefix("api")]
    public class ClientUserController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: api/employee/5
        [HttpGet]
        [Route("clientuser/{id}")]
        public IHttpActionResult GetClientUser(int id)
        {
            // not validating company ID here
            ClientUser cUser = db.ClientUsers.Find(id);
            if (cUser == null)
            {
                return NotFound();
            }

            var cUserShape = ClientUserShape.FromDataModel(cUser, Request);
            return Ok(cUserShape);
        }

        [HttpPut]
        [Route("clientuser/password/{id}")]
        public IHttpActionResult UpdateClientUserPassword(int id, ClientUserShape cUserShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != cUserShape.ClientUserID)
            {
                return BadRequest();
            }
            ClientUser cUser = db.ClientUsers.Find(id);
            if (cUser == null)
            {
                return NotFound();
            }
            // transform to data model
            ClientUserShape.ToDataModel(cUserShape, cUser);

            // Get security codes
            //EmployeesCode codes = db.EmployeeSecurityCodes.Find(employee.EmployeeId);

            // IF this is a password reset update, verify code.  dont make changes if not, unless unverified.
            // TODO: add time expiration to vcode
            //if (employee.EmployeeStatus != EmployeeStatusType.Active && employeeShape.SecurityCode != codes.Vcode)
            //{
            //    return BadRequest();
            //}
            //else
            //{
            // password is not set on initial employee creation
            if (!String.IsNullOrEmpty(cUserShape.PasswordHash))
            {
                cUser.PasswordHash = EncryptionService.Sha256_hash(cUserShape.PasswordHash, string.Empty);
            }
                //codes.Vcode = string.Empty;
                //db.SaveChanges(); redundant
            //}

            db.SaveChanges();
            return Ok();
        }

    }
}