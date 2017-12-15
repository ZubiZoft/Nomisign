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
            var employeeByCell = db.Employees.Where(e => e.CellPhoneNumber.Equals(employeeShape.CellPhoneNumber, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if(employeeByCell.Count == 1)
            {
                // lock employee account
                var employeeForReset = employeeByCell[0];
                employeeForReset.EmployeeStatus = EmployeeStatusType.PasswordResetLocked;
                db.SaveChanges();

                // generate code
                EmployeesCode code = db.EmployeeSecurityCodes.Find(employeeForReset.EmployeeId);
                code.Vcode = EncryptionService.GenerateSecurityCode();
                code.GeneratedDate = DateTime.Now;
                db.SaveChanges();

                SendSMS.SendSMSMsg(employeeForReset.CellPhoneNumber, String.Format("Password reset was requested.  Please visit http://{0}/nomisign/account/{1} to reset password.  Security Code: {2}", 
                    httpDomain, employeeForReset.EmployeeId, code.Vcode));
                log.Info("Reset password request for user: " + employeeForReset.EmployeeId);
                return Ok();
            }
            else
            {
                log.Info("Invalid password request for user: " + employeeShape.EmailAddress);
                return BadRequest("Invalid Login Data");
            }
            
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult DoEmployeeLogin(EmployeeShape employeeShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Employee emp;
            EmployeesCode code;
            var employeeByPhone = db.Employees.Where(e => e.CellPhoneNumber.Equals(employeeShape.CellPhoneNumber, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (employeeByPhone.Count != 1)
            {
                log.Info("Invalid login request for user: " + employeeShape.CellPhoneNumber);
                return BadRequest("Invalid Login Data");
            }
            else
            {
                emp = employeeByPhone[0];
                code = db.EmployeeSecurityCodes.Find(emp.EmployeeId);
                if(null == code)
                {
                    return BadRequest("Employee password data is not found");
                }
            }

            // no null passwords allowed
            if (null != emp.PasswordHash && 
                emp.EmployeeStatus == EmployeeStatusType.Active && 
                emp.PasswordHash == EncryptionService.Sha256_hash(employeeShape.PasswordHash, code.Prefix))
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
                user.PasswordHash == EncryptionService.Sha256_hash(userShape.PasswordHash, string.Empty))
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

        [HttpPost]
        [Route("clientlogin")]
        public IHttpActionResult DoClientLogin(ClientUserShape clientUserShape)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ClientUser clientUser;
            var userByEmail = db.ClientUsers.Where(u => u.EmailAddress.Equals(clientUserShape.EmailAddress, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (userByEmail.Count != 1)
            {
                log.Info("Invalid login request for user: " + clientUserShape.EmailAddress);
                return BadRequest("Invalid Login Data");
            }
            else
            {
                clientUser = userByEmail[0];
            }

            if (null != clientUser.PasswordHash &&
                clientUser.UserStatus == UserStatusType.Active &&
                clientUser.PasswordHash == EncryptionService.Sha256_hash(clientUserShape.PasswordHash, string.Empty))
            {
                clientUser.LastLogin = DateTime.Now;
                db.SaveChanges();
                // return DB user.  Not shape because need user stats as a int
                // Dont return password 
                clientUser.PasswordHash = string.Empty;
                return Ok(clientUser);
            }

            log.Info("Invalid login request for user: " + clientUserShape.EmailAddress);
            return BadRequest("Invalid Login");
        }
    }
}