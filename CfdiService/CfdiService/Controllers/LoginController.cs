using CfdiService.Filters;
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
            var employeeByCell = db.Employees.Where(e =>
                    e.CellPhoneNumber.Equals(employeeShape.CellPhoneNumber) ||
                    e.EmailAddress.Equals(employeeShape.CellPhoneNumber)
                ).ToList();
            if (employeeByCell.Count > 0)
            {
                // lock employee account
                foreach (var employeeForReset in employeeByCell)
                {
                    employeeForReset.EmployeeStatus = EmployeeStatusType.PasswordResetLocked;
                    employeeForReset.FailedLoginCount = 0;
                    //db.SaveChanges();

                    // generate code

                    db.SaveChanges();

                    //SendSMS.SendSMSMsg(employeeForReset.CellPhoneNumber, String.Format("Password reset was requested.  Please visit http://{0}/nomisign/account/{1} to reset password.  Security Code: {2}", 
                    //    httpDomain, employeeForReset.EmployeeId, code.Vcode));
                    log.Info("Reset password request for user: " + employeeForReset.EmployeeId);
                }
                var employee1 = employeeByCell[0];
                EmployeesCode code = db.EmployeeSecurityCodes.Find(employee1.EmployeeId);
                if (null == code)
                {
                    code = new EmployeesCode()
                    {
                        Vcode = EncryptionService.GenerateSecurityCode(),
                        GeneratedDate = DateTime.Now,
                        EmployeeId = employee1.EmployeeId,
                        Prefix = Guid.NewGuid().ToString()
                    };
                    db.EmployeeSecurityCodes.Add(code);
                }
                else
                {
                    code.Vcode = EncryptionService.GenerateSecurityCode();
                    code.GeneratedDate = DateTime.Now;
                }
                string res = null;
                try
                {
                    SendSMS.SendSMSQuiubo(String.Format("Tu cuenta ha sido reiniciada.  Por favor, ingresa a http://{0}/nomisign/account/{1} para reiniciar tu contraseña.  Tu código de seguridad es: {2}",
                            httpDomain, employee1.EmployeeId, code.Vcode), string.Format("+52{0}", employee1.CellPhoneNumber), out res);
                }
                catch { }
                try
                {
                    SendEmail.SendEmailMessage(employee1.EmailAddress, "Reinicia tu cuenta", string.Format(Strings.restYourAccountMessage, httpDomain, employee1.EmployeeId, code.Vcode));
                }
                catch { }
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var employeeByPhone = db.Employees.Where(e => e.CellPhoneNumber.Equals(employeeShape.CellPhoneNumber, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (employeeByPhone.Count < 1)
            {
                var employeeByEmail = db.Employees.Where(e => e.EmailAddress.Equals(employeeShape.CellPhoneNumber,
                StringComparison.InvariantCultureIgnoreCase)).ToList();
                if (employeeByEmail.Count < 1)
                {
                    log.Info("Invalid login request for user: " + employeeShape.CellPhoneNumber);
                    var emps = db.Employees.Where(e =>
                            e.EmailAddress.Equals(employeeShape.CellPhoneNumber, StringComparison.InvariantCultureIgnoreCase) ||
                            e.CellPhoneNumber.Equals(employeeShape.CellPhoneNumber, StringComparison.InvariantCultureIgnoreCase)
                        ).Distinct().ToList();
                    log.Info("Records: " + emps.Count().ToString());
                    foreach (var e in emps)
                    {
                        e.FailedLoginCount = e.FailedLoginCount + 1;
                        if (e.FailedLoginCount >= 3)
                        {
                            e.EmployeeStatus = EmployeeStatusType.PasswordFailureLocked;
                        }
                        db.Entry(e).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                    return BadRequest("Invalid Login");
                }
                else
                {
                    // no null passwords allowed
                    foreach (var emp in employeeByEmail)
                    {
                        var code = db.EmployeeSecurityCodes.FirstOrDefault(e => e.EmployeeId == emp.EmployeeId);
                        if (null != emp.PasswordHash &&
                            (emp.EmployeeStatus == EmployeeStatusType.Active || emp.EmployeeStatus == EmployeeStatusType.PasswordFailureLocked) &&
                            emp.PasswordHash == EncryptionService.Sha256_hash(employeeShape.PasswordHash, code.Prefix))
                        {
                            if (emp.FailedLoginCount < 3)
                            {
                                emp.LastLoginDate = DateTime.Now;
                                emp.TokenTimeout = DateTime.Now;
                                emp.SessionToken = TokenGenerator.GetToken();
                                emp.FailedLoginCount = 0;
                                db.SaveChanges();
                                // hide password 
                                emp.PasswordHash = string.Empty;
                                var eShape = EmployeeShape.FromDataModel(emp, Request);
                                //eShape.HasContractToSign = LooksForAUnSignedContract(employeeShape.CellPhoneNumber);
                                return Ok(eShape);
                            }
                            else
                            {
                                return Conflict();
                            }
                        }
                    }
                }
            }
            else
            {
                // no null passwords allowed
                foreach (var emp in employeeByPhone)
                {
                    var code = db.EmployeeSecurityCodes.FirstOrDefault(e => e.EmployeeId == emp.EmployeeId);
                    if (null != emp.PasswordHash &&
                        (emp.EmployeeStatus == EmployeeStatusType.Active || emp.EmployeeStatus == EmployeeStatusType.PasswordFailureLocked) &&
                        emp.PasswordHash == EncryptionService.Sha256_hash(employeeShape.PasswordHash, code.Prefix))
                    {
                        if (emp.FailedLoginCount < 3)
                        {
                            emp.LastLoginDate = DateTime.Now;
                            emp.TokenTimeout = DateTime.Now;
                            emp.SessionToken = TokenGenerator.GetToken();
                            emp.FailedLoginCount = 0;
                            db.SaveChanges();
                            // hide password 
                            emp.PasswordHash = string.Empty;
                            var eShape = EmployeeShape.FromDataModel(emp, Request);
                            //eShape.HasContractToSign = LooksForAUnSignedContract(employeeShape.CellPhoneNumber);
                            return Ok(eShape);
                        }
                        else
                        {
                            return Conflict();
                        }
                    }
                }
            }

            log.Info("Invalid login request for user: " + employeeShape.CellPhoneNumber);
            try
            {
                var empss = db.Employees.Where(e =>
                        e.EmailAddress.Equals(employeeShape.CellPhoneNumber, StringComparison.InvariantCultureIgnoreCase) ||
                        e.CellPhoneNumber.Equals(employeeShape.CellPhoneNumber, StringComparison.InvariantCultureIgnoreCase)
                    ).Distinct().ToList();
                log.Info("Records: " + empss.Count().ToString());
                foreach (var e in empss)
                {
                    e.FailedLoginCount = e.FailedLoginCount + 1;
                    if (e.FailedLoginCount >= 3)
                    {
                        e.EmployeeStatus = EmployeeStatusType.PasswordFailureLocked;
                    }
                    db.Entry(e).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Info(ex);
            }
            return BadRequest("Invalid Login");
        }

        [HttpPost]
        [Route("contracts")]
        [Authorize(Roles = "EMPLOYEE")]
        [IdentityBasicAuthentication]
        public IHttpActionResult HasContractToSign(EmployeeShape employeeShape)
        {
            try
            {
                int result = LooksForAUnSignedContract(employeeShape.CellPhoneNumber);
                return Ok(result);
            }
            catch (Exception e)
            {
                log.Info(e);
                log.Info(e.StackTrace);
                log.Info(e.Message);
                log.Info(e.Source);
            }
            return BadRequest("Server Error");
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
                if (VerifyDefaultAdminAccount(userShape, defaultAdminUser))
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

            if (null != user.PasswordHash &&
                user.UserStatus == UserStatusType.Active &&
                user.PasswordHash == EncryptionService.Sha256_hash(userShape.PasswordHash, string.Empty))
            {
                user.LastLogin = DateTime.Now;
                user.TokenTimeout = DateTime.Now;
                user.SessionToken = TokenGenerator.GetToken();
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
                clientUser.TokenTimeout = DateTime.Now;
                clientUser.SessionToken = TokenGenerator.GetToken();
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
            if (shape.EmailAddress == "admin" && shape.PasswordHash == "password123")
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

        private int LooksForAUnSignedContract(string account)
        {
            var employeeAcc = db.FindEmployeeByAccount(account);

            var commonEmplyees = db.FindEmployeesByCurp(employeeAcc.CURP);

            foreach (var e in commonEmplyees)
            {
                var countDocs = db.CountDocumentsByCompanyNUser(e.CompanyId, e.EmployeeId);
                if (countDocs > 1)
                {
                    continue;
                }
                else
                {
                    var unsignedDocs = db.CountDocumentsNotSignedByCompanyNUser(e.CompanyId, e.EmployeeId);
                    if (unsignedDocs.Count() < 1)
                    {
                        continue;
                    }
                    else
                    {
                        return unsignedDocs[0].DocumentId;
                    }
                }
            }

            return -1;
        }
    }
}