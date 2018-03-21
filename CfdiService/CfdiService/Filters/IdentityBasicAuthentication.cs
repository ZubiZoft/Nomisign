using CfdiService.Authentication;
using CfdiService.Model;
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;

namespace CfdiService.Filters
{
    public class IdentityBasicAuthentication : Attribute, IAuthenticationFilter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected static readonly int TIMEOUT_MINUTES = 10;
        protected static readonly int TIMEOUT_MINUTES_ADMIN = 60;

        public bool AllowMultiple { get; }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            var challenge = new AuthenticationHeaderValue("Basic");
            context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
            return Task.FromResult(0);
        }

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            // 1. Look for credentials in the request.
            //log.Info("1.0");
            HttpRequestMessage request = context.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;
            var vals = request.Headers.GetValues("ClientType");
            if (request.Method == HttpMethod.Options)
            {
                //log.Info("1.1");
                return;
            }
            // 2. If there are no credentials, do nothing.
            //log.Info("2.0");
            if (authorization == null)
            {
                //log.Info("2.1");
                context.ErrorResult = new LoginFailureResult("Missign Token", request);
                return;
            }

            // 3. If there are credentials but the filter does not recognize the 
            //    authentication scheme, do nothing.
            //log.Info("3.0");
            if (authorization.Scheme != "Basic")
            {
                //log.Info("3.1");
                context.ErrorResult = new LoginFailureResult("Unsupported Scheme", request);
                return;
            }
            // 4. If there are credentials that the filter understands, try to validate them.
            // 5. If the credentials are bad, set the error result.
            //log.Info("4.0");
            if (String.IsNullOrEmpty(authorization.Parameter))
            {
                //log.Info("4.1");
                context.ErrorResult = new LoginFailureResult("Missing credentials", request);
                return;
            }
            string token = authorization.Parameter;
            //log.Info(token);
            IPrincipal principal = LoginAuthorizeUser(token, vals.FirstOrDefault());
            //log.Info("5.0");
            if (principal == null)
            {
                //log.Info("5.2");
                context.ErrorResult = new LoginFailureResult("Invalid token", request);
            }
            // 6. If the credentials are valid, set principal.
            else
            {
                //log.Info("5.3");
                context.Principal = principal;
            }
            //log.Info("6.0");
        }

        public IPrincipal LoginAuthorizeUser(string token, string clientType)
        {
            try
            {
                log.Info(clientType);
                if (string.IsNullOrEmpty(clientType))
                {
                    return null;
                }
                ModelDbContext db = new ModelDbContext();
                switch (clientType)
                {
                    case "nomisign":
                        Employee employee = db.Employees.FirstOrDefault(e => e.SessionToken.Equals(token) &&
                                System.Data.Entity.DbFunctions.AddMinutes(e.TokenTimeout, TIMEOUT_MINUTES) > DateTime.Now &&
                                e.EmployeeStatus == EmployeeStatusType.Active);
                        if (employee != null)
                        {
                            List<Claim> claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.Name, employee.EmployeeId.ToString()),
                            new Claim(ClaimTypes.Role, "EMPLOYEE")
                        };

                            ClaimsIdentity identity = new ClaimsIdentity(claims, "Basic");

                            return new ClaimsPrincipal(new[] { identity });
                        }
                        break;
                    case "nomiadmin":
                        User user = db.Users.FirstOrDefault(e => e.SessionToken.Equals(token) &&
                                System.Data.Entity.DbFunctions.AddMinutes(e.TokenTimeout, TIMEOUT_MINUTES_ADMIN) > DateTime.Now);
                        if (user != null)
                        {
                            List<Claim> claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.Name, user.UserId.ToString()),
                            new Claim(ClaimTypes.Role, "ADMIN")
                        };

                            ClaimsIdentity identity = new ClaimsIdentity(claims, "Basic");

                            return new ClaimsPrincipal(new[] { identity });
                        }
                        break;
                    case "nomiclient":
                        ClientUser client = db.ClientUsers.FirstOrDefault(e => e.SessionToken.Equals(token) &&
                                System.Data.Entity.DbFunctions.AddMinutes(e.TokenTimeout, TIMEOUT_MINUTES) > DateTime.Now);
                        if (client != null)
                        {
                            List<Claim> claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.Name, client.ClientUserID.ToString()),
                            new Claim(ClaimTypes.Role, "CLIENT")
                        };

                            ClaimsIdentity identity = new ClaimsIdentity(claims, "Basic");

                            return new ClaimsPrincipal(new[] { identity });
                        }
                        break;
                    case "uploader":
                        Company uploader = db.Companies.Where(c => c.ApiKey.Equals(token)).FirstOrDefault();
                        if (uploader != null)
                        {
                            List<Claim> claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.Name, uploader.CompanyId.ToString()),
                            new Claim(ClaimTypes.Role, "UPLOADER")
                        };

                            ClaimsIdentity identity = new ClaimsIdentity(claims, "Basic");

                            return new ClaimsPrincipal(new[] { identity });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Info(ex.StackTrace);
                log.Info(ex.Message);
                log.Info(ex.Source);
            }
            return null;
        }
    }
}