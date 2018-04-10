using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using Org.BouncyCastle.Tsp;


namespace CfdiService.Services
{
    public class Nom1512017Service
    {
        private static readonly string _reachcoreUser = System.Configuration.ConfigurationManager.AppSettings["ReachcoreUser"];
        private static readonly string _reachcorePassword = System.Configuration.ConfigurationManager.AppSettings["ReachcorePassword"];

        public static string GeneraConstanciaNOM1512017(string pdfpath)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(pdfpath);

            SHA256 sha256 = SHA256Managed.Create();
            byte[] hash = sha256.ComputeHash(bytes);


            TimeStampRequestGenerator tsr = new TimeStampRequestGenerator();
            tsr.SetReqPolicy("1.16.484.101.10.316.1.2");
            TimeStampRequest tsq = tsr.Generate(TspAlgorithms.Sha256, hash, null);

            byte[] tsqbin = tsq.GetEncoded();

            String tsqb64 = Convert.ToBase64String(tsqbin);

            ServiceReference1.AuthSoapHd hdrs = new ServiceReference1.AuthSoapHd();
            hdrs.Clave = "PassW0rd23.";
            hdrs.Entidad = "bancox";
            hdrs.Usuario = "bancox_user2";

            String referencia = "12";

            ServiceReference1.GeneraConstanciaRequest req = new ServiceReference1.GeneraConstanciaRequest(hdrs, referencia, tsqb64);
            ServiceReference1.WebServiceSoap client = new ServiceReference1.WebServiceSoapClient();
            ServiceReference1.GeneraConstanciaResponse response = client.GeneraConstancia(req);
            ServiceReference1.TConstancia cons = response.GeneraConstanciaResult;
            String constancia = cons.Constancia;
            String descripcion = cons.Descripcion;

            if (!string.IsNullOrEmpty(constancia))
                return constancia;
            else
                return null;
        }
    }
}