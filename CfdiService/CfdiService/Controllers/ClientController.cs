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
    public class ClientController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string httpDomain = System.Configuration.ConfigurationManager.AppSettings["signingAppDomain"];
        private ModelDbContext db = new ModelDbContext();

        // TODO: Add Client Company CRUD Servics

        [HttpGet]
        [Route("client/documents/{clientId}")]
        [Authorize(Roles = "CLIENT")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetClientDocuments(int clientId)
        {
            var result = new List<DocumentListShape>();
            var docListResult = db.Documents.Where(x => x.ClientCompanyId == clientId).OrderBy(r => r.CompanyId).ToList();
            foreach (Document doc in docListResult)
            {
                result.Add(DocumentListShape.FromDataModel(doc, Request));
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("clients")]
        [Authorize(Roles = "CLIENT")]
        [IdentityBasicAuthentication]
        public IHttpActionResult GetAllClients()
        {
            var result = new List<string>();
            var clients = db.Clients.ToList();
            foreach (var c in clients)
            {
                result.Add(c.ClientCompanyRFC);
            }
            return Ok(result);
        }
    }
}