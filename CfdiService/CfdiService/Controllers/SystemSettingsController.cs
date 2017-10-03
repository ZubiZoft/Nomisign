using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CfdiService.Controllers
{
    [RoutePrefix("api")]
    public class SystemSettingsController : ApiController
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: api/signaturepurchases
        [HttpGet]
        [Route("systemsettings")]
        public IHttpActionResult GetSettings()
        {
            var result = new List<SystemSettings>();
            foreach (var c in db.Settings)
            {
                result.Add(c);
            }
            return Ok(result);
        }
    }
}