using CfdiService.Authentication;
using CfdiService.Model;
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
            log.Info("1");
            // 1. Look for credentials in the request.
            HttpRequestMessage request = context.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;
            log.Info("1.1");
            // 2. If there are no credentials, do nothing.
            if (authorization == null)
            {
                log.Info("2");
                return;
            }

            // 3. If there are credentials but the filter does not recognize the 
            //    authentication scheme, do nothing.
            if (authorization.Scheme != "Basic")
            {
                log.Info("3");
                return;
            }
            log.Info("4");
            // 4. If there are credentials that the filter understands, try to validate them.
            // 5. If the credentials are bad, set the error result.
            if (String.IsNullOrEmpty(authorization.Parameter))
            {
                log.Info("4.1");
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                return;
            }
            log.Info("5");
            string token = authorization.Parameter;
            if (token == null)
            {
                log.Info("5.1");
                context.ErrorResult = new AuthenticationFailureResult("Invalid token", request);
            }
            log.Info("6");
            IPrincipal principal = LoginAuthorizeUser(token);
            if (principal == null)
            {
                log.Info("6.1");
                context.ErrorResult = new AuthenticationFailureResult("Invalid username or password", request);
            }
            // 6. If the credentials are valid, set principal.
            else { 
                log.Info("6");

            context.Principal = principal;
            }

        }

        public EmployeePrincipal LoginAuthorizeUser(string token)
        {
            try
            {
                ModelDbContext db = new ModelDbContext();
                Employee employee = db.Employees.FirstOrDefault(e => e.SessionToken.Equals(token));
                EmployeeShape employeeShape = null;
                EmployeeIdentity employeeIdentity = null;
                EmployeePrincipal employeePrincipal = null;
                if (employee != null)
                    employeeShape = EmployeeShape.FromDataModel(employee);

                if (employeeShape != null)
                    employeeIdentity = new EmployeeIdentity(employeeShape);

                if (employeeIdentity != null)
                {
                    employeePrincipal = new EmployeePrincipal();
                    employeePrincipal.Identity = employeeIdentity;
                }

                return employeePrincipal;
            }
            catch (Exception ex){ log.Info(ex.StackTrace); log.Info(ex.Message); log.Info(ex.Source); return null; }
        }
    }
}