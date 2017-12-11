using CfdiService.Model;
using CfdiService.Services;
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
        private readonly string httpDomain = System.Configuration.ConfigurationManager.AppSettings["signingAppDomain"];
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // POST: api/companyusers
        [HttpGet]
        [Route("login")]
        public IHttpActionResult Ping()
        {
            return Ok();
        }

        [HttpPost]
        [Route("login/reset")]
        public IHttpActionResult DoEmployeeLoginReset(EmployeeShape employeeShape)
        {
            var employeeByEmail = db.Employees.Where(e => e.CellPhoneNumber.Equals(employeeShape.CellPhoneNumber, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if(employeeByEmail.Count == 1)
            {
                var employeeForReset = employeeByEmail[0];
                employeeForReset.EmployeeStatus = EmployeeStatusType.PasswordResetLocked;
                db.SaveChanges();
                SendSMS.SendSMSMsg(employeeForReset.CellPhoneNumber, String.Format("Password reset was requested.  Please visit http://{0}/nomisign/account/{1} to reset password", httpDomain, employeeForReset.EmployeeId));
                log.Info("Reset password request for user: " + employeeForReset.EmployeeId);
                return Ok();
            }
            else
            {
                log.Info("Invalid password request for user: " + employeeShape.EmailAddress);
                return BadRequest("Invalid Login Data");
            }
            
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

            Employee emp;
            var employeeByEmail = db.Employees.Where(e => e.CellPhoneNumber.Equals(employeeShape.CellPhoneNumber, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (employeeByEmail.Count != 1)
            {
                log.Info("Invalid login request for user: " + employeeShape.CellPhoneNumber);
                return BadRequest("Invalid Login Data");
            }
            else
            {
                emp = employeeByEmail[0];
            }

            // no null passwords allowed
            if (null != emp.PasswordHash && 
                emp.EmployeeStatus == EmployeeStatusType.Active && 
                emp.PasswordHash == EncryptionService.Sha256_hash(employeeShape.PasswordHash))
            {
                emp.LastLoginDate = DateTime.Now;
                db.SaveChanges();
                // hide password 
                emp.PasswordHash = string.Empty;
                return Ok(EmployeeShape.FromDataModel(emp, Request));
            }

            log.Info("Invalid login request for user: " + employeeShape.CellPhoneNumber + "password: " + emp.PasswordHash);
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

            User user;
            var userByEmail = db.Users.Where(u => u.EmailAddress.Equals(userShape.EmailAddress, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (userByEmail.Count != 1)
            {
                log.Info("Invalid login request for user: " + userShape.EmailAddress);
                return BadRequest("Invalid Login Data");
            }
            else
            {
                user = userByEmail[0];
            }

            if(null != user.PasswordHash && 
                user.UserStatus == UserStatusType.Active && 
                user.PasswordHash == EncryptionService.Sha256_hash(userShape.PasswordHash))
            {
                user.LastLogin = DateTime.Now;
                db.SaveChanges();
                // return DB user.  Not shape because need user stats as a int
                // Dont return password 
                user.PasswordHash = string.Empty;
                return Ok(user);
            }

            log.Info("Invalid login request for user: " + userShape.EmailAddress);
            return BadRequest("Invalid Login");
        }
    }
}