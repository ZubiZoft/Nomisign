using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CfdiService
{
    public static class Strings
    {
        /// <summary>
        /// Previous english strings and teh new one in spanish so i know
        /// </summary>
        // TODO: move these settings to a cache class so it does not get pulled from web.config every time
        //string msgBody = String.Format("Dear {0} {1},\r\n\r\nWecome to the nomisign application.  Please visit the site at http://{2}/nomisign/account/{3} to complete your user setup.\r\n\r\nPlease use the email address of {4} to set up your password!",
        //    employee.FirstName,
        //    employee.LastName1,
        //    httpDomain,
        //    employee.EmployeeId, 
        //    employee.EmailAddress);

        //string smsBody = String.Format("Your user has been created for the Nomisign application.  Please visit the site at http://{0}/nomisign/account/{1} or check email for login deatils",
        //    httpDomain, employee.EmployeeId );

        // Spanish SMS and email message
        public static readonly string newEmployeeWelcomeMessge = "Bienvenido a Nomisign, la aplicación para revisar y firmar sus nominas. Visite este link http://{0}/nomisign/account/{1} para crear su contraseña. Código de seguridad: {2}";
        public static readonly string newEmployeeWelcomeMessgeMobile = "La empresa {0} utiliza los servicios de la plataforma NomiSign para que tengas la facilidad de firmar electrónicamente tus recibos de nómina. Da click en este enlace para crear tu contraseña. http://{1}/nomisign/account/{2} . Tu código de seguridad es {3}.";

        public static readonly string newEmployeeWelcomeMessgeEmailSubject = "Bienvenido a Nomisign";

        //public static readonly string verifyPhoneNumberSMSMessage = "Este mensaje es para comprobar el numero del celular. Por favor compruebe que su CRUP está correcto";

        public static readonly string verifyPhoneNumberSMSMessage = "Este mensaje es para comprobar el numero del celular para NOMISIGN.";
        public static string httpDomain = System.Configuration.ConfigurationManager.AppSettings["signingAppDomain"];

        // Password
        // public static readonly string password = "Contraseña";
        // public static readonly string verifyPassword = "Verificación de Contraseña";

        // "Please visit nomisign site for review of new docs
        public static readonly string visitSiteTosignDocumentSMS = @"La empresa {0} ha colocado una nueva Nómina para el período terminado {1} en Nomisign para que la revises y la aceptes. http://{2}/nomisign/";
        public static readonly string visitSiteTosignDocumentMessage = @"<!doctype html>
<html lang=""en"">
<head>
  <meta charset = ""utf-8"">
  <title>TemplatesNomisign</title>
  <base href=""/"">

  <meta name = ""viewport"" content=""width=device-width, initial-scale=1"">
</head>
<body bgcolor = ""#efefef"" text=""#7e7e7e"" link=""#7e7e7e"" vlink=""#7e7e7e"">
<font face=""verdana"">
<table width = ""100%"">
  <tr>
    <th width = ""15%""></th>
    <th width = ""70%"" bgcolor=""#ffffff"">
      <br>
      <img width = ""50%"" src=""http://{0}/nomiadmin/assets/images/Nomi_Sign-12-1-1.png"">
      <br>
      <br>
      <br>
      <h1>Bienvenido a Nomisign&copy;</h1>
      <br>
      <p>
        La empresa {1}, ha colocado una nueva Nómina para el período terminado {2} en Nomisign para que la revises y la aceptes.
        <br>
        Visite a Nomisign para revisar y firmar sus documentos haciendo click en el siguiente botón.
      </p>
      <br>
      <div>
        <table width = ""100%"" cellpadding=""15px"">
          <tr>
            <th width = ""30%""></th>
            <th width = ""40%"" bgcolor=""#2cbbc3"">
              <a href = ""http://{0}/nomisign/"" target=""_blank"">
                Nomisign
              </a>
            </th>
            <th width = ""30%""></th>
          </tr>
        </table>
      </div>
      <br>
      <p>O copia y pega la siguiente liga en cualquier navegador:</p>
      <p>http://{0}/nomisign/</p>
      <br>
      <br>
    </th>
    <th width = ""15%""></th>
  </tr>
  <tr>
    <th></th>
    <th>
      <font size = ""1"">
        <code>
        
        </code>
      </font>
    </th>
    <th></th>
  </tr>
</table>
</font>
</body>
</html>
";

        public static readonly string restYourAccountMessage = @"<!doctype html>
<html lang=""en"">
<head>
  <meta charset = ""utf-8"">
  <title>TemplatesNomisign</title>
  <base href=""/"">

  <meta name = ""viewport"" content=""width=device-width, initial-scale=1"">
</head>
<body bgcolor = ""#efefef"" text=""#7e7e7e"" link=""#7e7e7e"" vlink=""#7e7e7e"">
<font face=""verdana"">
<table width = ""100%"">
  <tr>
    <th width = ""15%""></th>
    <th width = ""70%"" bgcolor=""#ffffff"">
      <br>
      <img width = ""50%"" src=""http://{0}/nomiadmin/assets/images/Nomi_Sign-12-1-1.png"">
      <br>
      <br>
      <br>
      <h1>Bienvenido a Nomisign&copy;</h1>
      <br>
      <p>
        Tu cuenta ha sido reiniciada.
        <br>
        Visite Nomisign para reiniciar tu contraseña haciendo click en el siguiente botón.  Tu código de seguridad es: {2}
      </p>
      <br>
      <div>
        <table width = ""100%"" cellpadding=""15px"">
          <tr>
            <th width = ""30%""></th>
            <th width = ""40%"" bgcolor=""#2cbbc3"">
              <a href = ""http://{0}/nomisign/account/{1}"" target=""_blank"">
                Nomisign
              </a>
            </th>
            <th width = ""30%""></th>
          </tr>
        </table>
      </div>
      <br>
      <p>O copia y pega la siguiente liga en cualquier navegador:</p>
      <p>http://{0}/nomisign/account/{1}</p>
      <br>
      <br>
    </th>
    <th width = ""15%""></th>
  </tr>
  <tr>
    <th></th>
    <th>
      <font size = ""1"">
        <code>
        
        </code>
      </font>
    </th>
    <th></th>
  </tr>
</table>
</font>
</body>
</html>
";


        public static readonly string smsQuantityWarningSubject = "Advertnecia SMS para la empresa {1} se están agotando.";

        public static readonly string smsQuantityWarning = @"<!doctype html>
<html lang=""en"">
<head>
  <meta charset = ""utf-8"">
  <title>TemplatesNomisign</title>
  <base href=""/"">

  <meta name = ""viewport"" content=""width=device-width, initial-scale=1"">
</head>
<body bgcolor = ""#efefef"" text=""#7e7e7e"" link=""#7e7e7e"" vlink=""#7e7e7e"">
<font face=""verdana"">
<table width = ""100%"">
  <tr>
    <th width = ""15%""></th>
    <th width = ""70%"" bgcolor=""#ffffff"">
      <br>
      <img width = ""50%"" src=""http://{0}/nomiadmin/assets/images/Nomi_Sign-12-1-1.png"">
      <br>
      <br>
      <br>
      <h1>Bienvenido a Nomisign&copy;</h1>
      <br>
      <p>
        Los SMS disponibles para la empresa {1} se están agotando, el numero actual de mensajes disponibles es {2}.
        <br>
        Favor de contactar a su representante de ventas para poder adquirir mas SMS.
      </p>
      <br>
      <div>
        <table width = ""100%"" cellpadding=""15px"">
          <tr>
            <th width = ""30%""></th>
            <th width = ""40%"" bgcolor=""#2cbbc3"">
              <a href = ""http://{0}/nomisign/"" target=""_blank"">
                Nomisign
              </a>
            </th>
            <th width = ""30%""></th>
          </tr>
        </table>
      </div>
      <br>
      
      <br>
      <br>
    </th>
    <th width = ""15%""></th>
  </tr>
  <tr>
    <th></th>
    <th>
      <font size = ""1"">
        <code>
        
        </code>
      </font>
    </th>
    <th></th>
  </tr>
</table>
</font>
</body>
</html>
";

        public static readonly string signatureLicenseQuantityWarningSubject = "Advertnecia las licencias para firma disponibles para la empresa {1} se están agotando.";

        public static readonly string signatureLicenseQuantityWarning = @"<!doctype html>
<html lang=""en"">
<head>
  <meta charset = ""utf-8"">
  <title>TemplatesNomisign</title>
  <base href=""/"">

  <meta name = ""viewport"" content=""width=device-width, initial-scale=1"">
</head>
<body bgcolor = ""#efefef"" text=""#7e7e7e"" link=""#7e7e7e"" vlink=""#7e7e7e"">
<font face=""verdana"">
<table width = ""100%"">
  <tr>
    <th width = ""15%""></th>
    <th width = ""70%"" bgcolor=""#ffffff"">
      <br>
      <img width = ""50%"" src=""http://{0}/nomiadmin/assets/images/Nomi_Sign-12-1-1.png"">
      <br>
      <br>
      <br>
      <h1>Bienvenido a Nomisign&copy;</h1>
      <br>
      <p>
        Las licencias para firma disponibles para la empresa {1} se están agotando, el numero actual de firmas disponibles es {2}.
        <br>
        Favor de contactar a su representante de ventas para poder adquirir mas licencias de firma.
      </p>
      <br>
      <div>
        <table width = ""100%"" cellpadding=""15px"">
          <tr>
            <th width = ""30%""></th>
            <th width = ""40%"" bgcolor=""#2cbbc3"">
              <a href = ""http://{0}/nomisign/"" target=""_blank"">
                Nomisign
              </a>
            </th>
            <th width = ""30%""></th>
          </tr>
        </table>
      </div>
      <br>
      
      <br>
      <br>
    </th>
    <th width = ""15%""></th>
  </tr>
  <tr>
    <th></th>
    <th>
      <font size = ""1"">
        <code>
        
        </code>
      </font>
    </th>
    <th></th>
  </tr>
</table>
</font>
</body>
</html>
";

        public static readonly string signatureLicenseWarningSalesMessageSubject = "Las licencias para firma disponibles para la empresa {1} se están agotando.";

        public static readonly string signatureLicenseWarningSalesMessage = @"<!doctype html>
<html lang=""en"">
<head>
  <meta charset = ""utf-8"">
  <title>TemplatesNomisign</title>
  <base href=""/"">

  <meta name = ""viewport"" content=""width=device-width, initial-scale=1"">
</head>
<body bgcolor = ""#efefef"" text=""#7e7e7e"" link=""#7e7e7e"" vlink=""#7e7e7e"">
<font face=""verdana"">
<table width = ""100%"">
  <tr>
    <th width = ""15%""></th>
    <th width = ""70%"" bgcolor=""#ffffff"">
      <br>
      <img width = ""50%"" src=""http://{0}/nomiadmin/assets/images/Nomi_Sign-12-1-1.png"">
      <br>
      <br>
      <br>
      <h1>Bienvenido a Nomisign&copy;</h1>
      <br>
      <p>
        Las licencias para firma disponibles para la empresa {1} se están agotando, el numero actual de firmas disponibles es {2}.
        <br>
        Favor de contactar a la empresa {1} para que adquieran mas licencias de firma.
      </p>
      <br>
      <div>
        <table width = ""100%"" cellpadding=""15px"">
          <tr>
            <th width = ""30%""></th>
            <th width = ""40%"" bgcolor=""#2cbbc3"">
              <a href = ""http://{0}/nomisign/"" target=""_blank"">
                Nomisign
              </a>
            </th>
            <th width = ""30%""></th>
          </tr>
        </table>
      </div>
      <br>
      
      <br>
      <br>
    </th>
    <th width = ""15%""></th>
  </tr>
  <tr>
    <th></th>
    <th>
      <font size = ""1"">
        <code>
        
        </code>
      </font>
    </th>
    <th></th>
  </tr>
</table>
</font>
</body>
</html>
";

        public static readonly string smsWarningSalesMessageSubject = "Advertnecia SMS para la empresa {1} se están agotando.";

        public static readonly string smsWarningSalesMessage = @"<!doctype html>
<html lang=""en"">
<head>
  <meta charset = ""utf-8"">
  <title>TemplatesNomisign</title>
  <base href=""/"">

  <meta name = ""viewport"" content=""width=device-width, initial-scale=1"">
</head>
<body bgcolor = ""#efefef"" text=""#7e7e7e"" link=""#7e7e7e"" vlink=""#7e7e7e"">
<font face=""verdana"">
<table width = ""100%"">
  <tr>
    <th width = ""15%""></th>
    <th width = ""70%"" bgcolor=""#ffffff"">
      <br>
      <img width = ""50%"" src=""http://{0}/nomiadmin/assets/images/Nomi_Sign-12-1-1.png"">
      <br>
      <br>
      <br>
      <h1>Bienvenido a Nomisign&copy;</h1>
      <br>
      <p>
        Los SMS disponibles para la empresa {1} se están agotando, el numero actual de mensajes disponibles es {2}.
        <br>
        Favor de contactar a su representante de ventas para poder adquirir mas SMS.
      </p>
      <br>
      <div>
        <table width = ""100%"" cellpadding=""15px"">
          <tr>
            <th width = ""30%""></th>
            <th width = ""40%"" bgcolor=""#2cbbc3"">
              <a href = ""http://{0}/nomisign/"" target=""_blank"">
                Nomisign
              </a>
            </th>
            <th width = ""30%""></th>
          </tr>
        </table>
      </div>
      <br>
      
      <br>
      <br>
    </th>
    <th width = ""15%""></th>
  </tr>
  <tr>
    <th></th>
    <th>
      <font size = ""1"">
        <code>
        
        </code>
      </font>
    </th>
    <th></th>
  </tr>
</table>
</font>
</body>
</html>
";

        // ""review your new docs""
        public static readonly string visitSiteTosignDocumentMessageEmailSubject = "Revisar y firmar su documento";

        public static readonly string verifyCurpNumberSMSText = "Este mensaje es para comprobar el numero del celular para NOMISIGN";

        public static readonly string PDF_EXT = ".pdf";
        public static readonly string XML_EXT = ".xml";
    }
}