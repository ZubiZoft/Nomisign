﻿using System;
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
        public static readonly string newEmployeeWelcomeMessgeMobile = "Bienvenido a Nomisign, la aplicación para revisar y firmar sus nominas. Visite el siguiente link para crear su contraseña. Código de seguridad: {0}";
        public static readonly string newEmployeeWelcomeMessgeMobileLink = "http://{0}/nomisign/account/{1}";

        public static readonly string newEmployeeWelcomeMessgeEmailSubject = "Bienvenido a Nomisign";

        //public static readonly string verifyPhoneNumberSMSMessage = "Este mensaje es para comprobar el numero del celular. Por favor compruebe que su CRUP está correcto";

        public static readonly string verifyPhoneNumberSMSMessage = "Este mensaje es para comprobar el numero del celular para NOMISIGN.";

        // Password
        // public static readonly string password = "Contraseña";
        // public static readonly string verifyPassword = "Verificación de Contraseña";

        // "Please visit nomisign site for review of new docs
        public static readonly string visitSiteTosignDocumentSMS = @"Su Patrón {0}, ha colocado una nueva Nómina para el período terminado {1} en Nomisign para que la revise y la acepte. http://18.216.139.244/nomisign/";
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
      <img width = ""50%"" src=""http://18.216.139.244/nomiadmin/assets/images/Nomi_Sign-12-1-1.png"">
      <br>
      <br>
      <br>
      <h1>Bienvenido a Nomisign&copy;</h1>
      <br>
      <p>
        Su Patrón {0}, ha colocado una nueva Nómina para el período terminado {1} en Nomisign para que la revise y la acepte. 
        <br>
        Visite a Nomisign para revisar y firmar sus documentos haciendo click en el siguiente botón.
      </p>
      <br>
      <div>
        <table width = ""100%"" cellpadding=""15px"">
          <tr>
            <th width = ""30%""></th>
            <th width = ""40%"" bgcolor=""#2cbbc3"">
              <a href = ""http://18.216.139.244/nomisign/"" target=""_blank"">
                Nomisign
              </a>
            </th>
            <th width = ""30%""></th>
          </tr>
        </table>
      </div>
      <br>
      <p>O copia y pega la siguiente liga en cualquier navegador:</p>
      <p>http://18.216.139.244/nomisign/</p>
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
        Lorem ipsum dolor sit amet, consectetur adipiscing elit.Nulla quam velit, vulputate eu pharetra nec, mattis ac
       neque.Duis vulputate commodo lectus, ac blandit elit tincidunt id.Sed rhoncus, tortor sed eleifend tristique,
       tortor mauris molestie elit, et lacinia ipsum quam nec dui. Quisque nec mauris sit amet elit iaculis pretium sit
       amet quis magna. Aenean velit odio, elementum in tempus ut, vehicula eu diam.Pellentesque rhoncus aliquam
       mattis.

       Ut vulputate eros sed felis sodales nec vulputate justo hendrerit. Vivamus varius pretium ligula, a aliquam odio
       euismod sit amet. Quisque laoreet sem sit amet orci ullamcorper at ultricies metus viverra.Pellentesque arcu

       mauris, malesuada quis ornare accumsan, blandit sed diam.
       Lorem ipsum dolor sit amet, consectetur adipiscing elit.Nulla quam velit, vulputate eu pharetra nec, mattis ac

       neque.Duis vulputate commodo lectus, ac blandit elit tincidunt id.Sed rhoncus, tortor sed eleifend tristique,
       tortor mauris molestie elit, et lacinia ipsum quam nec dui. Quisque nec mauris sit amet elit iaculis pretium sit
       amet quis magna. Aenean velit odio, elementum in tempus ut, vehicula eu diam.Pellentesque rhoncus aliquam
       mattis.

       Ut vulputate eros sed felis sodales nec vulputate justo hendrerit. Vivamus varius pretium ligula, a aliquam odio
       euismod sit amet. Quisque laoreet sem sit amet orci ullamcorper at ultricies metus viverra.Pellentesque arcu

       mauris, malesuada quis ornare accumsan, blandit sed diam.
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