using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using Org.BouncyCastle.Tsp;
using CfdiService.Model;

namespace CfdiService.Services
{
    public class Nom1512017Service
    {
        private static readonly string _reachcoreUser = System.Configuration.ConfigurationManager.AppSettings["ReachcoreUser"];
        private static readonly string _reachcorePassword = System.Configuration.ConfigurationManager.AppSettings["ReachcorePassword"];
        private static readonly string _reachcoreEntity = System.Configuration.ConfigurationManager.AppSettings["ReachcoreEntity"];
        private static readonly string _reachcoreReqPolicy = System.Configuration.ConfigurationManager.AppSettings["ReachcoreReqPolicy"];

        public static ConstanciaNOM151 GeneraConstanciaNOM1512017(string pdfpath, Document document)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(pdfpath);

            SHA256 sha256 = SHA256Managed.Create();
            byte[] hash = sha256.ComputeHash(bytes);


            TimeStampRequestGenerator tsr = new TimeStampRequestGenerator();
            tsr.SetReqPolicy(_reachcoreReqPolicy);
            TimeStampRequest tsq = tsr.Generate(TspAlgorithms.Sha256, hash, null);

            byte[] tsqbin = tsq.GetEncoded();

            String tsqb64 = Convert.ToBase64String(tsqbin);

            ServiceReference1.AuthSoapHd hdrs = new ServiceReference1.AuthSoapHd();
            hdrs.Clave = _reachcorePassword;
            hdrs.Entidad = _reachcoreEntity;
            hdrs.Usuario = _reachcoreUser;

            String referencia = string.Format("{0}-{1}", document.DocumentId.ToString(), DateTime.Now.ToString("ddMMyyyyHHmmssfff"));

            ServiceReference1.GeneraConstanciaRequest req = new ServiceReference1.GeneraConstanciaRequest(hdrs, referencia, tsqb64);
            ServiceReference1.WebServiceSoap client = new ServiceReference1.WebServiceSoapClient();
            ServiceReference1.GeneraConstanciaResponse response = client.GeneraConstancia(req);
            ServiceReference1.TConstancia cons = response.GeneraConstanciaResult;
            String constancia = cons.Constancia;
            String descripcion = cons.Descripcion;

            ConstanciaNOM151 con151 = new ConstanciaNOM151(cons.Constancia, cons.Descripcion, cons.Folio.ToString(), cons.Estado.ToString());
            if (!string.IsNullOrEmpty(con151.constancia))
                return con151;
            else
                return null;
        }
    }

    
}