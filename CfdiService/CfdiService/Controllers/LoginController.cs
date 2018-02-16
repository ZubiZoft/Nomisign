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
        private static bool allowDefaultAdmin = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static LoginController()
        {
            if (null != System.Configuration.ConfigurationManager.AppSettings["allowDefaultAdmin"])
            {
                // verify to allow global admin account - for new installs
                Boolean.TryParse(System.Configuration.ConfigurationManager.AppSettings["allowDefaultAdmin"], out allowDefaultAdmin);
            }
        }

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
                //db.SaveChanges();

                // generate code
                EmployeesCode code = db.EmployeeSecurityCodes.Find(employeeForReset.EmployeeId);
                if(null == code)
                {
                    code = new EmployeesCode()
                    {
                        Vcode = EncryptionService.GenerateSecurityCode(),
                        GeneratedDate = DateTime.Now,
                        EmployeeId = employeeForReset.EmployeeId,
                        Prefix = Guid.NewGuid().ToString()
                    };
                    db.EmployeeSecurityCodes.Add(code);
                }
                else
                {
                    code.Vcode = EncryptionService.GenerateSecurityCode();
                    code.GeneratedDate = DateTime.Now;
                }
                
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
        [Route("adminlogin/reset")]
        public IHttpActionResult DoAdminLoginReset(UserShape userShape)
        {
            var userByEmail = db.Users.Where(e => e.EmailAddress.Equals(userShape.EmailAddress, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (userByEmail.Count == 1)
            {
                // lock employee account
                var userForReset = userByEmail[0];
                userForReset.UserStatus = UserStatusType.PasswordResetLocked;
                //db.SaveChanges();

                db.SaveChanges();

                SendEmail.SendEmailMessage(userForReset.EmailAddress,
                    "Reset password request for " + userForReset.DisplayName,
                    String.Format("Password reset was requested.  Please visit http://{0}/nomiadmin/account/{1} to reset password.", httpDomain, userForReset.UserId
                    ));
                log.Info("Reset password request for user: " + userForReset.UserId);
                return Ok();
            }
            else
            {
                log.Info("Invalid password request for user: " + userShape.EmailAddress);
                return BadRequest("Invalid Login Data");
            }

        }

        [HttpPost]
        [Route("clientlogin/reset")]
        public IHttpActionResult DoClientLoginReset(UserShape userShape)
        {
            var clientUserByEmail = db.ClientUsers.Where(e => e.EmailAddress.Equals(userShape.EmailAddress, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (clientUserByEmail.Count == 1)
            {
                // lock employee account
                var userForReset = clientUserByEmail[0];
                userForReset.UserStatus = UserStatusType.PasswordResetLocked;
                //db.SaveChanges();

                db.SaveChanges();

                SendEmail.SendEmailMessage(userForReset.EmailAddress,
                    "Reset password request for " + userForReset.DisplayName,
                    String.Format("Password reset was requested.  Please visit http://{0}/nomiadmin/account/{1} to reset password.", httpDomain, userForReset.ClientUserID
                    ));
                log.Info("Reset password request for user: " + userForReset.ClientUserID);
                return Ok();
            }
            else
            {
                log.Info("Invalid password request for user: " + userShape.EmailAddress);
                return BadRequest("Invalid Login Data");
            }

        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult DoEmployeeLogin(EmployeeShape employeeShape)
        {
            log.Info("1");
            if (!ModelState.IsValid)
            {
                log.Info("1.1");
                return BadRequest(ModelState);
            }
            log.Info("2");
            var employeeByPhone = db.Employees.Where(e => e.CellPhoneNumber.Equals(employeeShape.CellPhoneNumber, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (employeeByPhone.Count < 1)
            {
                log.Info("2.1");
                var employeeByEmail = db.Employees.Where(e => e.EmailAddress.Equals(employeeShape.CellPhoneNumber,
                StringComparison.InvariantCultureIgnoreCase)).ToList();
                log.Info("3");
                if (employeeByEmail.Count < 1)
                {
                    log.Info("3.1");
                    log.Info("Invalid login request for user: " + employeeShape.CellPhoneNumber);
                    return BadRequest("Invalid Login Data");
                }
                else
                {
                    log.Info("3.2");
                    // no null passwords allowed
                    foreach (var emp in employeeByEmail)
                    {
                        log.Info("3.3");
                        var code = db.EmployeeSecurityCodes.FirstOrDefault(e => e.EmployeeId == emp.EmployeeId);
                        if (null != emp.PasswordHash &&
                            emp.EmployeeStatus == EmployeeStatusType.Active &&
                            emp.PasswordHash == EncryptionService.Sha256_hash(employeeShape.PasswordHash, code.Prefix))
                        {
                            log.Info("3.4");
                            emp.LastLoginDate = DateTime.Now;
                            db.SaveChanges();
                            // hide password 
                            emp.PasswordHash = string.Empty;
                            return Ok(EmployeeShape.FromDataModel(emp, Request));
                        }
                    }
                }
            }
            else
            {
                log.Info("2.2");
                // no null passwords allowed
                foreach (var emp in employeeByPhone)
                {
                    log.Info("2.3");
                    var code = db.EmployeeSecurityCodes.FirstOrDefault(e => e.EmployeeId == emp.EmployeeId);
                    if (null != emp.PasswordHash &&
                        emp.EmployeeStatus == EmployeeStatusType.Active &&
                        emp.PasswordHash == EncryptionService.Sha256_hash(employeeShape.PasswordHash, code.Prefix))
                    {
                        log.Info("2.4");
                        emp.LastLoginDate = DateTime.Now;
                        db.SaveChanges();
                        // hide password 
                        emp.PasswordHash = string.Empty;
                        return Ok(EmployeeShape.FromDataModel(emp, Request));
                    }
                }
            }

            log.Info("Invalid login request for user: " + employeeShape.CellPhoneNumber + "password: " + employeeShape.PasswordHash);
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

            if (allowDefaultAdmin)
            {
                User defaultAdminUser = new Model.User();
                if(VerifyDefaultAdminAccount(userShape, defaultAdminUser))
                {
                    return Ok(defaultAdminUser);
                }
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
                log.Info("Invalid login request for user (wrong username): " + clientUserShape.EmailAddress);
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

            log.Info("Invalid login request for user (wrong password): " + clientUserShape.EmailAddress);
            return BadRequest("Invalid Login");
        }

        private bool VerifyDefaultAdminAccount(UserShape shape, User adminUser)
        {
            if(shape.EmailAddress == "admin" && shape.PasswordHash == "password123")
            {
                adminUser.UserStatus = UserStatusType.Active;
                adminUser.UserType = UserAdminType.GlobalAdmin;
                adminUser.UserId = 0;
                adminUser.DisplayName = "Administrator";
                adminUser.CompanyId = 0;
                adminUser.EmailAddress = "manager@nomisign.com";
                adminUser.PhoneNumber = "";
                return true;
            }

            return false;
        }
    }
}