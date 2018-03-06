using CfdiService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CfdiService
{
    /// <summary>
    /// Summary description for Handler2
    /// </summary>
    public class DocumentHandler : IHttpHandler
    {
        private ModelDbContext db = new ModelDbContext();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(HttpContext context)
        {
            var docId = context.Request.QueryString["docid"];
            var id = int.Parse(docId);
            var doc = db.Documents.Where(d => d.DocumentId == id).FirstOrDefault();

            if (doc != null)
            {
                var pathDoc = NomiFileAccess.GetFilePath(doc);
                log.Info("DocumentHandler: " + pathDoc);
                context.Response.ContentType = "application/pdf";

                context.Response.WriteFile(pathDoc);
                return;
            }

            context.Response.ContentType = "text/plain";
            context.Response.Write("File not found");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}