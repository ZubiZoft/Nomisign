using CfdiService.Authentication;
using CfdiService.Model;
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
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
            HttpRequestMessage request = context.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;
            // 2. If there are no credentials, do nothing.
            if (authorization == null)
            {
                context.ErrorResult = new AuthenticationFailureResult("Missign Token", request);
                return;
            }

            // 3. If there are credentials but the filter does not recognize the 
            //    authentication scheme, do nothing.
            if (authorization.Scheme != "Basic")
            {
                context.ErrorResult = new AuthenticationFailureResult("Unsupported Scheme", request);
                return;
            }
            // 4. If there are credentials that the filter understands, try to validate them.
            // 5. If the credentials are bad, set the error result.
            if (String.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                return;
            }
            string token = authorization.Parameter;

            IPrincipal principal = LoginAuthorizeUser(token, request.Headers.Referrer.AbsolutePath);
            if (principal == null)
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid token", request);
            }
            // 6. If the credentials are valid, set principal.
            else
            {
                context.Principal = principal;
            }

        }

        public IPrincipal LoginAuthorizeUser(string token, string path)
        {
            try
            {
                ModelDbContext db = new ModelDbContext();
                if (path.ToLower().Contains("nomisign"))
                {
                    Employee employee = db.Employees.FirstOrDefault(e => e.SessionToken.Equals(token) && e.TokenTimeout.GetValueOrDefault().AddMinutes(10) < DateTime.Now);
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