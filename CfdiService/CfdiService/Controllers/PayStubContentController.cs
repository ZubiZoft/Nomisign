using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CfdiService.Controllers
{
    [RoutePrefix("api/upload")]
    public class UploadController : ApiController
    {
        [HttpPost]
        [Route("openbatch/{rfc}")]
        public IHttpActionResult OpenBatch(string rfc, [FromBody] CfdiService.Shapes.OpenBatch batchInfo)
        {
            return Ok();
        }

        [HttpPost]
        [Route("addfile/{batchid}")]
        public IHttpActionResult AddFile(string batchid, [FromBody] CfdiService.Shapes.FileUpload batchInfo)
        {
            return Ok();
        }

        [HttpPost]
        [Route("closebatch/{batchid}")]
        public IHttpActionResult CloseBatch(string batchid)
        {
            return Ok();
        }

/*
        [HttpGet]
        [Route("companies")]
        public IHttpActionResult GetCompanies()
        {
            IList<Company> Companys = new List<Company>();
            Companys.Add(new Company() { CompanyID = 1, DocStoragePath1 = @"C:\cdfi\acme\filestore1", DocStoragePath2 = @"C:\cdfi\acme\filestore2", CompanyRFC = "CAAI951203PR6" });
            Companys.Add(new Company() { CompanyID = 2, DocStoragePath1 = @"C:\cdfi\amazon\filestore1", DocStoragePath2 = @"C:\cdfi\amazon\filestore2", CompanyRFC = "CAAK8833774Z0" });
            Companys.Add(new Company() { CompanyID = 3, DocStoragePath1 = @"C:\cdfi\ibm\filestore1", DocStoragePath2 = @"C:\cdfi\ibm\filestore2", CompanyRFC = "CAAI776644PP1" });
            return Ok<IList<Company>>(Companys);
        }
*/
    }

}
