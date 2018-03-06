using CfdiService.Authentication;
using CfdiService.Model;
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
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
            Debug.WriteLine("1.0");
            HttpRequestMessage request = context.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;
            if (request.Method == HttpMethod.Options)
            {
                Debug.WriteLine("1.0");
                return;
            }
            // 2. If there are no credentials, do nothing.
            Debug.WriteLine("2.0");
            if (authorization == null)
            {
                Debug.WriteLine("2.1");
                context.ErrorResult = new AuthenticationFailureResult("Missign Token", request);
                return;
            }

            // 3. If there are credentials but the filter does not recognize the 
            //    authentication scheme, do nothing.
            Debug.WriteLine("3.0");
            if (authorization.Scheme != "Basic")
            {
                Debug.WriteLine("3.1");
                context.ErrorResult = new AuthenticationFailureResult("Unsupported Scheme", request);
                return;
            }
            // 4. If there are credentials that the filter understands, try to validate them.
            // 5. If the credentials are bad, set the error result.
            Debug.WriteLine("4.0");
            if (String.IsNullOrEmpty(authorization.Parameter))
            {
                Debug.WriteLine("4.1");
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                return;
            }
            string token = authorization.Parameter;

            IPrincipal principal = LoginAuthorizeUser(token, request.Headers.Referrer.AbsolutePath);
            Debug.WriteLine("5.0");
            if (principal == null)
            {
                Debug.WriteLine("5.2");
                context.ErrorResult = new AuthenticationFailureResult("Invalid token", request);
            }
            // 6. If the credentials are valid, set principal.
            else
            {
                Debug.WriteLine("5.3");
                context.Principal = principal;
            }
            Debug.WriteLine("6.0");
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