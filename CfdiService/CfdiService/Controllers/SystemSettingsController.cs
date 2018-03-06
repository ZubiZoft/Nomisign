using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

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
            Debug.WriteLine("Hellow !!");
            var result = new List<SystemSettings>();
            foreach (var c in db.Settings)
            {
                result.Add(c);
            }

            if(result.Count == 0)
            {
                var settings = new SystemSettings();
                // cannot be null
                settings.ProductName = "Nomisign";
                settings.SystemFilePath1 = "Nomisign_Files1";
                settings.SystemFilePath2 = "Nomisign_Files2";
                // settings.ProductIcon = "not used!";
                // settings.SupportTelephoneNumber = "(312) 332-9200";
                // initialize system settings
                db.Settings.Add(settings);
                db.SaveChanges();
                result.Add(settings);
            }
            return Ok(result);
        }
    }
}